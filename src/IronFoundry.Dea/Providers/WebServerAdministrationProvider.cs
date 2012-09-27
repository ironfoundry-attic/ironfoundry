namespace IronFoundry.Dea.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;
    using IronFoundry.Dea.Configuration;
    using IronFoundry.Dea.Properties;
    using IronFoundry.Dea.Services;
    using IronFoundry.Misc;
    using IronFoundry.Misc.Logging;
    using IronFoundry.Misc.Utilities;

    public class WebServerAdministrationProvider : IWebServerAdministrationProvider
    {
        private static readonly TimeSpan twoSeconds = TimeSpan.FromSeconds(2);
        private static bool loggingUnlocked = false;

        private static readonly Regex apppoolStateRegex = new Regex(@"state:(\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex workerProcessRegex = new Regex(@"WP ""(\d+)"" \(applicationPool:([^)]+)\)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private const string IIsAppPoolObject = "apppool";
        private const string IIsWorkerProcessObject = "wp";

        private readonly ILog log;
        private readonly IPAddress localIPAddress;
        private readonly IFirewallService firewallService;

        private readonly object appcmdLock = new object();
        private readonly string appCmdPath;

        public WebServerAdministrationProvider(ILog log, IDeaConfig config, IFirewallService firewallService)
        {
            this.log = log;
            this.localIPAddress = config.LocalIPAddress;
            this.firewallService = firewallService;
            this.appCmdPath = config.AppCmdPath;
            UnlockLogging();
        }

        public WebServerAdministrationBinding InstallWebApp(
            string localDirectory, string applicationInstanceName, ushort managedRuntimeVersion)
        {
            WebServerAdministrationBinding rv = null;

            if (managedRuntimeVersion != 2 && managedRuntimeVersion != 4)
            {
                throw new ArgumentException(
                    String.Format(Resources.WebServerAdministrationProvider_InvalidManagedRuntimeVersion_Fmt, managedRuntimeVersion), "managedRuntimeVersion");
            }

            try
            {
                ushort applicationPort = 0;

                bool exists = DoesIIsAppPoolExist(applicationInstanceName);
                if (exists)
                {
                    log.Error(Resources.WebServerAdministrationProvider_AppAlreadyExists_Fmt, applicationInstanceName);
                }
                else
                {
                    // NB: must lock to ensure multiple threads don't grab the same port.
                    lock (appcmdLock)
                    {
                        string cmd = String.Format("add apppool /name:{0}", applicationInstanceName);
                        ExecCmdResult rslt = ExecAppcmd(cmd, 5, twoSeconds);
                        if (false == rslt.Success)
                        {
                            return null;
                        }

                        cmd = String.Format(
                            "set apppool {0} /autoStart:true /managedRuntimeVersion:v{1}.0 /managedPipelineMode:Integrated /processModel.loadUserProfile:true",
                            applicationInstanceName, managedRuntimeVersion);
                        rslt = ExecAppcmd(cmd, 5, twoSeconds);
                        if (false == rslt.Success)
                        {
                            return null;
                        }

                        applicationPort = Utility.RandomFreePort();
                        cmd = String.Format("add site /name:{0} /bindings:http/*:{1}: /physicalPath:{2}",
                            applicationInstanceName, applicationPort, localDirectory);
                        rslt = ExecAppcmd(cmd, 5, twoSeconds);
                        if (false == rslt.Success)
                        {
                            return null;
                        }

                        cmd = String.Format("set site {0} /[path='/'].applicationPool:{0}", applicationInstanceName);
                        rslt = ExecAppcmd(cmd, 5, twoSeconds);
                        if (false == rslt.Success)
                        {
                            return null;
                        }

                        cmd = String.Format("set config {0} /section:system.webServer/httpLogging /dontLog:True", applicationInstanceName);
                        rslt = ExecAppcmd(cmd, 5, twoSeconds);
                        if (false == rslt.Success)
                        {
                            return null;
                        }

                        cmd = String.Format("start apppool {0}", applicationInstanceName);
                        rslt = ExecAppcmd(cmd, 5, twoSeconds);
                        if (false == rslt.Success)
                        {
                            return null;
                        }

                        cmd = String.Format("start site {0}", applicationInstanceName);
                        rslt = ExecAppcmd(cmd, 5, twoSeconds);
                        if (false == rslt.Success)
                        {
                            return null;
                        }
                    }

                    rv = new WebServerAdministrationBinding { Host = localIPAddress.ToString(), Port = applicationPort };
                }

                firewallService.Open(applicationPort, applicationInstanceName);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return rv;
        }

        public void UninstallWebApp(string applicationInstanceName)
        {
            try
            {
                string cmd = String.Format("stop apppool {0}", applicationInstanceName);
                ExecAppcmd(cmd, 5, twoSeconds);

                ushort i = 0;
                ApplicationInstanceStatus status = ApplicationInstanceStatus.Unknown;
                while (ApplicationInstanceStatus.Stopped != status && i < 5)
                {
                    status = GetApplicationStatus(applicationInstanceName);
                    ++i;
                }

                cmd = String.Format("delete apppool {0}", applicationInstanceName);
                ExecAppcmd(cmd, 5, twoSeconds);

                cmd = String.Format("delete site {0}", applicationInstanceName);
                ExecAppcmd(cmd, 5, twoSeconds);

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            try
            {
                firewallService.Close(applicationInstanceName);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public ApplicationInstanceStatus GetApplicationStatus(string applicationInstanceName)
        {
            ApplicationInstanceStatus rv = ApplicationInstanceStatus.Unknown;
            try
            {
                string state = GetIIsAppPoolState(applicationInstanceName);
                if (state.IsNullOrWhiteSpace())
                {
                    rv = ApplicationInstanceStatus.Deleted;
                }
                else
                {
                    if (state == "started")
                    {
                        rv = ApplicationInstanceStatus.Started;
                    }
                    else if (state == "starting")
                    {
                        rv = ApplicationInstanceStatus.Starting;
                    }
                    else if (state == "stopped")
                    {
                        rv = ApplicationInstanceStatus.Stopped;
                    }
                    else if (state == "stopping")
                    {
                        rv = ApplicationInstanceStatus.Stopping;
                    }
                    else
                    {
                        rv = ApplicationInstanceStatus.Unknown;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return rv;
        }

        public IDictionary<string, IList<int>> GetIIsWorkerProcesses()
        {
            IDictionary<string, IList<int>> rv = null;

            ExecCmdResult rslt = ExecAppcmd("list wp", 1, null, true);
            if (rslt.Success && false == rslt.Output.IsNullOrWhiteSpace())
            {
                string[] lines = rslt.Output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                if (false == lines.IsNullOrEmpty())
                {
                    rv = new Dictionary<string, IList<int>>();
                    foreach (string line in lines)
                    {
                        Match m = workerProcessRegex.Match(line);
                        int pid = Convert.ToInt32(m.Groups[1].Value);
                        string apppoolName = m.Groups[2].Value;
                        IList<int> pids;
                        if (rv.TryGetValue(apppoolName, out pids))
                        {
                            pids.Add(pid);
                        }
                        else
                        {
                            rv.Add(apppoolName, new List<int> { pid });
                        }
                    }
                }
            }

            return rv;
        }

        private bool DoesIIsAppPoolExist(string objectName)
        {
            string state = GetIIsAppPoolState(objectName);
            return false == state.IsNullOrWhiteSpace();
        }

        private string GetIIsAppPoolState(string objectName)
        {
            string rv = null;

            ExecCmdResult rslt = ExecAppcmd(String.Format(@"list {0} ""/name:{1}""", IIsAppPoolObject, objectName), 1, null, true);
            if (rslt.Success)
            {
                Match m = apppoolStateRegex.Match(rslt.Output);
                rv = m.Groups[1].Value.ToLowerInvariant();
            }

            return rv;
        }

        private void UnlockLogging()
        {
            if (false == loggingUnlocked)
            {
                ExecAppcmd("unlock config /section:system.webserver/httpLogging");
                loggingUnlocked = true;
            }
        }

        private ExecCmdResult ExecAppcmd(string arguments,
            ushort numTries = 1, TimeSpan? retrySleepInterval = null, bool expectError = false)
        {
            var execCmd = new ExecCmd(log, appCmdPath, arguments);
            return execCmd.Run(numTries, retrySleepInterval, expectError);
        }
    }
}