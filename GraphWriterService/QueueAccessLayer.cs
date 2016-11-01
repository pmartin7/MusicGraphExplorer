using Microsoft.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicGraphStore.GraphDataModel;

namespace GraphWriterService
{
    public sealed class QueueAccess
    {
        #region Properties
        CloudQueue queue;
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
            CloudQueueClient client = storageAccount.CreateCloudQueueClient();
            ////TODO: reqd the queue name from a list of constants
            queue = client.GetQueueReference("graphstoreupdateinstructionqueue");
            queue.CreateIfNotExists();
        }
        #endregion

        #region Public Enqueue Methods
        public async Task EnqueueGenreRelationshipInstruction(Genre genre)
        {
            InstructionMessage message = new InstructionMessage(InstructionCodes.UpdateGenreRelationships, genre.Name);
            //put instruction messages on message queue
            CloudQueueMessage instruction = new CloudQueueMessage(message.Message);
            await queue.AddMessageAsync(instruction);
        }
        #endregion

        #region Public Message Processing Methods
        public async Task<InstructionMessage> DequeueInstruction()
        {
            InstructionMessage instruction = null;
            ////TODO: catch errors when we cannot get messages from the queue, log and continue
            CloudQueueMessage qmsg = await queue.GetMessageAsync();
            try
            {
                if (null != qmsg)
                {
                    instruction = new InstructionMessage(qmsg.AsString);
                    queue.DeleteMessageAsync(qmsg);
                }
                return instruction;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                if (null != qmsg)
                {
                    queue.DeleteMessageAsync(qmsg); //could not process, so delete the message to avoid choking
                }
                return instruction;
            }
        }
        #endregion

        #region Queue Attributes
        public async Task<bool> IsQueueEmpty()
        {
            bool response = false;
            await queue.FetchAttributesAsync();

            if (queue.ApproximateMessageCount == 0) { response = true; }

            return response;
        }

        public async Task<int> GetQueueSize()
        {
            int response = 0;
            try
            {
                await queue.FetchAttributesAsync();
                response = queue.ApproximateMessageCount.Value;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            return response;
        }
        #endregion

    }
}
