using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GraphWriterService
{
    public abstract class WorkerEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public virtual bool OnStart()
        {
            return (true);
        }

        /// <summary>
        /// This method prevents unhandled exceptions from being thrown
        /// from the worker thread.
        /// </summary>
        internal void ProtectedRun()
        {
            try
            {
                // Call the Workers Run() method
                Run();
            }
            catch (SystemException)
            {
                // Exit Quickly on a System Exception
                throw;
            }
            catch (Exception e)
            {
                Trace.TraceInformation(e.Message);
                Debug.WriteLine(e.Message);
            }
        }

        public virtual void Run()
        {
            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public virtual async Task RunAsync(CancellationToken cancellationToken)
        {
        }


        public virtual void OnStop()
        {
            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();
        }
    }
}
