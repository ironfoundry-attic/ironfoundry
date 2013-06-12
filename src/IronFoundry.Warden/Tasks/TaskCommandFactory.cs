namespace IronFoundry.Warden.Tasks
{
    using System;
    using IronFoundry.Warden.Containers;

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
            TaskCommand command = null;

            switch (commandName)
            {
                case "mkdir" :
                    command =  new MkdirCommand(container, arguments);
                    break;
                case "touch" :
                    command =  new TouchCommand(container, arguments);
                    break;
                case "ps1" :
                    command =  new PowershellCommand(container, arguments, shouldImpersonate);
                    break;
                case "unzip" :
                    command =  new UnzipCommand(container, arguments);
                    break;
                default :
                    throw new InvalidOperationException(String.Format("Unknown script command: '{0}'", commandName));
            }

            return command;
        }
    }
}
