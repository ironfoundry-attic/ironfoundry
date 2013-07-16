namespace IronFoundry.Warden.ProcessIsolation.DiskQuota
{
    public interface IDiskQuotaManager
    {
        /// <summary>
        /// Removes any disk quotas for the specified user on the volume.
        /// </summary>
        /// <param name="logonName">The fully qualified domain name. (for example, SAM-compatible and UPN) </param>
        void Remove(string logonName);

        /// <summary>
        /// Gets the disk quota info for the specified user on the volume.
        /// </summary>
        /// <param name="logonName">The fully qualified domain name. (for example, SAM-compatible and UPN) </param>
        /// <returns>Empty info if user not found or the actual settings and usage values.</returns>
        DiskQuotaInfo GetInfo(string logonName);

        /// <summary>
        /// Adds or updates the disk quota for the specified user on the volume.
        /// </summary>
        /// <param name="logonName">The fully qualified domain name. (for example, SAM-compatible and UPN) </param>
        /// <param name="quotaLimitBytes"></param>
        /// <param name="thresholdBytes"></param>
        void AddOrUpdate(string logonName, double quotaLimitBytes, double thresholdBytes);
    }
}
