﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZD.Service.DAL.Domain
{
    public class UserInfo
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public Employee Employee
        {
            get;
            set;
        }
    }
}
