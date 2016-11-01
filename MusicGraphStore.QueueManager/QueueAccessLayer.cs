using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphWriterService
{
    public sealed class QueueAccess
    {
        #region Properties
        CloudQueueClient client;
        #endregion

        #region Singleton Constructor
        //allocate an instance of the data access singleton
        static readonly QueueAccess _instance = new QueueAccess();

        //publicly available instance of the data access object
        public static QueueAccess Instance
        {
            get { return _instance; }
        }

        //private constructor
        private QueueAccess()
        {

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
        }

        #endregion
    }
}
