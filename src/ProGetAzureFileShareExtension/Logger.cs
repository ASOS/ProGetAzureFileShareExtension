using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace ProGetAzureFileShareExtension
{
    internal class Logger
    {
        public static ILog Initialise(string logFileName)
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();

            var patternLayout = new PatternLayout
            {
                ConversionPattern = "%date [%thread] %-5level - %message%newline"
            };
            patternLayout.ActivateOptions();

            if (!string.IsNullOrWhiteSpace(logFileName))
            {
                var rollingFileAppender = new RollingFileAppender
                {
                    AppendToFile = true,
                    File = logFileName,
                    Layout = patternLayout,
                    MaxSizeRollBackups = 5,
                    MaximumFileSize = "10MB",
                    RollingStyle = RollingFileAppender.RollingMode.Size,
                    StaticLogFileName = true
                };
                rollingFileAppender.ActivateOptions();
                hierarchy.Root.AddAppender(rollingFileAppender);
            }
            var memory = new MemoryAppender();
            memory.ActivateOptions();
            hierarchy.Root.AddAppender(memory);

            hierarchy.Root.Level = Level.Debug;
            hierarchy.Configured = true;

            return LogManager.GetLogger(typeof (Logger));
        }
    }
}
