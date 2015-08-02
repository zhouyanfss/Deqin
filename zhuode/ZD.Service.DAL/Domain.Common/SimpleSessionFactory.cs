using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.Service.DAL.Domain.Common
{
    public class SimpleSessionFactory
    {
        protected static string SessionFactoryConfigPath = "";

        public static string SessionConfig
        {
            get { return SessionFactoryConfigPath; }
            set { SessionFactoryConfigPath = value; }
        }

        protected static string DynamicSessionFactoryConfigPath = "";

        public static string DynamicSessionConfig
        {
            get { return DynamicSessionFactoryConfigPath; }
            set { DynamicSessionFactoryConfigPath = value; }
        }

        public static ConnectionInfo DefaultBaseConnection { get; set; }

        public static ConnectionInfo DefaultWmsConnection { get; set; }

        public static ConnectionInfo DefaultEsbConnection { get; set; }
    }

    public class ConnectionInfo
    {
        public string DBServerAddress { get; set; }

        public string DBName { get; set; }

        public string DBCategory { get; set; }

        public string UserId { get; set; }

        public string Password { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}", DBCategory, ToConnectString());
        }

        public string ToConnectString()
        {
            return string.Format("Server={0};initial catalog={1};uid={2};pwd={3}",
                                                 DBServerAddress, DBName, UserId, Password);
        }

        public static ConnectionInfo Parse(string connectString, string category)
        {
            var connection = new ConnectionInfo();
            connection.DBCategory = category;

            var findString = "Server=";
            var beginIndex = connectString.IndexOf(findString);
            var endIndex = connectString.IndexOf(';', beginIndex);
            connection.DBServerAddress = connectString.Substring(beginIndex + findString.Length, endIndex - beginIndex - findString.Length);

            findString = "initial catalog=";
            beginIndex = connectString.IndexOf(findString);
            endIndex = connectString.IndexOf(';', beginIndex);
            connection.DBName = connectString.Substring(beginIndex + findString.Length, endIndex - beginIndex - findString.Length);

            findString = "uid=";
            beginIndex = connectString.IndexOf(findString);
            endIndex = connectString.IndexOf(';', beginIndex);
            connection.UserId = connectString.Substring(beginIndex + findString.Length, endIndex - beginIndex - findString.Length);

            findString = "pwd=";
            beginIndex = connectString.IndexOf(findString);
            endIndex = connectString.Length;
            connection.Password = connectString.Substring(beginIndex + findString.Length, endIndex - beginIndex - findString.Length);

            return connection;
        }
    }
}
