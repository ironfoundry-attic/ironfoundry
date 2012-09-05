namespace IronFoundry.Bosh.Agent.Handlers
{
    using IronFoundry.Bosh.Configuration;
    using Newtonsoft.Json.Linq;

    public class CompilePackage : BaseMessageHandler
    {
        public CompilePackage(IBoshConfig config) : base(config) { }

        public override HandlerResponse Handle(JObject parsed)
        {
            /*
             * agent/lib/agent/message/compile_package.rb
            # TODO implement sha1 verification
            # TODO propagate errors
            install_dependencies
            get_source_package
            unpack_source_package
            compile
            pack
            result = upload
            return { "result" => result }
             */

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