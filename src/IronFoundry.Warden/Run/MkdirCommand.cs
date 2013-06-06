namespace IronFoundry.Warden.Run
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using IronFoundry.Warden.Configuration;
    using IronFoundry.Warden.Containers;

    public class MkdirCommand : PathCommand
    {
        private static readonly WardenConfig config = new WardenConfig();

        public MkdirCommand(Container container, string[] arguments)
            : base(container, arguments)
        {
            if (base.arguments.IsNullOrEmpty())
            {
                throw new ArgumentNullException("mkdir command requires at least one argument.");
            }
        }

        protected override void ProcessPathInContainer(string pathInContainer, StringBuilder output)
        {
            Directory.CreateDirectory(pathInContainer);
            output.AppendFormat("mkdir: created directory '{0}'", pathInContainer).AppendLine();
        }
    }
}
