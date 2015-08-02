using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZD.Service.DAL.Domain;

namespace ZD.Service.BLL
{
    public class UserBLL
    {
        public bool UserSignIn(string userName, string password)
        {
            var repo = RepositoryFactory.Repository;
            var user = repo.Get<UserInfo>(userName);
            if (user == null || user.Password != password)
                return false;
            else
                return true;
        }
    }
}
