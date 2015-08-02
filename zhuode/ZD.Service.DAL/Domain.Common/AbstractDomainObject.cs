using System;
using System.Collections.Generic;
using ZD.Service.DAL.Domain;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Mapping;

namespace ZD.Service.DAL.Domain.Common
{
    public class AbstractDomainObject<TId> : Entity<TId>
    {
        /// <summary>
        /// </summary>
        public AbstractDomainObject()
        { 
        }

        public void Load(TId id)
        {
            Transact(() => NHibernateSession.Load(persitentType, id) );
        }

        public Entity<TId> Get(TId id)
        {
            return Transact(() => (Entity<TId>)NHibernateSession.Get(persitentType, id) );
        }

        /// <summary>
        /// Loads an instance of type T from the DB based on its ID.
        /// </summary>
        public Entity<TId> Get(TId id, bool shouldLock)
        {
            return Transact(() =>
            {
                Entity<TId> entity;

                if (shouldLock)
                {
                    entity = (Entity<TId>)NHibernateSession.Get(persitentType, id, LockMode.Upgrade);
                }
                else
                {
                    entity = (Entity<TId>)NHibernateSession.Get(persitentType, id);
                }

                return entity;
            });
        }

        /// <summary>
        /// Loads every instance of the requested type with no filtering.
        /// </summary>
        public List<Entity<TId>> GetAll()
        {
            return Transact(() => GetByCriteria());
        }

        /// <summary>
        /// HQL query
        /// </summary>
        public IList<Entity<TId>> Query(string qry)
        {
            return Transact(() => NHibernateSession.
                                   CreateQuery(qry).List<Entity<TId>>() as List<Entity<TId>>);
        }

        /// <summary>
        /// HQL query
        /// </summary>
        public IList<Entity<TId>> Query(IQuery qry)
        {
            return Transact(() => qry.List<Entity<TId>>() as List<Entity<TId>>);
        }

        /// <summary>
        /// create IQuery object
        /// </summary>
        public IQuery CreateQuery(string qry)
        {
            return NHibernateSession.CreateQuery(qry);
        }

        public ICriteria CreateCriterion()
        {
            return NHibernateSession.CreateCriteria(persitentType);
        }

        /// <summary>
        /// Loads every instance of the requested type using the supplied <see cref="ICriterion" />.
        /// If no <see cref="ICriterion" /> is supplied, this behaves like <see cref="GetAll" />.
        /// </summary>
        public List<Entity<TId>> GetByCriteria(params ICriterion[] criterion)
        {
            return Transact(() =>
            {
                ICriteria criteria = NHibernateSession.CreateCriteria(persitentType);

                foreach (ICriterion criterium in criterion)
                {
                    criteria.Add(criterium);
                }

                return criteria.List<Entity<TId>>() as List<Entity<TId>>;
            });
        }

        public List<Entity<TId>> GetByExample(Entity<TId> exampleInstance, params string[] propertiesToExclude)
        {
            return Transact(() =>
            {
                ICriteria criteria = NHibernateSession.CreateCriteria(persitentType);
                Example example = Example.Create(exampleInstance);

                foreach (string propertyToExclude in propertiesToExclude)
                {
                    example.ExcludeProperty(propertyToExclude);
                }

                criteria.Add(example);

                return criteria.List<Entity<TId>>() as List<Entity<TId>>;
            });
        }

        /// <summary>
        /// Looks for a single instance using the example provided.
        /// </summary>
        /// <exception cref="NonUniqueResultException" />
        public Entity<TId> GetUniqueByExample(Entity<TId> exampleInstance, params string[] propertiesToExclude)
        {
            return Transact(() =>
            {
                List<Entity<TId>> foundList = GetByExample(exampleInstance, propertiesToExclude);
                if (foundList.Count > 1)
                {
                    throw new NonUniqueResultException(foundList.Count);
                }

                if (foundList.Count > 0)
                {
                    return foundList[0];
                }

                return default(Entity<TId>);
            });
        }

        /// <summary>
        /// For entities that have assigned ID's, you must explicitly call Save to add a new one.
        /// See http://www.hibernate.org/hib_docs/reference/en/html/mapping.html#mapping-declaration-id-assigned.
        /// </summary>
        public void Save()
        {
            Transact(() => NHibernateSession.Save(this));
        }

        /// <summary>
        /// For entities with automatically generated IDs, such as identity, SaveOrUpdate may 
        /// be called when saving a automatically entity.  SaveOrUpdate can also be called to _update_ any 
        /// entity, even if its ID is assigned.
        /// </summary>
        public void SaveOrUpdate()
        {
            Transact(() => NHibernateSession.SaveOrUpdate(this));
        }

        public void Delete()
        {
            Transact(() => NHibernateSession.Delete(this));
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
                using (var tx = NHibernateSession.BeginTransaction())
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
        protected ISession NHibernateSession
        {
            get
            {
                return NHibernateSessionManager.Instance.GetSessionFrom(SimpleSessionFactory.SessionConfig);
            }
        }

        private Type persitentType = typeof(Entity<TId>);
    }

}
