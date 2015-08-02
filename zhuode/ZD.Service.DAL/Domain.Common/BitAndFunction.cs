using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Dialect.Function;
using NHibernate.Metadata;
using NHibernate.SqlCommand;

namespace ZD.Service.DAL.Domain.Common
{
    public class BitAndFunction : ISQLFunction 
    {
        public bool HasArguments { get { return true; } }
        public bool HasParenthesesIfNoArguments { get { return true; } }
        public NHibernate.Type.IType ReturnType(NHibernate.Type.IType columnType, NHibernate.Engine.IMapping mapping)
        {
            return columnType;
        }

        public SqlString Render(System.Collections.IList args, NHibernate.Engine.ISessionFactoryImplementor factory)
        {
            if (args.Count != 2)
            {
                throw new ArgumentException(
                  "BitAndFunction requires 2 arguments!");
            }

            return new SqlString(string.Format("({0} & {1})", args[0], args[1]));
        }
    }
}
