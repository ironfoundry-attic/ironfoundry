namespace CloudFoundry.Net.Dea
{
    using System;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using ICSharpCode.SharpZipLib.GZip;
    using ICSharpCode.SharpZipLib.Tar;
    using NLog;
    using Types;

    public class FilesManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly bool disableDirCleanup = false;
        private readonly string dropletsPath;

        public FilesManager()
        {
            Boolean.TryParse(ConfigurationManager.AppSettings[Constants.AppSettings.DisableDirCleanup], out disableDirCleanup);

            dropletsPath = ConfigurationManager.AppSettings[Constants.AppSettings.DropletsDirectory];
            ApplicationPath = ConfigurationManager.AppSettings[Constants.AppSettings.ApplicationsDirectory];
            Directory.CreateDirectory(dropletsPath);
            Directory.CreateDirectory(ApplicationPath);

            SnapshotFile = Path.Combine(ConfigurationManager.AppSettings[Constants.AppSettings.DropletsDirectory], "snapshot.json");
        }

        public string ApplicationPath { get; private set; }

        public string SnapshotFile { get; private set; }

        public string GetApplicationPathFor(Instance argInstance)
        {
            string instanceDropletsPath, instanceApplicationPath;
            getInstancePaths(argInstance, out instanceDropletsPath, out instanceApplicationPath);
            return instanceApplicationPath;
        }

        public void TakeSnapshot(Snapshot argSnapshot)
        {
            File.WriteAllText(SnapshotFile, argSnapshot.ToJson(), new ASCIIEncoding());
        }

        public Snapshot GetSnapshot()
        {
            Snapshot rv = null;

            if (File.Exists(SnapshotFile))
            {
                string dropletsJson = File.ReadAllText(SnapshotFile, new ASCIIEncoding());
                rv = EntityBase.FromJson<Snapshot>(dropletsJson);
            }

            return rv;
        }

        public void CleanupInstanceDirectory(Instance argInstance)
        {
            if (false == disableDirCleanup)
            {
                string instanceDropletsPath, instanceApplicationPath;
                getInstancePaths(argInstance, out instanceDropletsPath, out instanceApplicationPath);
                try
                {
                    if (Directory.Exists(instanceDropletsPath))
                    {
                        Directory.Delete(instanceDropletsPath, true);
                    }
                    if (Directory.Exists(instanceApplicationPath))
                    {
                        Directory.Delete(instanceApplicationPath, true);
                    }
                }
                catch
                {
                    // TODO
                }
            }
        }

        public bool Stage(Droplet argDroplet, Instance argInstance)
        {
            bool rv = false;

            using (FileData file = getStagedApplicationFile(argDroplet.ExecutableUri))
            {
                if (null != file)
                {
                    string instanceDropletsPath, instanceApplicationPath;
                    getInstancePaths(argInstance, out instanceDropletsPath, out instanceApplicationPath);
                    Directory.CreateDirectory(instanceDropletsPath);
                    Directory.CreateDirectory(instanceApplicationPath);

                    using (var gzipStream = new GZipInputStream(file.FileStream))
                    {
                        var tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
                        tarArchive.ExtractContents(instanceDropletsPath);
                        tarArchive.Close();
                    }

                    var instanceApplicationDirInfo =  new DirectoryInfo(instanceApplicationPath);
                    Utility.CopyDirectory(new DirectoryInfo(Path.Combine(instanceDropletsPath, "app")), instanceApplicationDirInfo);

                    rv = true;
                }
            }

            return rv;
        }

        // TODO not a FilesManager kind of method
        public void BindServices(Droplet argDroplet, string argIIsName)
        {
            if (false == argDroplet.Services.IsNullOrEmpty())
            {
                Configuration c = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/", argIIsName);
                if (null != c)
                {
                    ConnectionStringsSection connectionStringsSection = c.GetSection("connectionStrings") as ConnectionStringsSection;
                    if (null != connectionStringsSection)
                    {
                        foreach (Service svc in argDroplet.Services.Where(s => s.IsMSSqlServer))
                        {
                            if (null != svc.Credentials)
                            {
                                SqlConnectionStringBuilder builder;
                                ConnectionStringSettings defaultConnectionStringSettings = connectionStringsSection.ConnectionStrings["Default"];
                                if (null != defaultConnectionStringSettings)
                                {
                                    builder = new SqlConnectionStringBuilder(defaultConnectionStringSettings.ConnectionString);
                                }
                                else
                                {
                                    builder = new SqlConnectionStringBuilder();
                                }

                                builder.DataSource = svc.Credentials.Host;

                                if (svc.Credentials.Password.IsNullOrWhiteSpace() || svc.Credentials.Username.IsNullOrWhiteSpace())
                                {
                                    builder.IntegratedSecurity = true;
                                }
                                else
                                {
                                    builder.UserID = svc.Credentials.Username;
                                    builder.Password = svc.Credentials.Password;
                                }

                                defaultConnectionStringSettings.ConnectionString = builder.ConnectionString;
                                // TODO
                                // builder.InitialCatalog 
                                break;
                            }
                        }
                    }
                    c.Save();
                }
            }
        }

        private void getInstancePaths(Instance argInstance,
            out string outInstanceDropletsPath, out string outInstanceApplicationPath)
        {
            outInstanceDropletsPath = Path.Combine(dropletsPath, argInstance.Dir);
            outInstanceApplicationPath = Path.Combine(ApplicationPath, argInstance.Dir);
        }

        private static FileData getStagedApplicationFile(string executableUri)
        {
            FileData rv = null;

            try
            {
                string tempFile = Path.GetTempFileName();

                var sw = new Stopwatch();
                sw.Start();
                using (var client = new WebClient())
                {
                    client.Proxy = null;
                    client.UseDefaultCredentials = false;
                    client.DownloadFile(executableUri, tempFile);
                }
                sw.Stop();
                Logger.Debug("Took {0} time to dowload from {1} to {2}", sw.Elapsed, executableUri, tempFile);

                rv = new FileData(new FileStream(tempFile, FileMode.Open), tempFile);
            }
            catch
            {
                // TODO
                // Can happen if there's a 404 or something.
            }

            return rv;
        }
    }
}