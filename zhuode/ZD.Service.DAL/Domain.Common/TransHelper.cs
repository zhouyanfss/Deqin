using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Transactions;
using ZD.Service.DAL.Domain;
using NHibernate;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace ZD.Service.DAL.Domain.Common
{
    public class TransHelper
    {
        public TResult Transact<TResult>(IsolationLevel isolationLevel,int timeOutMinutes, Func<TResult> func)
        {
            var tOpt = new TransactionOptions
            {
                IsolationLevel = isolationLevel,
                Timeout = new TimeSpan(0, timeOutMinutes, 0)
            };

            using (var scope = new TransactionScope(TransactionScopeOption.Required, tOpt))
            {
                var result = func.Invoke();
                scope.Complete();
                return result;
            }
        }

        public TResult Transact<TResult>(Func<TResult> func)
        {
            var tOpt = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = new TimeSpan(0, 2, 0)
            };

            using (var scope = new TransactionScope(TransactionScopeOption.Required, tOpt))
            {
                var result = func.Invoke();
                scope.Complete();
                return result;
            }
        }

        public void Transact(Action action)
        {
            Transact<bool>(() =>
            {
                action.Invoke();
                return false;
            });
        }

        public TResult Transact2<TResult>(IsolationLevel isolationLevel, int timeOutSeconds, Func<TransactionScope, TResult> func)
        {
            var tOpt = new TransactionOptions
            {
                IsolationLevel = isolationLevel,
                Timeout = new TimeSpan(0, 0, timeOutSeconds)
            };

            using (var scope = new TransactionScope(TransactionScopeOption.Required, tOpt))
            {
                var result = func.Invoke(scope);
                //scope.Complete();
                return result;
            }
        }

        public TResult   Transact2<TResult>(Func<TransactionScope, TResult> func)
        {
            var tOpt = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = new TimeSpan(0, 2, 0)
            };

            using (var scope = new TransactionScope(TransactionScopeOption.Required, tOpt))
            {
                var result = func.Invoke(scope);
                //scope.Complete();
                return result;
            }
        }

        public void Transact2(Action<TransactionScope> action)
        {
            Transact2<bool>((scope) =>
            {
                action.Invoke(scope);
                return false;
            });
        }

        public TResult HibernateTransact<TResult>(Func<TResult> func)
        {
            if (!RepositoryFactory.Repository.NHibernateSession.Transaction.IsActive)
            {
                // Wrap in transaction
                using (var tx = RepositoryFactory.Repository.NHibernateSession
                                    .BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    var result = func.Invoke();
                    tx.Commit();
                    return result;
                }
            }
            return func.Invoke();
        }

        public void HibernateTransact(Action action)
        {
            Transact<bool>(() =>
            {
                action.Invoke();
                return false;
            });
        }

        public class TransactionWrapper
        {
            private readonly ITransaction _tx;
            public TransactionWrapper(ITransaction tx)
            {
                _tx = tx;
            }

            public void Complete()
            {
                _tx.Commit();
            }

            public void RollBack()
            {
                _tx.Rollback();
            }

        }

        public TResult HibernateTransact2<TResult>(Func<TransactionWrapper, TResult> func)
        {
            if (!RepositoryFactory.Repository.NHibernateSession.Transaction.IsActive)
            {
                // Wrap in transaction
                using (var tx = RepositoryFactory.Repository.NHibernateSession
                                    .BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    var tw = new TransactionWrapper(tx);
                    var result = func.Invoke(tw);
                    //tx.Commit();
                    return result;
                }
            }
            else
            {
                throw new Exception("Transaction already started.");
            }
        }

        public void HibernateTransact2(Action<TransactionWrapper> action)
        {
            HibernateTransact2<bool>((scope) =>
            {
                action.Invoke(scope);
                return false;
            });
        }

        public TResult SerializableTransact<TResult>(Func<TResult> func)
        {
            var tOpt = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.Serializable,
                Timeout = new TimeSpan(0, 2, 0)
            };

            using (var scope = new TransactionScope(TransactionScopeOption.Required, tOpt))
            {
                var result = func.Invoke();
                scope.Complete();
                return result;
            }
        }

        public void SerializableTransact(Action action)
        {
            Transact<bool>(() =>
            {
                action.Invoke();
                return false;
            });
        }

        public TResult SerializableTransact2<TResult>(Func<TransactionWrapper, TResult> func)
        {
            return SerializableTransact2(RepositoryFactory.Repository, func);
        }

        public TResult SerializableTransact2<TResult>(IRepository repo, Func<TransactionWrapper, TResult> func)
        {
            if (!repo.NHibernateSession.Transaction.IsActive)
            {
                // Wrap in transaction
                using (var tx = repo.NHibernateSession
                                    .BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    var tw = new TransactionWrapper(tx);
                    var result = func.Invoke(tw);
                    //tx.Commit();
                    return result;
                }
            }
            else
            {
                throw new Exception("Transaction already started.");
            }
        }

        public TResult SerializableTransact3<TResult>(Func<TransactionWrapper, TResult> func)
        {
            if (!RepositoryFactory.Repository.NHibernateSession.Transaction.IsActive)
            {
                // Wrap in transaction
                using (var tx = RepositoryFactory.Repository.NHibernateSession
                                    .BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    var tw = new TransactionWrapper(tx);
                    var result = func.Invoke(tw);
                    //tx.Commit();
                    return result;
                }
            }
            else
            {
                return func.Invoke(null);
            }
        }

        public void SerializableTransact2(Action<TransactionWrapper> action)
        {
            HibernateTransact2<bool>((scope) =>
            {
                action.Invoke(scope);
                return false;
            });
        }
    }
}
