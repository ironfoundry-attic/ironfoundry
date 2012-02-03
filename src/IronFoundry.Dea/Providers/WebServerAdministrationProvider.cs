namespace IronFoundry.Dea.Providers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;
    using IronFoundry.Dea.Config;
    using IronFoundry.Dea.Logging;
    using IronFoundry.Dea.Properties;
    using IronFoundry.Dea.Services;
    using Microsoft.Win32;

    public class WebServerAdministrationProvider : IWebServerAdministrationProvider
    {
        private static readonly TimeSpan twoSeconds = TimeSpan.FromSeconds(2);
        private static readonly Regex appcmdStateRegex = new Regex(@"state:(\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private const string IIsAppPoolObject = "apppool";
        private const string IIsSiteObject = "site";

        private readonly ILog log;
        private readonly IPAddress localIPAddress;
        private readonly IFirewallService firewallService;

        private readonly object appcmdLock = new object();
        private readonly string appCmdPath;

        public WebServerAdministrationProvider(ILog log, IConfig config, IFirewallService firewallService)
        {
            this.log = log;
            this.localIPAddress = config.LocalIPAddress;
            this.firewallService = firewallService;
            this.appCmdPath = InitAppCmdPath();
        }

        public WebServerAdministrationBinding InstallWebApp(
            string localDirectory, string applicationInstanceName, uint memMB)
        {
            WebServerAdministrationBinding rv = null;

            try
            {
                ushort applicationPort = 0;

                bool exists = DoesIIsObjectExist(IIsAppPoolObject, applicationInstanceName);
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
                        AppCmdResult rslt = ExecAppcmd(cmd, 5, twoSeconds);
                        if (false == rslt.Success)
                        {
                            return null;
                        }

                        uint memKB = memMB * 1024;
                        cmd = String.Format(
                            "set apppool {0} /autoStart:true /managedRuntimeVersion:v4.0 /managedPipelineMode:Integrated /recycling.periodicRestart.privateMemory:{1}",
                            applicationInstanceName, memKB);
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

        public bool DoesApplicationExist(string applicationInstanceName)
        {
            bool rv = false;

            try
            {
                /*
                    C:\>%windir%\system32\inetsrv\appcmd.exe list site "/name:Default Web Site"
                    SITE "Default Web Site" (id:1,bindings:http/*:80:,net.tcp/808:*,net.pipe/*,net.msmq/localhost,msmq.formatname/localhost,state:Started)
                 */
                string poolState = GetIIsObjectState(IIsAppPoolObject, applicationInstanceName);
                if (false == poolState.IsNullOrWhiteSpace())
                {
                    string siteState = GetIIsObjectState(IIsSiteObject, applicationInstanceName);
                    if (false == siteState.IsNullOrWhiteSpace())
                    {
                        rv = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return rv;
        }

        public ApplicationInstanceStatus GetApplicationStatus(string applicationInstanceName)
        {
            ApplicationInstanceStatus rv = ApplicationInstanceStatus.Unknown;
            try
            {
                /*
                    C:\>%windir%\system32\inetsrv\appcmd.exe list apppool /name:DefaultAppPool
                    APPPOOL "DefaultAppPool" (MgdVersion:v4.0,MgdMode:Integrated,state:Started)
                 */
                string state = GetIIsObjectState(IIsAppPoolObject, applicationInstanceName);
                if (false == state.IsNullOrWhiteSpace())
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

        private bool DoesIIsObjectExist(string objectType, string objectName)
        {
            string state = GetIIsObjectState(objectType, objectName);
            return false == state.IsNullOrWhiteSpace();
        }

        private string GetIIsObjectState(string objectType, string objectName)
        {
            string rv = null;

            AppCmdResult rslt = ExecAppcmd(String.Format(@"list {0} ""/name:{1}""", objectType, objectName), 1, null, true);
            if (rslt.Success)
            {
                Match m = appcmdStateRegex.Match(rslt.Output);
                rv = m.Groups[1].Value.ToLowerInvariant();
            }

            return rv;
        }

        private class AppCmdResult
        {
            private readonly bool success = false;
            private readonly string output = null;

            public AppCmdResult(bool success, string output)
            {
                this.success = success;
                this.output = output;
            }

            public bool Success { get { return success; } }
            public string Output { get { return output; } }
        }

        private AppCmdResult ExecAppcmd(string arguments,
            ushort numTries = 1, TimeSpan? retrySleepInterval = null, bool expectError = false)
        {
            bool success = false;
            string output = null, errout = null;
            try
            {
                for (ushort i = 0; i < numTries && false == success; ++i)
                {
                    lock (appcmdLock)
                    {
                        var p = new Process();
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.RedirectStandardError = true;
                        p.StartInfo.FileName = appCmdPath;
                        p.StartInfo.Arguments = arguments;
                        p.Start();
                        output = p.StandardOutput.ReadToEnd().TrimEnd('\r', '\n');
                        errout = p.StandardError.ReadToEnd().TrimEnd('\r', '\n');
                        p.WaitForExit();
                        success = 0 == p.ExitCode;
                    }
                    if (false == success)
                    {
                        if (false == expectError)
                        {
                            log.Error(Resources.WebServerAdministrationProvider_AppCmdFailed_Fmt, arguments, errout);
                        }
                        if (numTries > 1 && retrySleepInterval.HasValue)
                        {
                            Thread.Sleep(retrySleepInterval.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                output = null;
                log.Error(ex);
            }
            return new AppCmdResult(success, output);
        }

        private static string InitAppCmdPath()
        {
            RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
            RegistryKey subKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\InetStp");
            string iisInstallPath = subKey.GetValue("InstallPath").ToString();
            string appCmdPath = Path.Combine(iisInstallPath, "appcmd.exe");
            if (false == File.Exists(appCmdPath))
            {
                throw new ArgumentException(String.Format(Resources.WebServerAdministrationProvider_AppCmdNotFound_Fmt, iisInstallPath));
            }
            return appCmdPath;
        }
    }
}