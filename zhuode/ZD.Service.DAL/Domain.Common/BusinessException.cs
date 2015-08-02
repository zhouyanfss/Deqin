using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.Service.DAL.Domain.Common
{
    public class BusinessException : Exception
    {
        public BusinessException(string msg)
            : base(msg)
        {
        }
    }
}
