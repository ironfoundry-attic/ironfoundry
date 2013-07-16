namespace IronFoundry.Warden.ProcessIsolation.DiskQuota
{
    using System;
    using DiskQuotaTypeLibrary;
    using NLog;

    // ref: http://msdn.microsoft.com/en-us/library/windows/desktop/bb787902(v=vs.85).aspx
    public class DiskQuotaManager : IDiskQuotaManager
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly DiskQuotaControl controller;

        /// <summary>
        /// Creates a new disk quota manager
        /// </summary>
        /// <param name="diskVolume">The fully qualified volume lable (for example, c:\ or unc style \\server\c$\)</param>
        public DiskQuotaManager(string diskVolume)
        {
            controller = new DiskQuotaControl();
            controller.Initialize(diskVolume, true);
            controller.UserNameResolution = UserNameResolutionConstants.dqResolveNone;
        }

        public void AddOrUpdate(string logonName, double quotaLimitBytes, double thresholdBytes)
        {
            if (string.IsNullOrWhiteSpace(logonName))
            {
                throw new ArgumentException("Logon Name expected", "logonName");
            }
            if (quotaLimitBytes < 1)
            {
                throw new ArgumentOutOfRangeException("quotaLimitBytes", quotaLimitBytes, "Expected 1 <= value <= disk space");
            }
            if (thresholdBytes < 1)
            {
                throw new ArgumentOutOfRangeException("thresholdBytes", thresholdBytes, "Expected 1 <= value <= quotaLimit");
            }

            try
            {
                var user = FindUser(logonName) ?? controller.AddUser(logonName);
                if (user.QuotaUsed > quotaLimitBytes)
                {
                    throw new Exception("New quota limit already exceeded");
                }

                log.Info("Setting quota for user {0} to {1} with alert at {2}", user.DisplayName, quotaLimitBytes, thresholdBytes);
                user.QuotaLimit = quotaLimitBytes;
                user.QuotaThreshold = thresholdBytes;
            }
            catch (Exception ex)
            {
                log.ErrorException("Unable to add or update user disk quota limits", ex);
            }
        }

        public DiskQuotaInfo GetInfo(string logonName)
        {
            var user = FindUser(logonName);
            if (user != null)
            {
                return new DiskQuotaInfo
                {
                    QuotaLimit = user.QuotaLimitText,
                    QuotaThreshold = user.QuotaThresholdText,
                    QuotaUsed = user.QuotaUsedText
                };
            }

            return new DiskQuotaInfo();
        }

        public void Remove(string logonName)
        {
            var user = FindUser(logonName);
            if (user != null)
            {
                try
                {
                    controller.DeleteUser(user);
                }
                catch (Exception ex)
                {
                    log.ErrorException(String.Format("Unable to delete user {0}", logonName), ex);
                }
            }
        }

        // ref: FindUser docs - http://msdn.microsoft.com/en-us/library/windows/desktop/bb787904(v=vs.85).aspx
        // ref: DIDiskQuotaUser - http://msdn.microsoft.com/en-us/library/windows/desktop/bb787925(v=vs.85).aspx
        // note: if
        private DIDiskQuotaUser FindUser(string logonName)
        {
            try
            {
                return controller.FindUser(logonName);
            }
            catch
            {
                try
                {
                    // in case the username hasn't loaded yet and/or the cached username is different
                    var sid = controller.TranslateLogonNameToSID(logonName);
                    controller.FindUser(sid);
                }
                catch (Exception ex)
                {
                    log.ErrorException(String.Format("Unable to lookup user {0}", logonName), ex);
                }
            }

            return null;
        }
    }
}
