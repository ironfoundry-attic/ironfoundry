namespace IronFoundry.Warden.Tasks
{
    using System;
    using Containers;

    public class TaskCommandFactory
    {
        private readonly Container container;
        private readonly bool shouldImpersonate;

        public TaskCommandFactory(Container container, bool shouldImpersonate)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            this.container = container;
            this.shouldImpersonate = shouldImpersonate;
        }

        public TaskCommand Create(string commandName, string[] arguments)
        {
            switch (commandName)
            {
                case "mkdir" :
                    return new MkdirCommand(container, arguments);
                case "touch" :
                    return new TouchCommand(container, arguments);
                case "ps1" :
                    return new PowershellCommand(container, arguments, shouldImpersonate);
                case "unzip" :
                    return new UnzipCommand(container, arguments);
                case "iis" :
                    return new WebApplicationCommand(container, arguments, shouldImpersonate);
                default :
                    throw new InvalidOperationException(String.Format("Unknown script command: '{0}'", commandName));
            }
        }
    }
}
