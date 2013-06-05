namespace IronFoundry.Warden.Jobs
{
    using System.Collections.Generic;
    using System.Threading;

    public class JobManager : IJobManager
    {
        private uint jobIds = 0;

        private readonly IDictionary<uint, Job> jobs = new Dictionary<uint, Job>();
        private readonly ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim();

        public uint StartJobFor(IJobRunnable runnable)
        {
            try
            {
                rwlock.EnterWriteLock();
                uint jobId = GetNextJobID();
                var job = new Job(jobId, runnable);
                jobs.Add(jobId, job);
                job.Run();
                return jobId;
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }

        public Job GetJob(uint jobId)
        {
            try
            {
                rwlock.EnterReadLock();
                return jobs[jobId];
            }
            finally
            {
                rwlock.ExitReadLock();
            }
        }

        private uint GetNextJobID()
        {
            ++jobIds;
            return jobIds;
        }
    }
}
