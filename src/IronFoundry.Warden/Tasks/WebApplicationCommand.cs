namespace IronFoundry.Warden.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Containers;

    public class WebApplicationCommand : ProcessCommand
    {
        private readonly string port;

        public WebApplicationCommand(Container container, string[] arguments, bool shouldImpersonate) : base(container, arguments, shouldImpersonate)
        {
            if (arguments.IsNullOrEmpty() || String.IsNullOrWhiteSpace(arguments[0]))
            {
                throw new ArgumentException("expected port as first argument");
            }
            port = arguments[0];
        }

        protected override TaskCommandResult DoExecute()
        {
            var webRoot = Path.Combine(container.ContainerPath, "app");
            var args = String.Format(@"-webroot ""{0}"" -port {1}", webRoot, port);

            return RunProcess(container.ContainerPath, "iishost.exe", args);
        }
    }
}
