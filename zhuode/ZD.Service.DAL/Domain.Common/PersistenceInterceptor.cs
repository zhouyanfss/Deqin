using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZD.Service.DAL.Domain.Common;
using NHibernate;
using NHibernate.Event;
using NHibernate.Event.Default;
using NHibernate.Mapping;
using Array = System.Array;

namespace ZD.Service.DAL.Domain
{
     // public class PersistenceInterceptor : EmptyInterceptor

    public class PersistenceInterceptor : EmptyInterceptor
    {

        public override bool OnFlushDirty(object entity, object id, object[] currentState,
              object[] previousState, string[] propertyNames,
              global::NHibernate.Type.IType[] types)
        {
            if (entity is IVersionedEntity)
            {
                SetVersion(propertyNames, currentState);

               return true;
            }
            return false;
        }
    

        public override bool OnSave(object entity, object id, object[] state,
          string[] propertyNames, global::NHibernate.Type.IType[] types)
        {
            if (entity is IVersionedEntity)
            {
                SetVersion(propertyNames, state);
                return true;
            }
            return false;
        }

        private void SetVersion(string[] propertyNames, object[] state)
        {
            int index = Array.IndexOf(propertyNames, "ModifiedTime");
            if (index >= 0)
            {
                state[index] = DateTime.Now;

                state[Array.IndexOf(propertyNames, "ModifiedBy")] = SysLoginUser.CurrentUserName;
            }
        }


    }
}
