namespace IronFoundry.Warden.Tasks
{
    using System;
    using IronFoundry.Warden.Containers;

    public class PowershellCommand : TaskCommand
    {
        public PowershellCommand(Container container, string[] arguments)
            : base(container, arguments)
        {
        }

        public override TaskCommandResult Execute()
        {
            throw new NotImplementedException();
        }
    }
}
