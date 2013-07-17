namespace IronFoundry.Warden.Tasks
{
    using System;
    using Containers;
    using Protocol;

    public class TaskCommandFactory
    {
        private readonly Container container;
        private readonly ResourceLimits rlimits;

        public TaskCommandFactory(Container container, ResourceLimits rlimits)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            this.container = container;
            this.rlimits = rlimits;
        }

        public TaskCommand Create(string commandName, string[] arguments)
        {
            switch (commandName)
            {
                case "exe" :
                    return new ExeCommand(container, arguments, rlimits);
                case "mkdir" :
                    return new MkdirCommand(container, arguments);
                case "iis" :
                    return new WebApplicationCommand(container, arguments, rlimits);
                case "ps1" :
                    return new PowershellCommand(container, arguments, rlimits);
                case "replace-tokens" :
                    return new ReplaceTokensCommand(container, arguments);
                case "tar" :
                    return new TarCommand(container, arguments);
                case "touch" :
                    return new TouchCommand(container, arguments);
                case "unzip" :
                    return new UnzipCommand(container, arguments);
                default :
                    throw new InvalidOperationException(String.Format("Unknown script command: '{0}'", commandName));
            }
        }
    }
}
