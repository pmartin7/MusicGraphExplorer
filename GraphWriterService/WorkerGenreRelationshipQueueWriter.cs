using Microsoft.Azure;
using MusicGraphStore.GraphDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphWriterService.Helpers;
using MusicGraphStore.DataAccessLayer;
using System.Threading;
using System.Diagnostics;

namespace GraphWriterService
{
    internal class WorkerGenreRelationshipQueueWriter : WorkerEntryPoint
    {
        public override async Task RunAsync(CancellationToken cancellationToken)
        {
            Debug.WriteLine("Started WorkerGenreRelationshipQueueWriter");

            TimeSpan enqueueThreadSleepTime = Config.GetEnqueueThreadSleepTime();
            TimeSpan graphRefreshInterval = Config.GetGraphRefreshInterval();
            object lockObject = new object();

            List<Genre> genres = new List<Genre>();
            DataAccess dal = DataAccess.Instance;
            QueueAccess qal = QueueAccess.Instance;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    //get from db the latest time at which updates happened
                    GraphUpdateState state = new GraphUpdateState() { Type = GraphUpdateStates.GenreRelationship };
                    state.RefreshFromDb();

                    //add to the queue if we have finished added to the queue for a time period greater than the interval
                    //and that we have not started adding already
                    if (((DateTime.Now - state.LastInstructionAddedEndDtTm) > graphRefreshInterval)
                         && ((DateTime.Now - state.LastInstructionAddedStartDtTm) > graphRefreshInterval))
                    {
                        //critical section to ensure that only one thread goes fetch data from store
                        Monitor.Enter(lockObject);
                        try
                        {
                            genres = dal.GetAllGenres();
                            Debug.WriteLine("Updating relationship relevance for {0} genres", genres.Count);
                            state.SyncLastInstructionAddedStartDtTm();
                        }
                        finally
                        {
                            Monitor.Exit(lockObject);
                        }

                        //multithreaded enqueue
                        Parallel.For(0, genres.Count, (index) =>
                        {
                            qal.EnqueueGenreRelationshipInstruction(genres[index]).Wait();
                            Debug.WriteLine("Added instruction {0}/{1} from thread {2}", index, genres.Count, Thread.CurrentThread.ManagedThreadId);
                        });

                        //finally, update state to reflect that last instruction was added
                        state.SyncLastInstructionAddedEndDtTm();

                    }
                    else
                    {
                        Debug.WriteLine("Waiting to add Genre relationship instructions to queue: {0}", enqueueThreadSleepTime);
                        Thread.Sleep(enqueueThreadSleepTime);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
            }
        }
    }
}
