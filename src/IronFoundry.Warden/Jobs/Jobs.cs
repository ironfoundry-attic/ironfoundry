namespace IronFoundry.Warden.Jobs
{
    using System;
    using System.Threading.Tasks;

    public class Job
    {
        private readonly uint jobId;
        private readonly Task<IJobResult> runnableTask;

        public Job(uint jobId, IJobRunnable runnable) // TODO use something other than Func to save pre-run state for recovery.
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
            this.runnableTask = new Task<IJobResult>(runnable.Run);
        }

        public void Run()
        {
            runnableTask.Start();
        }

        public bool IsCompleted
        {
            get { return runnableTask.IsCompleted; }
        }

        public IJobResult Result
        {
            get
            {
                IJobResult rslt = null;

                if (runnableTask.IsCompleted)
                {
                    rslt =  runnableTask.Result;
                }

                return rslt;
            }
        }
    }
}
