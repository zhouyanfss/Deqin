using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace ZD.Serivce.Contract
{
    [ServiceContract]
    public interface IUserService
    {
        [OperationContract]
        UserSignInResponse SignIn(UserSignInRequest request);
    }
}
