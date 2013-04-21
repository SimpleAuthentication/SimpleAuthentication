using System;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace WorldDomination.Web.Authentication.Extensions.Glimpse {
    public class GlimpseLogger : ILoggingService
    {
        private ILog logger;

        public GlimpseLogger()
        {
            CreateAppender();
            logger = LogManager.GetLogger("WorldDomination");
        }

        public static IAppender CreateAppender() {
            var appender = new MemoryAppender();
            appender.Name = "MemoryAppender";
            appender.Layout = CreateDefaultLayout();
            appender.AddFilter(CreateDefaultFilter());
            appender.ActivateOptions(); // if omitted - throws an excpetion
            log4net.Config.BasicConfigurator.Configure(appender); //if omitted - no errors, but logging does not work
            return appender;
        }

        public void Debug(string message) {
            logger.Debug(message);
        }

        public void Info(string message) {
            logger.Info(message);
        }

        public void Warn(string message) {
            logger.Warn(message);
        }

        public void Error(string message, Exception exception = null) {
            logger.Error(message, exception);
        }

        public void Fatal(string message, Exception exception = null) {
            logger.Fatal(message, exception);
        }


        private static ILayout CreateDefaultLayout() {
            PatternLayout layout = new PatternLayout();
            layout.ConversionPattern = "%d{yyyy-MM-dd hh:mm:ss} - %level %m%n";
            layout.ActivateOptions();
            return layout;
        }


        private static IFilter CreateDefaultFilter() {
            LevelRangeFilter filter = new LevelRangeFilter { LevelMin = Level.All };
            filter.ActivateOptions();
            return filter;
        }
    }
}