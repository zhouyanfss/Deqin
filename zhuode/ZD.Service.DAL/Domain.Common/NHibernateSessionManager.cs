using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Xml;
using ZD.Service.DAL.Domain.Common;
using NHibernate;
using NHibernate.Cache;
using NHibernate.Cfg;
using NHibernate.Event;
using ZD.Utils;

namespace ZD.Service.DAL.Domain
{
    /// <summary>
    /// Handles creation and management of sessions and 
    /// transactions.  It is a singleton because 
    /// building the initial session factory is very expensive. 
    /// </summary>
    public sealed class NHibernateSessionManager
    {
        #region Thread-safe, lazy Singleton

        /// <summary>
        /// This is a thread-safe, lazy singleton. 
        /// </summary>
        public static NHibernateSessionManager Instance
        {
            get
            {
                return Nested.NHibernateSessionManager;
            }
        }

        /// <summary>
        /// Private constructor to enforce singleton
        /// </summary>
        private NHibernateSessionManager() { }

        /// <summary>
        /// Assists with ensuring thread-safe, lazy singleton
        /// </summary>
        private class Nested
        {
            static Nested() { }
            internal static readonly NHibernateSessionManager NHibernateSessionManager = new NHibernateSessionManager();
        }

        #endregion

        /// <summary>
        /// This method attempts to find a session factory stored in <see cref="sessionFactories" />
        /// via its name; if it can't be found it creates a new one and adds it the hashtable.
        /// </summary>
        /// <param name="sessionFactoryConfigPath">Path location of the factory config</param>
        private ISessionFactory GetSessionFactoryFor(string sessionFactoryConfigPath)
        {
            Check.Require(!string.IsNullOrEmpty(sessionFactoryConfigPath),
                          "_sessionFactoryConfigPath may not be null nor empty");

            //  Attempt to retrieve a stored SessionFactory from the hashtable.
            ISessionFactory sessionFactory = (ISessionFactory)sessionFactories[sessionFactoryConfigPath];

            //  Failed to find a matching SessionFactory so make a new one.
            if (sessionFactory == null)
            {
                Check.Require(File.Exists(sessionFactoryConfigPath),
                              "The config file at '" + sessionFactoryConfigPath + "' could not be found");

                Configuration cfg = new Configuration();
                cfg.Configure(sessionFactoryConfigPath);

                //  Now that we have our Configuration object, create a new SessionFactory
                sessionFactory = cfg.BuildSessionFactory();

                if (sessionFactory == null)
                {
                    throw new InvalidOperationException("cfg.BuildSessionFactory() returned null.");
                }

                sessionFactories.Add(sessionFactoryConfigPath, sessionFactory);
            }

            return sessionFactory;
        }

        private static IDictionary<string, IDictionary<string, string>> _dynamicConfig = new Dictionary<string, IDictionary<string, string>>();

        public static void LoadDynamicConfig(string dynamicSessionFactoryConfigPath)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(dynamicSessionFactoryConfigPath);
            var elemSfs = xmlDoc.GetElementsByTagName("session-factory");

            foreach (XmlNode elemSf in elemSfs)
            {
                if (string.IsNullOrEmpty(elemSf.Attributes["name"].Value))
                    continue;

                if (_dynamicConfig.ContainsKey(elemSf.Attributes["name"].Value))
                {
                    //throw new InvalidDataException("The dynamic config has the same factory name");
                    continue;
                }

                var props = new Dictionary<string, string>();

                props.Add("session_factory_name", elemSf.Attributes["name"].Value);

                foreach (XmlNode elemProp in elemSf.ChildNodes)
                {
                    if (elemProp is XmlComment)
                        continue;

                    if (elemProp.Name == "property")
                    {
                        if (!props.ContainsKey(elemProp.Attributes["name"].Value))
                        {
                            props.Add(elemProp.Attributes["name"].Value, elemProp.InnerText);
                        }
                    }
                    else if (elemProp.Name == "mapping")
                    {
                        props.Add("assembly", elemProp.Attributes["assembly"].Value);
                    }
                }

                _dynamicConfig.Add(elemSf.Attributes["name"].Value, props);
            }
        }

        private ISessionFactory GetSessionFactoryFor(string dynamicSessionFactoryConfigPath, ConnectionInfo connectionInfo)
        {
            Check.Require(!string.IsNullOrEmpty(dynamicSessionFactoryConfigPath),
                          "dynamicSessionFactoryConfigPath may not be null nor empty");

            var connectionString = connectionInfo.ToConnectString();
            var connectionKey = connectionInfo.ToString();

            //  Attempt to retrieve a stored SessionFactory from the hashtable.
            ISessionFactory sessionFactory = (ISessionFactory)sessionFactories[connectionKey];

            //  Failed to find a matching SessionFactory so make a new one.
            if (sessionFactory == null)
            {
                Check.Require(File.Exists(dynamicSessionFactoryConfigPath),
                              "The config file at '" + dynamicSessionFactoryConfigPath + "' could not be found");

                Configuration cfg = new Configuration();

                if (_dynamicConfig.Count == 0)
                {
                    LoadDynamicConfig(dynamicSessionFactoryConfigPath);
                }

                if (!_dynamicConfig.ContainsKey(connectionInfo.DBCategory))
                {
                    throw new InvalidOperationException("cfg.BuildSessionFactory() returned null.");
                }

                var props = new Dictionary<string, string>(_dynamicConfig[connectionInfo.DBCategory]);

                props.Add("connection.connection_string", connectionString);
                cfg.AddProperties(props);
                cfg.AddAssembly(props["assembly"].Trim());

                //  Now that we have our Configuration object, create a new SessionFactory
                sessionFactory = cfg.BuildSessionFactory();

                if (sessionFactory == null)
                {
                    throw new InvalidOperationException("cfg.BuildSessionFactory() returned null.");
                }

                sessionFactories.Add(connectionKey, sessionFactory);
            }

            return sessionFactory;
        }

        /// <summary>
        /// Allows you to register an interceptor on a new session.  This may not be called if there is already
        /// an open session attached to the HttpContext.  If you have an interceptor to be used, modify
        /// the HttpModule to call this before calling BeginTransaction().
        /// </summary>
        public void RegisterInterceptorOn(string sessionFactoryConfigPath, IInterceptor interceptor)
        {
            ISession session = (ISession)ContextSessions[sessionFactoryConfigPath];

            if (session != null && session.IsOpen)
            {
                Utils.Logger.Info("You cannot register an interceptor once a session has already been opened");
                throw new CacheException("You cannot register an interceptor once a session has already been opened");
            }

            GetSessionFrom(sessionFactoryConfigPath, interceptor);
        }

        public ISession GetSessionFrom(string sessionFactoryConfigPath)
        {
            return GetSessionFrom(sessionFactoryConfigPath, new PersistenceInterceptor());
        }

        public ISession GetSessionFrom(string dynamicSessionFactoryConfigPath, ConnectionInfo connectionInfo)
        {
            return GetSessionFrom(dynamicSessionFactoryConfigPath, connectionInfo ,new PersistenceInterceptor());
        }

        /// <summary>
        /// Gets a session with or without an interceptor.  This method is not called directly; instead,
        /// it gets invoked from other public methods.
        /// </summary>
        private ISession GetSessionFrom(string sessionFactoryConfigPath, IInterceptor interceptor)
        {
            ISession session = (ISession)ContextSessions[sessionFactoryConfigPath];

            if (session == null)
            {
                if (interceptor != null)
                {
                    session = GetSessionFactoryFor(sessionFactoryConfigPath).OpenSession(interceptor);
                }
                else
                {
                    session = GetSessionFactoryFor(sessionFactoryConfigPath).OpenSession();
                }

                if (session == null)
                    Utils.Logger.Fatal("get session failed", true);

                session.FlushMode = FlushMode.Commit;

                ContextSessions[sessionFactoryConfigPath] = session;
            }

            return session;
        }

        private ISession GetSessionFrom(string sessionFactoryConfigPath, ConnectionInfo connectionInfo, IInterceptor interceptor)
        {
            ISession session = (ISession)ContextSessions[connectionInfo.ToString()];

            if (session == null)
            {
                if (interceptor != null)
                {
                    session = GetSessionFactoryFor(sessionFactoryConfigPath, connectionInfo).OpenSession(interceptor);
                }
                else
                {
                    session = GetSessionFactoryFor(sessionFactoryConfigPath, connectionInfo).OpenSession();
                }

                if (session == null)
                    Utils.Logger.Fatal("get session failed", true);

                session.FlushMode = FlushMode.Commit;

                ContextSessions[connectionInfo.ToString()] = session;
            }

            return session;
        }

        /// <summary>
        /// Flushes anything left in the session and closes the connection.
        /// </summary>
        public void CloseSessionOn(string sessionInfo)
        {
            ISession session = (ISession)ContextSessions[sessionInfo];

            if (session != null)
            {
                if (session.IsOpen)
                {
                    session.Close();
                    session.Dispose();
                }

                ContextSessions.Remove(sessionInfo);
            }
        }

        /// <summary>
        /// Since multiple databases may be in use, there may be one session per database 
        /// persisted at any one time.  The easiest way to store them is via a hashtable
        /// with the key being tied to session factory.  If within a web context, this uses
        /// <see cref="HttpContext" /> instead of the WinForms specific <see cref="CallContext" />.  
        /// Discussion concerning this found at http://forum.springframework.net/showthread.php?t=572
        /// </summary>
        private Hashtable ContextSessions
        {
            get
            {
                if (IsInWebContext())
                {
                    if (HttpContext.Current.Items[SESSION_KEY] == null)
                        HttpContext.Current.Items[SESSION_KEY] = new Hashtable();

                    return (Hashtable)HttpContext.Current.Items[SESSION_KEY];
                }
                else
                {
                    if (CallContext.GetData(SESSION_KEY) == null)
                        CallContext.SetData(SESSION_KEY, new Hashtable());

                    return (Hashtable)CallContext.GetData(SESSION_KEY);
                }
            }
        }

        private bool IsInWebContext()
        {
            return HttpContext.Current != null;
        }

        private Hashtable sessionFactories = new Hashtable();
        private const string SESSION_KEY = "CONTEXT_SESSIONS";
    }
}