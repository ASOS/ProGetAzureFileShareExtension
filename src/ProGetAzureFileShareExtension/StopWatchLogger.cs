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