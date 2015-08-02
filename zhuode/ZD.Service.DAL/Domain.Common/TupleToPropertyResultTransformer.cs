using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NHibernate.Transform;

namespace ZD.Service.DAL.Domain.Common
{
    public class TupleToPropertyResultTransformer : IResultTransformer
    {
        private Type result;
        private PropertyInfo[] properties;

        public TupleToPropertyResultTransformer(Type result, params string[] names)
        {
            this.result = result;
            List<PropertyInfo> props = new List<PropertyInfo>();
            foreach (string name in names)
            {
                props.Add(result.GetProperty(name));
            }
            properties = props.ToArray();
        }

        public object TransformTuple(object[] tuple, string[] aliases)
        {
            object instance = Activator.CreateInstance(result);
            for (int i = 0; i < tuple.Length; i++)
            {
                properties[i].SetValue(instance, tuple[i], null);
            }
            return instance;
        }

        public IList TransformList(IList collection)
        {
            return collection;
        }
    }
}
