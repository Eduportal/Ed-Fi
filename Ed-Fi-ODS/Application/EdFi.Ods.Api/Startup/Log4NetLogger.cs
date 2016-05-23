using System;
using System.Diagnostics;
using log4net;
using Microsoft.Owin.Logging;

namespace EdFi.Ods.Api.Startup
{
    public class Log4NetLoggerFactory : ILoggerFactory
    {
        public ILogger Create(string name)
        {
            var log = LogManager.GetLogger(name);
            return new Log4NetLogger(log);
        }
    }

    internal class Log4NetLogger : ILogger
    {
        private readonly ILog _log;

        public Log4NetLogger(ILog log)
        {
            _log = log;
        }

        public bool WriteCore(TraceEventType eventType, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            switch (eventType)
            {
                case TraceEventType.Critical:
                    _log.Fatal(state, exception);
                    break;
                case TraceEventType.Error:
                    _log.Error(state, exception);
                    break;
                case TraceEventType.Warning:
                    _log.Warn(state,exception);
                    break;
                case TraceEventType.Information:
                    _log.Info(state, exception);
                    break;
                default:
                    _log.Debug(state, exception);
                    break;
            }
            return true;
        }
    }
}
