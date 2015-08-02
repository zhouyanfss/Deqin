using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZD.Service.DAL.Domain.Common;
using NHibernate;
using NHibernate.Criterion;
using ZD.Service.Interface;

namespace ZD.Service.DAL.Domain
{
    public abstract class QueryBase
    {
        public IRepository GetRepository()
        {
            return RepositoryFactory.Repository;
        }

        public List<TEntity> RunPageQuery<TEntity>(ICriteria dataCriteria, ICriteria countCriteria, ref QryPage page)
        {
            IRepository repo = GetRepository();
            return RunPageQuery<TEntity>(repo, dataCriteria, countCriteria, ref page);
        }

        public List<TEntity> RunPageQuery<TEntity>(IRepository repo, ICriteria dataCriteria, ICriteria countCriteria, ref QryPage page)
        {
            IList multiResult = repo.RunMultiCriteria(new []{dataCriteria, countCriteria});

            List<TEntity> result = new List<TEntity>();
            foreach (var o in (IList)multiResult[0])
                result.Add((TEntity)o);

            page.PageCount = (int)Math.Ceiling(((long)((IList)multiResult[1])[0]) / (double)page.PerPageSize);
            page.TotalCount = (int)(long)((IList)multiResult[1])[0];
            return result;
        }

        /// <summary>
        ///  确保分页查询时记录唯一，且此分页的记录个数正确。
        /// </summary>
        public IList<TEntity> RunPageQuery<TId, TEntity>(ICriteria idCriteria, ICriteria countCriteria, ref QryPage page)
        {
            IRepository repo = RepositoryFactory.Repository;
            return RunPageQuery<TId, TEntity>(repo, idCriteria, countCriteria, ref page);
        }

        public IList<TEntity> RunPageQuery<TId, TEntity>(IRepository repo, ICriteria idCriteria, ICriteria countCriteria, ref QryPage page)
        {
            idCriteria.SetProjection(Projections.ProjectionList().Add(Projections.GroupProperty("Id")));

            countCriteria.SetProjection(Projections.CountDistinct("Id"));
            
            IList multiResult = repo.RunMultiCriteria(new [] { idCriteria, countCriteria });

            var idList = new List<TId>();
            foreach (var o in (IList)multiResult[0])
                idList.Add((TId)o);

            page.PageCount = (int)Math.Ceiling(((int)((IList)multiResult[1])[0]) / (double)page.PerPageSize);
            page.TotalCount = (int)(int)((IList)multiResult[1])[0];

            var result = GetListResult<TEntity>(Restrictions.In("Id", idList));

            return result;
        }

        public static IList<TValue> GetListResult<TValue>(params DictionaryEntry[] queryParams)
        {
            var criteria = RepositoryFactory.Repository.CreateCriterion<TValue>();

            foreach (var queryParam in queryParams)
            {
                criteria.Add(Restrictions.Eq(queryParam.Key.ToString(), queryParam.Value));
            }

            return criteria.List<TValue>();
        }

        public static IList<TValue> GetListResult<TValue>(params ICriterion[] queryCriterias)
        {
            var criteria = RepositoryFactory.Repository.CreateCriterion<TValue>();

            foreach (var queryCriteria in queryCriterias)
            {
                criteria.Add(queryCriteria);
            }

            return criteria.List<TValue>();
        }

        public static TValue GetFirstResult<TValue>(params ICriterion[] queryCriterias)
        {
            var list = GetListResult<TValue>(queryCriterias);

            return list.Count == 0 ? default(TValue) : list[0];
        }

        public static IList<TValue> GetListResult<TValue>(IRepository repo, params ICriterion[] queryCriterias)
        {
            var criteria = repo.CreateCriterion<TValue>();

            foreach (var queryCriteria in queryCriterias)
            {
                criteria.Add(queryCriteria);
            }

            return criteria.List<TValue>();
        }

        public static TValue GetFirstResult<TValue>(IRepository repo, params ICriterion[] queryCriterias)
        {
            var list = GetListResult<TValue>(repo, queryCriterias);

            return list.Count == 0 ? default(TValue) : list[0];
        }

        //public List<TEntity> RunPageResponseQuery<TEntity>(ICriteria dataCriteria, ICriteria countCriteria, PageResponse page)
        //{
        //    IRepository repo = GetRepository();
        //    IList multiResult = repo.RunMultiCriteria(new ICriteria[] { dataCriteria, countCriteria });

        //    List<TEntity> result = new List<TEntity>();
        //    foreach (var o in (IList)multiResult[0])
        //        result.Add((TEntity)o);

        //    page.TotalPages = (int)Math.Ceiling(((long)((IList)multiResult[1])[0]) / (double)page.PageSize);
        //    page.TotalCount = (int)(long)((IList)multiResult[1])[0];
        //    return result;
        //}
    }


    //public interface IQueryBase
    //{
    //}

    //public interface IQueryBase<TResult> : IQueryBase
    //{
    //    TResult Execute();
    //}

    //public abstract class NHibernateQueryBase<TResult> : IQueryBase<TResult>
    //{
    //    NHibernateQueryBase()
    //    { }

    //    public abstract TResult Execute();

    //    protected ISession NHibernateSession
    //    {
    //        get
    //        {
    //            return NHibernateSessionManager.Instance.GetSessionFrom(SessionFactorySimple.SessionConfig);
    //        }
    //    }
    //}

    //public class EntityLoader<TId> 
    //{
    //    public Entity<TId> Load(TId id)
    //    {
    //        return NHibernateSession.Load<Entity<TId> >(id);
    //    }

    //    protected ISession NHibernateSession
    //    {
    //        get
    //        {
    //            return NHibernateSessionManager.Instance.GetSessionFrom(SessionFactorySimple.SessionConfig);
    //        }
    //    }
    //}
}