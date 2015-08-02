using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Configuration;
using System.Diagnostics;

namespace ZD.Utils
{
    /// <summary>
    /// logger 
    /// </summary>
    public class Logger
    {
        private static readonly string LOGGER = "ZD.Logging";
        public static Action<ExceptionInfo> OnLogExp = null;

        public static ILog GetLogger(string name)
        {
            return LogManager.GetLogger(name);
        }

        public static ILog Benchmark
        {
            get { return GetLogger("ZD.Benchmark"); }
        }

        public static void Info(string msg)
        {
            LogManager.GetLogger(LOGGER).Info(msg);
        }

        public static void Debug(string msg)
        {
            LogManager.GetLogger(LOGGER).Debug(msg);
        }

        public static void Warn(string msg)
        {
            LogManager.GetLogger(LOGGER).Warn(msg);
        }

        public static void Fatal(string msg)
        {
            LogManager.GetLogger(LOGGER).Fatal(msg);
        }

        public static void Info(string msg, bool btrace)
        {
            string info = msg;
            if (btrace)
                info = info + getTraceInfo();
            Info(info);
        }

        public static void Debug(string msg, bool btrace)
        {
            string info = msg;
            if (btrace)
                info = info + getTraceInfo();
            Debug(info);
        }

        public static void Warn(string msg, bool btrace)
        {
            string info = msg;
            if (btrace)
                info = info + getTraceInfo();
            Warn(info);
        }

        public static void Fatal(string msg, bool btrace)
        {
            string info = msg;
            if (btrace)
                info = info + getTraceInfo();
            Fatal(info);
        }

        public static void Fatal(Exception e)
        {
            LogExp(e);
        }

        public static void Fatal(string msg, Exception e)
        {
            LogExp(msg, e);
        }

        public static void LogExp(Exception e)
        {
            StringBuilder error = new StringBuilder(256);

            LogExp(error, e);
        }

        public static void LogExp(string msg, Exception e)
        {
            StringBuilder error = new StringBuilder(msg, 256);

            LogExp(error, e);
        }

        private static void LogExp(StringBuilder error, Exception e)
        {
            var errorMsg = error.ToString();
            var exceptionClass = GetExceptionTypeStack(e);
            var exceptionMessage = GetExceptionMessageStack(e);
            var exceptionCallStackTrace = GetExceptionCallStack(e);

            error.AppendLine("");
            error.AppendLine("Exception classes:   ");
            error.Append(exceptionClass);
            error.AppendLine("");
            error.AppendLine("Exception messages: ");
            error.Append(exceptionMessage);
            error.AppendLine("");
            error.AppendLine("Stack Traces:");
            error.Append(exceptionCallStackTrace);
            error.AppendLine("");

            Fatal(error.ToString(), false);

            if (EnableRecordException && OnLogExp != null)
            {
                OnLogExp(new ExceptionInfo()
                {
                    Error = errorMsg,
                    ExceptionCallTrace = exceptionCallStackTrace,
                    ExceptionLevel = 1,
                    ExceptionMessage = exceptionMessage,
                    ExceptionName = exceptionClass,
                });
            }
        }

        private static bool EnableRecordException
        {
            get
            {
                var config = ConfigurationManager.AppSettings["EnableRecordException"];
                if (config == null)
                    return false;

                bool ret = false;
                if (!bool.TryParse(config.ToString(), out ret))
                {
                    return false;
                }

                return ret;
            }
        }

        private static string GetExceptionTypeStack(Exception e)
        {
            if (e.InnerException != null)
            {
                StringBuilder message = new StringBuilder();
                message.AppendLine(GetExceptionTypeStack(e.InnerException));
                message.AppendLine("   " + e.GetType().ToString());
                return (message.ToString());
            }
            else
            {
                return "   " + e.GetType().ToString();
            }
        }

        private static string GetExceptionMessageStack(Exception e)
        {
            if (e.InnerException != null)
            {
                StringBuilder message = new StringBuilder();
                message.AppendLine(GetExceptionMessageStack(e.InnerException));
                message.AppendLine("   " + e.Message);
                return (message.ToString());
            }
            else
            {
                return "   " + e.Message;
            }
        }

        private static string GetExceptionCallStack(Exception e)
        {
            if (e.InnerException != null)
            {
                StringBuilder message = new StringBuilder();
                message.AppendLine(GetExceptionCallStack(e.InnerException));
                message.AppendLine("--- Next Call Stack:");
                message.AppendLine(e.StackTrace);
                return (message.ToString());
            }
            else
            {
                return e.StackTrace;
            }
        }

        protected static string getTraceInfo()
        {
            StringBuilder preamble = new StringBuilder();
            preamble.Append("\r\n Stack Trace:");
            StackTrace st = new StackTrace(true);
            for (int i = 0; i < st.FrameCount; i++)
            {
                StackFrame sf = st.GetFrame(i);
                string mname = sf.GetMethod().ReflectedType.FullName;
                preamble.Append("at method:");
                preamble.Append(mname);
                preamble.Append("::");
                preamble.Append(sf.GetMethod().Name);
                preamble.Append(" @");
                preamble.Append(sf.GetFileLineNumber());
                preamble.Append(" +file:");
                preamble.Append(sf.GetFileName());
                preamble.Append("\r\n");
                if (mname.StartsWith("System"))
                    break;
            }
            return preamble.ToString();
        }

        public static void ReceivedCallLog(string callCount, string info)
        {
            try
            {
                var path = System.IO.Directory.GetCurrentDirectory();
                const string dir = @"\ReceivedCallLog\";
                path += dir;

                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }

                var now = DateTime.Now;
                var fileName = now.Year.ToString() + now.Month.ToString() + now.Day.ToString() + ".txt";
                path += fileName;

                var mylog = "来电数目：" + callCount + "；" + "当前时间：" + now + "；" + "详细：" + info;
                if (!System.IO.File.Exists(path))
                {
                    string[] log = {
                                       "座席控件编程手册是这么说的",
                                       "QueueNum：等待排队电话个数",
                                       "QueueInfo：排队电话信息,该信息为一字符串,格式为: ",
                                       "N1,C1,T1,S1,Z1;N2,C2,T2,S2,Z2;…",
                                       "第一个参数是 N1为排队ACD编号",
                                       "第二个参数是 C1为排队电话的主叫号码",
                                       "第三个参数是 T1主叫时间",
                                       "第四个参数是 S1表示分配状态：0-等待分配 1-等待呼叫结果 2-已经振铃,等待应答结果 3-已分配过坐席但未成功应答，等待时间未到，继续等待重新分配",
                                       "第五个参数是 Z1坐席号",
                                       mylog
                                   };
                    System.IO.File.WriteAllLines(path, log, Encoding.UTF8);
                }
                else
                {
                    System.IO.File.AppendAllText(path, mylog + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch (Exception e)
            {
                LogExp("来电显示数目log失败", e);
            }
        }
    }

    public class FunctionLogger
    {
        public string Name { get; private set; }
        public bool Enable { get; set; }

        public FunctionLogger(string name)
        {
            Name = name;
        }

        public void Info(string msg)
        {
            if (Enable)
            {
                Logger.Info(msg);
            }
        }

        public void Debug(string msg)
        {
            if (Enable)
            {
                Logger.Debug(msg);
            }
        }

        public void Warn(string msg)
        {
            if (Enable)
            {
                Logger.Warn(msg);
            }
        }

        public void Fatal(string msg)
        {
            if (Enable)
            {
                Logger.Fatal(msg);
            }
        }
    }

    public static class FunctionLoggerFactory
    {
        private static IDictionary<string, FunctionLogger> _loggers = new Dictionary<string, FunctionLogger>();
        private static object _lockObject = new object();

        public static FunctionLogger LoggerInstance(string name)
        {
            if (!_loggers.ContainsKey(name))
            {
                lock (_lockObject)
                {
                    if (!_loggers.ContainsKey(name))
                    {
                        var logger = new FunctionLogger(name);
                        _loggers.Add(name, logger);
                    }
                }
            }

            return _loggers[name];
        }
    }

    public class ExceptionInfo
    {
        public string Error { get; set; }
        public string ExceptionName { get; set; }
        public int ExceptionLevel { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionCallTrace { get; set; }
    }
}
