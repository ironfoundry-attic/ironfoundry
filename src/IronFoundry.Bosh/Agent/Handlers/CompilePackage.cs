namespace IronFoundry.Bosh.Agent.Handlers
{
    using System.IO;
    using IronFoundry.Bosh.Configuration;
    using IronFoundry.Misc.Logging;
    using Newtonsoft.Json.Linq;

    public class CompilePackage : BaseMessageHandler
    {
        private readonly ILog log;

        private readonly string dataDirPath;
        private readonly string tmpDirPath;
        private readonly string logFilePath;
        private readonly string compileBasePath;
        private readonly string installBasePath;

        public CompilePackage(IBoshConfig config, ILog log) : base(config)
        {
            dataDirPath = Path.Combine(config.BaseDir, "data");
            tmpDirPath = Path.Combine(dataDirPath, "tmp");
            Directory.CreateDirectory(tmpDirPath);
            logFilePath = Path.Combine(tmpDirPath, config.AgentID);
            compileBasePath = Path.Combine(dataDirPath, "compile");
            installBasePath = Path.Combine(dataDirPath, "packages");

            this.log = log;
        }

        public override HandlerResponse Handle(JObject parsed)
        {
            var args = parsed["arguments"];
            @blobstore_id, @sha1, @package_name, @package_version, @dependencies = args

            // agent/lib/agent/message/compile_package.rb
            InstallDependencies();
            GetSourcePackage();
            UnpackSourcePackage();
            Compile();
            Pack();
            string result = Upload();
            return new HandlerResponse(result);
        }

        private void InstallDependencies()
        {
        }

        private void GetSourcePackage()
        {
        }

        private void UnpackSourcePackage()
        {
        }

        private void Compile()
        {
        }

        private void Pack()
        {
        }

        private string Upload()
        {
        }
    }
}