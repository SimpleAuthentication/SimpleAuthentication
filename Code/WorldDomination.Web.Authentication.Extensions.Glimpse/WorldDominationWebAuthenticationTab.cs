using System;
using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Tab.Assist;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace WorldDomination.Web.Authentication.Extensions.Glimpse {
    public class WorldDominationWebAuthenticationTab : ITab, ITabLayout, ITabSetup, IKey {
        private static readonly object Layout = TabLayout.Create()
                                                         .Row(r => {
                                                             r.Cell(0).WidthInPixels(100);
                                                             r.Cell(1).WidthInPixels(80);
                                                             r.Cell(2);
                                                             r.Cell(3).Class("mono");
                                                             r.Cell(4).Class("mono");
                                                         }).Build();

        private MemoryAppender _memoryAppender;
        private MemoryAppender MemoryAppender {
            get {
                if (_memoryAppender == null) {
                    Hierarchy h = LogManager.GetRepository() as Hierarchy;
                    var logger = h.GetLogger("WorldDomination");
                    if (logger == null)
                        return null;
                    var appenders = logger.Repository.GetAppenders().Where(x => x.Name == "MemoryAppender").ToList();
                    if (appenders.Any()) {
                        _memoryAppender = appenders.SingleOrDefault() as MemoryAppender;
                    }
                }
                return _memoryAppender;
            }
        }
        public string Key {
            get { return "glimpse_log"; }
        }

        public string Name {
            get { return "World Domination"; }
        }

        public RuntimeEvent ExecuteOn {
            get { return RuntimeEvent.EndRequest; }
        }

        public Type RequestContextType {
            get { return null; }
        }

        public object GetData(ITabContext context)
        {
            var section = Plugin.Create("TimeStamp", "Level", "LoggerName", "Message", "Exception");
            if (MemoryAppender != null)
            {
                var events = MemoryAppender.GetEvents().Where(x => x.LoggerName.ToLower().Contains("worlddomination")).ToList();
                if (!events.Any())
                    return null;
                foreach (var loggingEvent in events) {
                    section.AddRow()
                           .Column(loggingEvent.TimeStamp)
                           .Column(string.Format("<span data-levelNum='{0}'>{1}</span>", NumberFromLevel(loggingEvent.Level), loggingEvent.Level)).Raw()
                           .Column(loggingEvent.LoggerName)
                           .Column(loggingEvent.RenderedMessage)
                           .Column(loggingEvent.ExceptionObject != null ? loggingEvent.ExceptionObject.Message : null)
                           .ApplyRowStyle(StyleFromLevel(loggingEvent.Level));
                }

            }
            return section.Build();
        }

        public object GetLayout() {
            return Layout;
        }

        public void Setup(ITabSetupContext context) {

        }

        private string StyleFromLevel(Level level) {
            switch (level.DisplayName.ToUpper()) {
                case "TRACE":
                    return "trace";
                case "DEBUG":
                    return "debug";
                case "INFO":
                    return "info";
                case "WARN":
                    return "warn";
                case "ERROR":
                    return "error";
                case "FATAL":
                    return "fail";
                default:
                    return "";
            }
        }

        private int NumberFromLevel(Level level) {
            switch (level.DisplayName.ToUpper()) {
                case "TRACE":
                    return 1;
                case "Debug":
                    return 2;
                case "INFO":
                    return 3;
                case "WARN":
                    return 4;
                case "ERROR":
                    return 5;
                case "FATAL":
                    return 6;
                default:
                    return 0;
            }
        }
    }
}
