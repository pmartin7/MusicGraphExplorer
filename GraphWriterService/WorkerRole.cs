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
using GraphWriterService.Helpers;

namespace GraphWriterService
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private List<Thread> Threads = new List<Thread>();
        private List<WorkerEntryPoint> Workers;
        protected EventWaitHandle EventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        public override void Run()
        {
            // This service will read instructions from a queue and update/insert records into the Music Graph Store (e.g. update related artists + artistId))
            // Will initialize a DAL per instance of the service running
            Trace.TraceInformation("GraphWriterService is running");

            foreach (WorkerEntryPoint worker in Workers)
                Threads.Add(new Thread(worker.ProtectedRun));

            foreach (Thread thread in Threads)
                thread.Start();

            while (!EventWaitHandle.WaitOne(0))
            {
                // WWB: Restart Dead Threads
                for (Int32 i = 0; i < Threads.Count; i++)
                {
                    if (!Threads[i].IsAlive)
                    {
                        Threads[i] = new Thread(Workers[i].Run);
                        Threads[i].Start();
                    }
                }

                EventWaitHandle.WaitOne(1000);
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            Workers = new List<WorkerEntryPoint>();

            Workers.Add(new WorkerGenreRelationshipQueueWriter());
            Workers.Add(new WorkerInstructionQueueProcessor());

            foreach (WorkerEntryPoint worker in Workers)
            {
                worker.OnStart();
            }

            bool result = base.OnStart();

            Trace.TraceInformation("GraphWriterService has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("GraphWriterService is stopping");

            EventWaitHandle.Set();

            foreach (Thread thread in Threads)
                while (thread.IsAlive)
                    thread.Abort();

            // WWB: Check To Make Sure The Threads Are
            // Not Running Before Continuing
            foreach (Thread thread in Threads)
                while (thread.IsAlive)
                    Thread.Sleep(10);

            // WWB: Tell The Workers To Stop Looping
            foreach (WorkerEntryPoint worker in Workers)
                worker.OnStop();

            base.OnStop();

            Trace.TraceInformation("GraphWriterService has stopped");
        }
    }
}
