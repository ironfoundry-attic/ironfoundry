namespace IronFoundry.Warden.Jobs
{
    using System;
    using System.Threading.Tasks;
    using NLog;

    public class Job
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly uint jobId;
        private readonly IJobRunnable runnable;

        private Task runnableTask;
        private IJobListener listener;
        private IJobResult result;
        private bool isCompleted = false;

        public Job(uint jobId, IJobRunnable runnable)
        {
            if (jobId == default(uint))
            {
                throw new ArgumentException("jobId must be > 0");
            }
            if (runnable == null)
            {
                throw new ArgumentNullException("runnable");
            }
            this.jobId = jobId;
            this.runnable = runnable;
        }

        public async void Run()
        {
            try
            {
                runnableTask = runnable.RunAsync();
                await runnableTask;
            }
            catch (Exception ex)
            {
                result = new JobExceptionResult(ex);
            }
            finally
            {
                isCompleted = true;
            }
        }

        public void AttachListener(IJobListener listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }
            if (this.listener != null)
            {
                throw new InvalidOperationException("Only one listener can be attached to a job at a time.");
            }
            this.listener = listener;
        }

        public void Listen()
        {
            if (this.listener == null)
            {
                throw new InvalidOperationException("Must attach listener before calling Listen()");
            }
            if (this.runnableTask == null)
            {
                throw new InvalidOperationException("Must call Run() before calling Listen()");
            }

            try
            {
                runnable.JobStatusAvailable += runnable_JobStatusAvailable;
                log.Trace("Listen: BEFORE Task.WaitAll(runnabletask:{0})", runnableTask.Id);
                this.listener.ObserveStatus(this.Status); // Observe saved status
                Task.WaitAll(runnableTask);
                log.Trace("Listen: AFTER Task.WaitAll(runnabletask:{0})", runnableTask.Id);
            }
            finally
            {
                runnable.JobStatusAvailable -= runnable_JobStatusAvailable;
                isCompleted = true;
            }
        }

        public void Cancel()
        {
            runnable.Cancel();
        }

        public bool IsCompleted
        {
            get { return isCompleted; }
        }

        public IJobStatus Status
        {
            get { return runnable.Status; }
        }

        public IJobResult Result
        {
            get { return result; }
        }

        private void runnable_JobStatusAvailable(object sender, JobStatusEventArgs e)
        {
            if (this.listener == null)
            {
                throw new InvalidOperationException("Must attach listener before calling Listen()");
            }
            log.Trace("JobStatus: '{0}'", e.JobStatus.Data);
            this.listener.ObserveStatus(e.JobStatus);
        }
    }
}
