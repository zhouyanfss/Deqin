using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Dialect;

namespace ZD.Service.DAL.Domain.Common
{
    public class CustomSQLDialect : MsSql2008Dialect
    {
        public CustomSQLDialect()
        {
            RegisterFunction("bitand", new BitAndFunction());
        } 
    }
}
