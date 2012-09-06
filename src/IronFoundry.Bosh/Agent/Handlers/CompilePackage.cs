namespace IronFoundry.Bosh.Agent.Handlers
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using ICSharpCode.SharpZipLib.GZip;
    using ICSharpCode.SharpZipLib.Tar;
    using IronFoundry.Bosh.Blobstore;
    using IronFoundry.Bosh.Configuration;
    using IronFoundry.Misc.Logging;
    using IronFoundry.Misc.Utilities;
    using Newtonsoft.Json.Linq;

    public class CompilePackage : BaseMessageHandler
    {
        private const string PackagingScriptName = "packaging";

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
                object result = Upload();
                return new HandlerResponse(new { result = result });
            }
            finally
            {
                ClearLogFile();
                DeleteTmpFiles();
            }
        }

        private void InstallDependencies(JToken dependencies)
        {
            foreach (var d in dependencies)
            {
                var prop = (JProperty)d;
                var val = prop.Value;

                string depPkgName = prop.Name;
                string depBlobstoreID = (string)val["blobstore_id"];
                string depSha1 = (string)val["sha1"];
                string depVersion = (string)val["version"];
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
                    using (var runspace = RunspaceFactory.CreateRunspace())
                    {
                        runspace.Open();
                        runspace.SessionStateProxy.SetVariable("BoshCompileTarget", CompileDir);
                        runspace.SessionStateProxy.SetVariable("BoshInstallTarget", InstallDir);
                        using (Pipeline pipeline = runspace.CreatePipeline())
                        {
                            var cmd = new Command(PackagingScriptName);
                            pipeline.Commands.Add(cmd);
                            Collection<PSObject> results = pipeline.Invoke();
                        }
                        runspace.Close();
                        /* TODO
                        unless $?.exitstatus == 0
                          raise Bosh::Agent::MessageHandlerError.new(
                            "Compile Package Failure (exit code: #{$?.exitstatus})", output)
                        end
                         */
                    }
                }
            }
        }

        private string CompiledPackage
        {
            get { return sourceFile + ".compiled"; }
        }

        private void Pack()
        {
                // TODO @logger.info("Packing #{@package_name} #{@package_version}")
                /*
                Dir.chdir(install_dir) do
                  `tar -zcf #{compiled_package} .`
                end
                 */
            string installDirTmp = InstallDir.Replace('\\', '/');
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

        private object Upload()
        {
            BlobstoreClient client = blobstoreClientFactory.Create();
            string compiledBlobstoreID = client.Create(CompiledPackage);
            var fiCompiledPackage = new FileInfo(CompiledPackage);
            string compiledSha1 = fiCompiledPackage.Hexdigest();
            string compileLogID = client.Create(logFilePath); // TODO
            return new { sha1 = compiledSha1, blobstore_id = compiledBlobstoreID, compile_log_id = compileLogID };
            /*
        compiled_blobstore_id = nil
        File.open(compiled_package, 'r') do |f|
          compiled_blobstore_id = @blobstore_client.create(f)
        end
        compiled_sha1 = Digest::SHA1.hexdigest(File.read(compiled_package))
        compile_log_id = @blobstore_client.create(@log_file)
        @logger.info("Uploaded #{@package_name} #{@package_version} " +
                     "(sha1: #{compiled_sha1}, " +
                     "blobstore_id: #{compiled_blobstore_id})")
        @logger = nil
        { "sha1" => compiled_sha1, "blobstore_id" => compiled_blobstore_id,
          "compile_log_id" => compile_log_id }
             */
        }

        private void DeleteTmpFiles()
        {
        }

        private void ClearLogFile()
        {
            /*
      # Clears the log file after a compilation runs.  This is needed because if
      # reuse_compilation_vms is being used then without clearing the log then
      # the log from each subsequent compilation will include the previous
      # compilation's output.
      # @param [String] log_file Path to the log file.
      def clear_log_file(log_file)
        File.delete(log_file) if File.exists?(log_file)
        @logger = Logger.new(log_file)
      end
             */
        }
    }
}