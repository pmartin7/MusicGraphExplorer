using Microsoft.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphWriterService.Helpers
{
    public static class Config
    {
        #region Publig Getters
        public static TimeSpan GetGraphRefreshInterval()
        {
            return ParseTimeStampConfig("GraphRefreshIntervalInSeconds");
        }

        public static TimeSpan GetEnqueueThreadSleepTime()
        {
            return ParseTimeStampConfig("EnqueueThreadSleepTimeInSeconds");
        }

        public static TimeSpan GetDequeueThreadSleepTime()
        {
            return ParseTimeStampConfig("DequeueThreadSleepTimeInSeconds");
        }
        #endregion

        #region private helpers
        private static TimeSpan ParseTimeStampConfig(string configName)
        {
            int sleepTimeInSeconds;
            if (!int.TryParse(
                CloudConfigurationManager.GetSetting(configName), out sleepTimeInSeconds))
            {
                throw new ApplicationException(
                    string.Format("incorrect WorkerServiceSleepTime provided: {0}", configName)
                    );
            }
            return new TimeSpan(0, 0, sleepTimeInSeconds);
        }
        #endregion

    }
}
