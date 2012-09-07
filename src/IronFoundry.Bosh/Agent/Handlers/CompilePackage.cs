namespace IronFoundry.Bosh.Agent.Handlers
{
    using System;
    using System.IO;
    using ICSharpCode.SharpZipLib.GZip;
    using ICSharpCode.SharpZipLib.Tar;
    using IronFoundry.Bosh.Blobstore;
    using IronFoundry.Bosh.Configuration;
    using IronFoundry.Bosh.Properties;
    using IronFoundry.Misc.Logging;
    using IronFoundry.Misc.Utilities;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class CompilePackage : BaseMessageHandler
    {
        private const string PackagingScriptName = "packaging";
        private const string PackagingScriptNamePS1 = "packaging.ps1";

        private readonly ILog log;
        private readonly IBlobstoreClientFactory blobstoreClientFactory;

        private readonly string dataDirPath;
        private readonly string tmpDirPath;
        private readonly string logFilePath;
        private readonly string compileBasePath;
        private readonly string installBasePath;

        private string blobstoreID;
        private string sha1;
        private string packageName;
        private string packageVersion;

        private string sourceFile;
        private string compileDir;
        private string installDir;

        public CompilePackage(IBoshConfig config, ILog log, IBlobstoreClientFactory blobstoreClientFactory)
            : base(config)
        {
            dataDirPath = Path.Combine(config.BaseDir, "data");
            tmpDirPath = Path.Combine(dataDirPath, "tmp");
            Directory.CreateDirectory(tmpDirPath);
            logFilePath = Path.Combine(tmpDirPath, config.AgentID);
            compileBasePath = Path.Combine(dataDirPath, "compile");
            installBasePath = Path.Combine(dataDirPath, "packages");

            this.log = log;
            this.blobstoreClientFactory = blobstoreClientFactory;

            log.AddFileTarget(config.AgentID, logFilePath);
        }

        public override HandlerResponse Handle(JObject parsed)
        {
            try
            {
                var args = parsed["arguments"];

                blobstoreID = (string)args[0];
                sha1 = (string)args[1];
                packageName = (string)args[2];
                packageVersion = (string)args[3];
                var dependencies = args[4];

                // agent/lib/agent/message/compile_package.rb
                InstallDependencies(dependencies);
                GetSourcePackage();
                UnpackSourcePackage();
                Compile();
                Pack();
                UploadResult result = Upload();
                return new HandlerResponse(new JObject(new JProperty("result", result)));
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw new MessageHandlerException(ex);
            }
            finally
            {
                ClearLogFile();
                DeleteTmpFiles();
            }
        }

        private void InstallDependencies(JToken dependencies)
        {
            log.Info(Resources.CompilePackage_InstallingDependencies_Message);
            foreach (var d in dependencies)
            {
                var prop = (JProperty)d;
                var val = prop.Value;

                string depPkgName     = prop.Name;
                string depBlobstoreID = (string)val["blobstore_id"];
                string depSha1        = (string)val["sha1"];
                string depVersion     = (string)val["version"];

                log.Info(Resources.CompilePackage_InstallingDependency_Fmt, depPkgName, val.ToString());
                /*
                 * TODO
          install_dir = File.join(@install_base, pkg_name, pkg['version'])
          Util.unpack_blob(blobstore_id, sha1, install_dir)
          pkg_link_dst = File.join(@base_dir, 'packages', pkg_name)
          FileUtils.ln_sf(install_dir, pkg_link_dst)
                 */
            }
        }

        private void GetSourcePackage()
        {
            string compileTmp = Path.Combine(compileBasePath, "tmp");
            Directory.CreateDirectory(compileTmp);
            sourceFile = Path.Combine(compileTmp, blobstoreID);
            File.Delete(sourceFile);
            BlobstoreClient client = blobstoreClientFactory.Create();
            client.Get(blobstoreID, sourceFile);
        }

        private string CompileDir
        {
            get
            {
                if (null == compileDir)
                {
                    compileDir = Path.Combine(compileBasePath, packageName);
                }
                return compileDir;
            }
        }

        private string InstallDir
        {
            get
            {
                if (null == installDir)
                {
                    installDir = Path.Combine(installBasePath, packageName, packageVersion);
                }
                return installDir;
            }
        }

        private void UnpackSourcePackage()
        {
            if (Directory.Exists(CompileDir))
            {
                Directory.Delete(CompileDir, true);
            }
            Directory.CreateDirectory(CompileDir);

            try
            {
                using (var fs = File.OpenRead(sourceFile))
                {
                    using (var gzipStream = new GZipInputStream(fs))
                    {
                        using (var tarArchive = TarArchive.CreateInputTarArchive(gzipStream))
                        {
                            tarArchive.ExtractContents(CompileDir);
                            tarArchive.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                /*
                 * TODO
                  unless $?.exitstatus == 0
                    raise Bosh::Agent::MessageHandlerError.new(
                      "Compile Package Unpack Source Failure (exit code: #{$?.exitstatus})",
                      output)
                 */
                log.Error(ex);
                throw;
            }
        }

        private void Compile()
        {
            if (Directory.Exists(InstallDir))
            {
                Directory.Delete(InstallDir, true);
            }

            /* TODO
        pct_space_used = pct_disk_used(@compile_base)
        if pct_space_used >= @max_disk_usage_pct
          raise Bosh::Agent::MessageHandlerError,
              "Compile Package Failure. Greater than #{@max_disk_usage_pct}% " +
              "is used (#{pct_space_used}%."
        end
             */

            using (DirectoryScope.Create(CompileDir))
            {
                if (File.Exists(PackagingScriptName))
                {
                    log.Info(Resources.CompilePackage_CompilingPackage_Fmt, packageName, packageVersion);

                    // NB: has to have .ps1 extension
                    File.Copy(PackagingScriptName, PackagingScriptNamePS1);
                    string stdout, stderr;
                    int exitcode;
                    using (var exe = new PowershellExecutor(PackagingScriptNamePS1))
                    {
                        exe.StartAndWait();
                        stdout = exe.STDOUT;
                        stderr = exe.STDERR;
                        exitcode = exe.ExitCode;
                    }
                    if (exitcode != 0)
                    {
                        throw new MessageHandlerException(
                            String.Format(Resources.CompilePackage_CompilePackageFailure_Fmt, exitcode),
                            String.Join(" / ", stdout, stderr));
                    }
                    log.Info(stdout);
                }
            }
        }

        private string CompiledPackage
        {
            get { return sourceFile + ".compiled"; }
        }

        private void Pack()
        {
            log.Info(Resources.CompilePackage_Packing_Fmt, packageVersion, packageVersion);
            string installDirTmp = InstallDir.Replace('\\', '/'); // NB: TarArchive requires forward slashes
            using (var fs = File.OpenWrite(CompiledPackage))
            {
                using (var gzipStream = new GZipOutputStream(fs))
                {
                    using (var tarArchive = TarArchive.CreateOutputTarArchive(gzipStream))
                    {
                        tarArchive.RootPath = installDirTmp;
                        var tarEntry = TarEntry.CreateEntryFromFile(installDirTmp);
                    	tarArchive.WriteEntry(tarEntry, true);
                    }
                }
            }
        }

        private class UploadResult
        {
            private string sha1;
            private string blobstoreID;
            private string compileLogID;

            [JsonProperty(PropertyName = "sha1")]
            public string SHA1 { get { return sha1; } }
            [JsonProperty(PropertyName = "blobstore_id")]
            public string BlobstoreID { get { return blobstoreID; } }
            [JsonProperty(PropertyName = "compile_log_id")]
            public string CompileLogID { get { return compileLogID; } }

            public UploadResult(string sha1, string blobstoreID, string compileLogID)
            {
                this.sha1 = sha1;
                this.blobstoreID = blobstoreID;
                this.compileLogID = compileLogID;
            }
        }

        private UploadResult Upload()
        {
            BlobstoreClient client = blobstoreClientFactory.Create();
            string compiledBlobstoreID = client.Create(CompiledPackage);
            var fiCompiledPackage = new FileInfo(CompiledPackage);
            string compiledSha1 = fiCompiledPackage.Hexdigest();
            string compileLogID = client.Create(logFilePath); // TODO
            log.Info(Resources.CompilePackage_Uploaded_Fmt, packageName, packageVersion, compiledSha1, compiledBlobstoreID);
            return new UploadResult(compiledSha1, compiledBlobstoreID, compileLogID);
        }

        private void DeleteTmpFiles()
        {
            try
            {
                foreach (var dir in new[] { compileBasePath, installBasePath })
                {
                    Directory.Delete(dir, true);
                }
            }
            catch (Exception ex)
            {
                log.Warn(Resources.CompilePackage_ErrorDeleting_Fmt, ex.Message);
            }
        }

        private void ClearLogFile()
        {
            File.Delete(logFilePath);
        }
    }
}