namespace IronFoundry.Warden.ProcessIsolation.Service
{
    public class ResourceLimits
    {
        /// <summary>
        /// The maximum amount of memory allowed to be allocated to a job or its processes, in Megabytes.
        /// </summary>
        public uint MemoryMB { get; set; }

        /// <summary>
        /// Percent CPU limit from 1-100. Specify 0 to remove limit.
        /// </summary>
        public byte CpuPercent { get; set; }
    }
}
