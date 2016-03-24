/*
Copyright 2016 ASOS.com Limited

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

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
