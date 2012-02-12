namespace IronFoundry.Dea.Agent
{
    using System;
    using System.Diagnostics;
    using System.IO;
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
        private readonly IConfig config;
        private readonly bool disableDirCleanup = false;

        private readonly string dropletsPath;
        private readonly string applicationPath;

        public FilesManager(ILog log, IConfig config)
        {
            this.log = log;
            this.config = config;

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
            try
            {
                File.WriteAllText(SnapshotFile, snapshot.ToJson(), new ASCIIEncoding());
            }
            catch { }
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
                // Can happen if there's a 404 or something.
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