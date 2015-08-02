using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using ZD.Service.DAL.Domain;

namespace ZD.Service.DAL.Domain.Common
{
    public interface IEntity
    {
        //void OnUpdate();
    }

    public abstract class Entity<TId> : IEntity
    {
       // public readonly Type IdType = typeof(TId);

        public virtual TId Id { get;  set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Entity<TId>);
        }

        private static bool IsTransient(Entity<TId> obj)
        {
            return obj != null &&
                Equals(obj.Id, default(TId));
        }

        private Type GetUnproxiedType()
        {
            return GetType();
        }

        public virtual bool Equals(Entity<TId> other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (!IsTransient(this) &&
                !IsTransient(other) &&
                Equals(Id, other.Id))
            {
                var otherType = other.GetUnproxiedType();
                var thisType = GetUnproxiedType();
                return thisType.IsAssignableFrom(otherType) ||
                otherType.IsAssignableFrom(thisType);
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (Equals(Id, default(TId)))
                return base.GetHashCode();

            return Id.GetHashCode(); 
        }

        //public virtual void OnUpdate()
        //{
        //}

        public virtual IRepository Repository()
        {
            return new NHibernateRepository();
        }
    }

    public interface  IVersionedEntity
    {
    }

    public abstract class VersionedEntity<TId> : Entity<TId>, IVersionedEntity
    {
        public virtual int Version { get; set; }

        public virtual DateTime ModifiedTime { get; set; }

        public virtual string ModifiedBy { get; set; }
    }

    public static class EntityOp
    {
        public static void CopyPropValue<TFrom, TTo>(TFrom @from, TTo to)
        {
            var propsFrom = typeof(TFrom).GetProperties();

            var propsTo = typeof(TTo).GetProperties();

            foreach (var propertyInfoFrom in propsFrom)
            {
                var propertyInfoTo =
                    propsTo.FirstOrDefault(p => p.PropertyType.GUID == propertyInfoFrom.PropertyType.GUID &&
                                                   p.Name == propertyInfoFrom.Name);

                if (propertyInfoTo != null)
                {
                    var fromVal = propertyInfoFrom.GetValue(@from, null);
                    propertyInfoTo.SetValue(to, fromVal, null);
                }
            }
        }
    }

    public static class SysLoginUser
    {
        public static IPrincipal Current
        {
            get
            {
                return HttpContext.Current != null
                    ? HttpContext.Current.User : System.Threading.Thread.CurrentPrincipal;
            }
        }

        public static string CurrentUserName
        {
            get
            {
                return Current == null ? "" : HttpUtility.UrlDecode(Current.Identity.Name);
            }
        }
    }
}
