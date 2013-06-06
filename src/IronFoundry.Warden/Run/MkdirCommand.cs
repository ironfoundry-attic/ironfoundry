namespace IronFoundry.Warden.Run
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using IronFoundry.Warden.Configuration;
    using IronFoundry.Warden.Containers;

    public class MkdirCommand : TaskCommand
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

        public override TaskCommandResult Execute()
        {
            TaskCommandResult finalResult = null;
            var createdDirs = new StringBuilder();

            foreach (string dir in arguments)
            {
                try
                {
                    string dirTmp = dir.Trim();
                    string toCreate = dirTmp;
                    if (dirTmp.StartsWith("CROOT"))
                    {
                        toCreate = dirTmp.Replace("CROOT", Path.Combine(config.ContainerBasePath, container.Handle)).ToWinPathString(); 
                    }
                    else if (dirTmp.StartsWith("/"))
                    {
                        toCreate = Path.Combine(config.ContainerBasePath, container.Handle, dirTmp.Remove(0, 1));
                    }
                    Directory.CreateDirectory(toCreate);
                    createdDirs.AppendFormat("mkdir: created directory '{0}'", toCreate).AppendLine();
                }
                catch (Exception ex)
                {
                    finalResult = new TaskCommandResult(1, null, ex.Message);
                    break;
                }
            }

            if (finalResult == null)
            {
                string stdout = createdDirs.ToString();
                finalResult = new TaskCommandResult(0, stdout, null);
            }

            return finalResult;
        }
    }
}
