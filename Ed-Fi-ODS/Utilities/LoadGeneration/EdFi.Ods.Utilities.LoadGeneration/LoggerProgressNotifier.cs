using System;
using log4net;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public class LoggerProgressNotifier : IProgress<decimal>
    {
        private ILog _logger = LogManager.GetLogger(typeof(LoggerProgressNotifier));

        private decimal lastProgress;
        private DateTime lastDisplayedProgress;

        public void Report(decimal value)
        {
            if (Math.Abs(value - lastProgress) >= .01m
                || (DateTime.Now - lastDisplayedProgress).TotalSeconds >= 30)
            {
                lastProgress = value;
                lastDisplayedProgress = DateTime.Now;
                _logger.InfoFormat("{0:p1} complete.", value);
            }
        }
    }
}