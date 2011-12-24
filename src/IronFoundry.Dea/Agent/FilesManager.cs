namespace IronFoundry.Dea.Agent
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using ICSharpCode.SharpZipLib.GZip;
    using ICSharpCode.SharpZipLib.Tar;
    using IronFoundry.Dea.Config;
    using IronFoundry.Dea.Logging;
    using IronFoundry.Dea.Types;

    public class FilesManager : IFilesManager
    {
        private readonly ILog log;
        private readonly bool disableDirCleanup = false;

        private readonly string dropletsPath;
        private readonly string applicationPath;

        public FilesManager(ILog log, IConfig config)
        {
            this.log = log;

            disableDirCleanup = config.DisableDirCleanup;
            dropletsPath      = config.DropletDir;
            applicationPath   = config.AppDir;

            Directory.CreateDirectory(dropletsPath);
            Directory.CreateDirectory(applicationPath);

            SnapshotFile = Path.Combine(dropletsPath, "snapshot.json");
        }

        public string SnapshotFile { get; private set; }

        public string GetApplicationPathFor(Instance instance)
        {
            InstancePaths paths = GetInstancePaths(instance);
            return paths.FullAppPath;
        }

        public void TakeSnapshot(Snapshot snapshot)
        {
            File.WriteAllText(SnapshotFile, snapshot.ToJson(), new ASCIIEncoding());
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

        public void CleanupInstanceDirectory(Instance instance)
        {
            CleanupInstanceDirectory(instance, false);
        }

        public void CleanupInstanceDirectory(Instance instance, bool force = false)
        {
            if (force || (false == disableDirCleanup))
            {
                InstancePaths paths = GetInstancePaths(instance);
                try
                {
                    if (Directory.Exists(paths.DropletsPath))
                    {
                        Directory.Delete(paths.DropletsPath, true);
                    }
                    if (Directory.Exists(paths.BaseAppPath))
                    {
                        Directory.Delete(paths.BaseAppPath, true);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }
        }

        public bool Stage(Droplet droplet, Instance instance)
        {
            bool rv = false;

            using (FileData file = GetStagedApplicationFile(droplet.ExecutableUri))
            {
                if (null != file)
                {
                    InstancePaths paths = GetInstancePaths(instance);
                    Directory.CreateDirectory(paths.DropletsPath);
                    Directory.CreateDirectory(paths.BaseAppPath);

                    using (var gzipStream = new GZipInputStream(file.FileStream))
                    {
                        var tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
                        tarArchive.ExtractContents(paths.DropletsPath);
                        tarArchive.Close();
                    }

                    var instanceApplicationDirInfo =  new DirectoryInfo(paths.BaseAppPath);
                    Utility.CopyDirectory(new DirectoryInfo(paths.DropletsPath), instanceApplicationDirInfo);

                    rv = true;
                }
            }

            return rv;
        }

        // TODO not a FilesManager kind of method
        public void BindServices(Droplet droplet, string appPath)
        {
            if (false == droplet.Services.IsNullOrEmpty())
            {
                Configuration c = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/", appPath);
                if (null != c)
                {
                    ConnectionStringsSection connectionStringsSection = c.GetSection("connectionStrings") as ConnectionStringsSection;
                    if (null != connectionStringsSection)
                    {
                        foreach (Service svc in droplet.Services.Where(s => s.IsMSSqlServer))
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

                                if (false == svc.Credentials.Name.IsNullOrWhiteSpace())
                                {
                                    builder.InitialCatalog = svc.Credentials.Name;
                                }

                                defaultConnectionStringSettings.ConnectionString = builder.ConnectionString;
                                break;
                            }
                        }
                    }
                    c.Save();
                }
            }
        }

        private FileData GetStagedApplicationFile(string executableUri)
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
                log.Debug("Took {0} time to dowload from {1} to {2}", sw.Elapsed, executableUri, tempFile);

                rv = new FileData(new FileStream(tempFile, FileMode.Open), tempFile);
            }
            catch
            {
                // TODO Can happen if there's a 404 or something.
            }

            return rv;
        }

        private InstancePaths GetInstancePaths(Instance instance)
        {
            return new InstancePaths(
                dropletsPath: Path.Combine(dropletsPath, instance.Staged),
                baseAppPath: instance.Dir,
                fullAppPath: Path.Combine(instance.Dir, "app"));
        }

        private class InstancePaths
        {
            public string DropletsPath { get; private set; }
            public string BaseAppPath { get; private set; }
            public string FullAppPath { get; private set; }

            public InstancePaths(string dropletsPath, string baseAppPath, string fullAppPath)
            {
                this.DropletsPath = dropletsPath;
                this.BaseAppPath = baseAppPath;
                this.FullAppPath = fullAppPath;
            }
        }
    }
}