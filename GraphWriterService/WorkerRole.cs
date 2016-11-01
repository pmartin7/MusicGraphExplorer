using Microsoft.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using MusicGraphStore.DataAccessLayer;
using MusicGraphStore.GraphDataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace GraphWriterService
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("GraphWriterService is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("GraphWriterService has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("GraphWriterService is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("GraphWriterService has stopped");
        }

        #region Background Thread
        /// <summary>
        /// Background thread to put work and process work on queue
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ////TODO: find a better way to make putting work on queue a scheduled job
        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // This service will read instructions from a queue and update/insert records into the Music Graph Store (e.g. update related artists + artistId))
            // Will initialize a DAL per instance of the service running
            while (!cancellationToken.IsCancellationRequested)
            {
                ////TODO: add instructions to queue for all types of update operations
                ////TODO: note - update from spotify has an interesting pattern as the dequeuer will continue enqueeing work until a condition is reached

                //First generate instructions to update genre relationships
                await this.InstructUpdateGenreRelationships();

                //Then process instructions on the queue
                await this.ProcessInstructionQueue();

                // process update genre relationship relevance
                //System.Diagnostics.Debug.WriteLine("Starting to update genre relationships");
                //MusicGraphStore.Write.Process.ComputeAndInsertRelatedGenres(dal);
                //System.Diagnostics.Debug.WriteLine("Finished updating genre relationships");
            }
        }
        #endregion

        #region Work Enqueuers
        /// <summary>
        /// This methods will read all genres from the DAL, and enqueue an instruction message for each genre to update genre relationships
        /// </summary>
        /// <param name="qal"></param>
        /// <param name="dal"></param>
        ////TODO: This should be part of the MusicGraphStore.Write.Process class
        private async Task InstructUpdateGenreRelationships()
        {
            try
            {

                //Get all genres from the graph store
                DataAccess dal = DataAccess.Instance;
                List<MusicGraphStore.GraphDataModel.Genre> genres = dal.GetAllGenres();
                System.Diagnostics.Debug.WriteLine("Updating relationship relevance for {0} genres", genres.Count);

                //Connect to instruction queue --- get an instance of the queue access layer
                QueueAccess qal = QueueAccess.Instance;

                int n = 0;

                //then for each genre, compute the related genres, and insert
                foreach (Genre genre in genres)
                {
                    n++;
                    await qal.EnqueueGenreRelationshipInstruction(genre);
                    System.Diagnostics.Debug.WriteLine("Added instruction {0}/{1}", n, genres.Count);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }
        #endregion

        #region Work Processor
        private async Task ProcessInstructionQueue()
        {
            ////TODO: read from the queue and execute instructions for update graph from spotify, update related genres, updated related artists, and update artist relevance in genre
            try
            {
                QueueAccess qal = QueueAccess.Instance;
                bool isEmpty = await qal.IsQueueEmpty();
                int size = await qal.GetQueueSize();

                DataAccess dal = DataAccess.Instance;

                int n = 0;
                while (!isEmpty)
                {
                    InstructionMessage instruction = await qal.DequeueInstruction();
                    if (null != instruction)
                    {
                        if (null != instruction.InstructionKey)
                        {
                            switch (instruction.InstructionCode)
                            {
                                case InstructionCodes.UpdateGenreRelationships:
                                    Genre genre = new Genre() { Name = instruction.InstructionKey };
                                    Genre response = dal.ComputeRelatedGenresForGenre(genre);
                                    dal.InsertOrUpdateRelatedGenresForGenre(response);
                                    System.Diagnostics.Debug.WriteLine("{2}/{3} - updated {0} related genres for: {1}", response.RelatedGenres.Count, response.Name, n, size);
                                    break;
                                default:
                                    break;
                            }

                        }
                    }
                    else
                    {
                        isEmpty = true;
                        //queue is empty. 
                    }
                    n = (n + 1 + 10000) % 10000;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }
        #endregion

    }
}
