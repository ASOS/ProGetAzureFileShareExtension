using System;
using System.Diagnostics;
using log4net;

namespace ProGetAzureFileShareExtension
{
    internal class StopWatchLogger : IDisposable
    {
        private ILog _logger;
        private readonly string _message;
        private readonly Stopwatch _stopWatch;

        public StopWatchLogger(ILog logger, string message)
        {
            _logger = logger;
            _message = message;
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
        }

        public void Dispose()
        {
            _stopWatch.Stop();
            _logger.DebugFormat("Executing {0} took {1} milliseconds.", _message, _stopWatch.ElapsedMilliseconds);
        }
    }
}