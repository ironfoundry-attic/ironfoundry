namespace IronFoundry.Warden.Run
{
    using System;
    using IronFoundry.Warden.Containers;

    public class TaskCommandFactory
    {
        private readonly Container container;

        public TaskCommandFactory(Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            this.container = container;
        }

        public TaskCommand Create(string commandName, string[] arguments)
        {
            TaskCommand command = null;

            switch (commandName)
            {
                case "mkdir" :
                    command =  new MkdirCommand(container, arguments);
                    break;
                default :
                    throw new InvalidOperationException(String.Format("Unknown script command: '{0}'", commandName));
            }

            return command;
        }
    }
}
