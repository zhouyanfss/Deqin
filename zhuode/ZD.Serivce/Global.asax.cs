using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using ZD.Service.DAL.Domain.Common;
using ZD.Service.DAL.Domain;


namespace ZD.Service
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            SimpleSessionFactory.SessionConfig = System.AppDomain.CurrentDomain.BaseDirectory + "\\hibernate.cfg.xml";
            NHibernate.ISession ss = NHibernateSessionManager.Instance.GetSessionFrom(SimpleSessionFactory.SessionConfig);

        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}