using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using ZD.Service.DAL.Domain.Common;
using NHibernate;
using NHibernate.Criterion;

namespace ZD.Service.DAL.Domain
{
    public interface IRepository : IDisposable
    {
        TEntity Load<TEntity>(object id) where TEntity : class;
        TEntity Get<TEntity>(object id);
        TEntity GetForWrite<TEntity>(object id);
        TEntity GetForWrite2<TEntity>(object id, string idName = "Id");
        List<TEntity> GetAll<TEntity>();

        IList Query(string qry);
        IList Query(IQuery qry);
        IList<TEntity> Query<TEntity>(string qry);
        IList<TEntity> Query<TEntity>(IQuery qry);
        IQuery CreateQuery(string qry);
        ICriteria CreateCriterion<TEntity>();
        List<TEntity> GetByCriteria<TEntity>(params ICriterion[] criterion);
        IList RunMultiCriteria(ICriteria[] criterias);

        void Save(IEntity entity);
        void Save<TEntity>(TEntity entity) where TEntity : IEntity;

        void SaveOrUpdate(IEntity entity);
        void SaveOrUpdate<TEntity>(TEntity entity) where TEntity : IEntity;
        void SaveOrUpdateCopy<TEntity>(TEntity entity) where TEntity : IEntity;

        void Update(IEntity entity);
        void Update<TEntity>(TEntity entity) where TEntity : IEntity;

        void UpdateCopy(IEntity entity);
        IEntity UpdateCopy(IEntity entity,bool needReturn);
        void UpdateCopy<TEntity>(TEntity entity) where TEntity : IEntity;

        void Delete<TEntity>(TEntity entity);

        void Close();

        void CommitChanges();

        ISession NHibernateSession { get; }
    }
    
    public class NHibernateRepository : IRepository
    {
        public TEntity Load<TEntity>(object id) where TEntity : class
        {
            return Transact(() => NHibernateSession.Load(typeof(TEntity), id) as TEntity);
        }

        public TEntity Get<TEntity>(object id)
        
        {
            return Transact(() => (TEntity)NHibernateSession.Get(typeof(TEntity), id));
        }

        public TEntity GetForWrite<TEntity>(object id)
        {
            return Transact(() => (TEntity)NHibernateSession.Get(typeof(TEntity), id, LockMode.Write));
        }

        public TEntity GetForWrite2<TEntity>(object id, string idName = "Id")
        {
            return Transact(() =>
                                {
                                    var entityList = CreateCriterion<TEntity>()
                                        .Add(Restrictions.Eq(idName, id))
                                        .SetLockMode(LockMode.Write).List<TEntity>();
                                    return entityList != null && entityList.Count > 0 ? entityList[0] : default(TEntity);
                                });
        }

        /// <summary>
        /// Loads every instance of the requested type with no filtering.
        /// </summary>
        public List<TEntity> GetAll<TEntity>()
        {
            return Transact(() => GetByCriteria<TEntity>());
        }

        /// <summary>
        /// HQL query
        /// </summary>
        public IList<TEntity> Query<TEntity>(string qry)
        {
            return Transact(() => NHibernateSession.
                                      CreateQuery(qry).List<TEntity>() as List<TEntity>);
        }

        /// <summary>
        /// HQL query
        /// </summary>
        public IList<TEntity> Query<TEntity>(IQuery qry)
        {
            return Transact(() => qry.List<TEntity>() as List<TEntity>);
        }

        public IList Query(string qry)
        {
            return Transact(() => NHibernateSession.CreateQuery(qry).List());
        }

        public IList Query(IQuery qry)
        {
            return Transact(() => qry.List() );
        }

        /// <summary>
        /// create IQuery object
        /// </summary>
        public IQuery CreateQuery(string qry)
        {
            return NHibernateSession.CreateQuery(qry);
        }

        /// <summary>
        /// create IQuery object
        /// </summary>
        public IMultiQuery CreateMultiQuery()
        {
            return NHibernateSession.CreateMultiQuery();
        }

        public ICriteria CreateCriterion<TEntity>()
        {
            return NHibernateSession.CreateCriteria(typeof(TEntity));
        }

        public IList RunMultiCriteria(ICriteria[] criterias)
        {
            return Transact(() =>
                                {
                                    IMultiCriteria multiCriteria = NHibernateSession.CreateMultiCriteria();
                                    foreach (var c in criterias)
                                    {
                                        multiCriteria.Add(c);
                                    }
                                    IList results = multiCriteria.List();
                                    return results;
                                });
        }

        /// <summary>
        /// Loads every instance of the requested type using the supplied <see cref="ICriterion" />.
        /// If no <see cref="ICriterion" /> is supplied, this behaves like <see cref="GetAll" />.
        /// </summary>
        public List<TEntity> GetByCriteria<TEntity>(params ICriterion[] criterion)
        {
            return Transact(() => {
                ICriteria criteria = NHibernateSession.CreateCriteria(typeof(TEntity));

                foreach (ICriterion criterium in criterion)
                {
                    criteria.Add(criterium);
                }

                return criteria.List<TEntity>() as List<TEntity>;
            });
        }

        /// <summary>
        /// For entities that have assigned ID's, you must explicitly call Save to add a new one.
        /// See http://www.hibernate.org/hib_docs/reference/en/html/mapping.html#mapping-declaration-id-assigned.
        /// </summary>
        public void Save<TEntity>(TEntity entity) where TEntity : IEntity
        {
            Transact(() =>
            {
                //entity.OnUpdate();
                NHibernateSession.Save(entity);
            });
            return;
        }

        public void Save(IEntity entity)
        {
            Transact(() =>
            {
                //entity.OnUpdate();
                NHibernateSession.Save(entity);
            });
            return;
        }
        
        public void Update(IEntity entity)
        {
            Transact(() =>
            {
                NHibernateSession.Update(entity);
            });
            return;
        }

        public void Update<TEntity>(TEntity entity) where TEntity : IEntity
        {
            Transact(() =>
            {
                NHibernateSession.Update(entity);
            });
            return;
        }

        /// <summary>
        /// For entities with automatically generated IDs, such as identity, SaveOrUpdate may 
        /// be called when saving a automatically entity.  SaveOrUpdate can also be called to _update_ any 
        /// entity, even if its ID is assigned.
        /// </summary>
        public void SaveOrUpdate<TEntity>(TEntity entity)
            where TEntity : IEntity
        {
            Transact(() => {
                //entity.OnUpdate();
                //NHibernateSession.Merge(entity)
                NHibernateSession.SaveOrUpdate(entity);
            });
        }

        public void SaveOrUpdate(IEntity entity)
        {
            Transact(() =>
            {
                //entity.OnUpdate();
                NHibernateSession.SaveOrUpdate(entity);
            });
        }

        public void SaveOrUpdateCopy<TEntity>(TEntity entity)
            where TEntity : IEntity
        {
            Transact(() =>
            {
                //entity.OnUpdate();
                //NHibernateSession.Merge(entity)
                NHibernateSession.SaveOrUpdateCopy(entity);
            });
        }

        public void UpdateCopy<TEntity>(TEntity entity)
            where TEntity : IEntity
        {
            Transact(() =>
            {
                //entity.OnUpdate();
                NHibernateSession.SaveOrUpdateCopy(entity);
            });
        }

        public void UpdateCopy(IEntity entity)
        {
            Transact(() =>
            {
                //entity.OnUpdate();
                NHibernateSession.SaveOrUpdateCopy(entity);
            });
        }

        public IEntity UpdateCopy(IEntity entity,bool needReturn)
        {
            Transact(() =>
            {
                //entity.OnUpdate();
                return NHibernateSession.SaveOrUpdateCopy(entity) as IEntity;
            });

            return NHibernateSession.SaveOrUpdateCopy(entity) as IEntity;
        }

        public void Delete<TEntity>(TEntity entity)
        {
            Transact(() => NHibernateSession.Delete(entity));
        }

        /// <summary>
        /// Commits changes regardless of whether there's an open transaction or not
        /// </summary>
        public void CommitChanges()
        {
            NHibernateSession.Flush();
        }

        private TResult Transact<TResult>(Func<TResult> func)
        {
            if (!NHibernateSession.Transaction.IsActive)
            {
                // Wrap in transaction
                TResult result;
                using (var tx = NHibernateSession.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    result = func.Invoke();
                    tx.Commit();
                }
                return result;
            }
            // Don't wrap;
            return func.Invoke();
        }

        private void Transact(Action action)
        {
            Transact<bool>(() =>
            {
                action.Invoke();
                return false;
            });
        }

        /// <summary>
        /// Exposes the ISession used within the DAO.
        /// </summary>
        public virtual ISession NHibernateSession
        {
            get
            {
                return NHibernateSessionManager.Instance.GetSessionFrom(SimpleSessionFactory.SessionConfig);
            }
        }

        private bool _closed = false;
        public virtual void Close()
        {
            _closed = true;
            NHibernateSessionManager.Instance.CloseSessionOn(SimpleSessionFactory.SessionConfig);
        }

        private bool _disposed = false;
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (!_closed)
                {
                    Close();
                }
            }

            _disposed = true;
        }

        ~NHibernateRepository()
        {
            Dispose(false);
        }
    }

    public class DynamicNHibernateRepository : NHibernateRepository
    {
        private ConnectionInfo _connectionInfo;
        public ConnectionInfo ConnectionInfo { get { return _connectionInfo; } }
        public DynamicNHibernateRepository(ConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        public override ISession NHibernateSession
        {
            get
            {
                return NHibernateSessionManager.Instance.GetSessionFrom(SimpleSessionFactory.DynamicSessionConfig, _connectionInfo);
            }
        }

        public override void Close()
        {
            if (_connectionInfo != null)
            {
                NHibernateSessionManager.Instance.CloseSessionOn(_connectionInfo.ToString());
                _connectionInfo = null;
            }
        }
    }

    public class RepositoryFactory
    {
        public static IRepository Repository
        {
            get { return new NHibernateRepository(); }
        }

        public static void CloseCurrentSession()
        {
            NHibernateSessionManager.Instance.CloseSessionOn(SimpleSessionFactory.SessionConfig);
        }

        public static IRepository GetRepository(ConnectionInfo connection)
        {
            return new DynamicNHibernateRepository(connection);
        }
    }
}