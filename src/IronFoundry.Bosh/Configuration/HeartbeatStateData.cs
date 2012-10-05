namespace IronFoundry.Bosh.Configuration
{
    public class HeartbeatStateData
    {
        private readonly string job;
        private readonly ushort index;
        private readonly string jobState;

        public HeartbeatStateData(string job, ushort index, string jobState)
        {
            this.job = job;
            this.index = index;
            this.jobState = jobState;
        }

        public string Job { get { return job; } }
        public ushort Index { get { return index; } }
        public string JobState { get { return jobState; } }
    }
}