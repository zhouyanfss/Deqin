using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZD.Serivce.Contract;
using ZD.Service.BLL;

namespace ZD.Service.Process
{
    public class UserProcess
    {
        private static UserProcess _userProcess;

        static UserBLL _userBLL = new UserBLL();

        public static UserProcess Instance {
            get { return _userProcess ?? (_userProcess = new UserProcess()); }
        }
        public UserSignInResponse SignIn(UserSignInRequest request)
        {
            var isSignIn = _userBLL.UserSignIn(request.UserName, request.Password);

            if (isSignIn)
            {
                return new UserSignInResponse { IsSuccess = true };
            }
            else
                return new UserSignInResponse { IsSuccess = false, ErrorType = 1, ErrorMessage = "账号密码错误" };
        }
    }
}