using GraphWriterService.Helpers;
using MusicGraphStore.DataAccessLayer;
using MusicGraphStore.GraphDataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GraphWriterService
{
    internal class WorkerInstructionQueueProcessor : WorkerEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override async Task RunAsync(CancellationToken cancellationToken)
        {
            Debug.WriteLine("Started WorkerInstructionQueueProcessor");

            TimeSpan dequeueThreadSleepTime = Config.GetDequeueThreadSleepTime();

            while (!cancellationToken.IsCancellationRequested)
            {
                //Then process instructions on the queue as long as there are messages on the queue
                ProcessInstructionQueue().Wait();

                //if task completes all messages, sleep
                System.Diagnostics.Debug.WriteLine("Waiting to dequeue messages: {0}", dequeueThreadSleepTime);
                Thread.Sleep(dequeueThreadSleepTime);
            }
        }

        public override void Run()
        {
            try
            {
                //multi-threaded dequeue
                int threadCount = 4; ////TODO: make thread count a config
                Task[] tasks = new Task[threadCount];
                for (int i=0; i<threadCount; i++)
                {
                    tasks[i] = this.RunAsync(this.cancellationTokenSource.Token);
                }
                Task.WaitAll(tasks);
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override void OnStop()
        {
            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();
        }

        #region Work Processor
        private async Task ProcessInstructionQueue()
        {
            ////TODO: read from the queue and execute instructions for update graph from spotify, update related genres, updated related artists, and update artist relevance in genre
            try
            {
                QueueAccess qal = QueueAccess.Instance;
                bool isEmpty = await qal.IsQueueEmpty(); 

                DataAccess dal = DataAccess.Instance;

                GraphUpdateState genreRelationshipState = new GraphUpdateState() { Type = GraphUpdateStates.GenreRelationship }; ////TODO - make this a class constant

                int n = 0;
                InstructionMessage newInstruction;

                while (!isEmpty)
                {
                    //TODO: Delete message after instruction is processed
                    newInstruction = await qal.DequeueInstruction();

                    if (null != newInstruction)
                    {
                        if (null != newInstruction.InstructionKey)
                        {
                            switch (newInstruction.InstructionCode)
                            {
                                case InstructionCodes.UpdateGenreRelationships:
                                    Genre genre = new Genre() { Name = newInstruction.InstructionKey };
                                    Genre response = dal.ComputeRelatedGenresForGenre(genre);
                                    dal.InsertOrUpdateRelatedGenresForGenre(response);
                                    System.Diagnostics.Debug.WriteLine("updated {0} related genres for: {1} from thread {2}", 
                                        response.RelatedGenres.Count, response.Name, Thread.CurrentThread.ManagedThreadId);
                                    //update state
                                    ////TODO - this section on updating the lastUpdateStart is currently not needed
                                    if (n == 0) { genreRelationshipState.SyncLastUpdateStartdDtTm(); }
                                    genreRelationshipState.SyncLastUpdateEndDtTm(); // keep on updating while there are messages on queue
                                    break;
                                ////TODO: Handle cases for artist relationship, update graph from spotify, and artist genre relevance
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

                    if (n % 50 == 0)
                    {
                        int size = await qal.GetQueueSize();
                        Debug.WriteLine("Queue Size: {0}", size);
                    }
                    n++;
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
