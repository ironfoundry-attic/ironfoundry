namespace IronFoundry.Warden.Jobs
{
    using System;

    public class Job
    {
        private readonly uint jobId;
        private readonly IJobRunnable runnable;

        private IJobResult result;

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
                result = await runnable.RunAsync();
            }
            catch (Exception ex)
            {
                result = new JobExceptionResult(ex);
            }
        }

        public void Cancel()
        {
            runnable.Cancel();
        }

        public bool IsCompleted
        {
            get { return result != null; }
        }

        public IJobStatus Status
        {
            get { return runnable.Status; }
        }

        public IJobResult Result
        {
            get { return result; }
        }
    }
}
