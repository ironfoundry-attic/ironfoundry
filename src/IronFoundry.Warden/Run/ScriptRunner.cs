namespace IronFoundry.Warden.Run
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using IronFoundry.Warden.Configuration;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Jobs;
    using Newtonsoft.Json;

    public class ScriptRunner : IJobRunnable
    {
        private static readonly WardenConfig config = new WardenConfig();
        private static readonly IDictionary<string, Func<Container, string[], ScriptResult[]>> commandHandlers =
            new Dictionary<string, Func<Container, string[], ScriptResult[]>>
        {
            {
                "mkdir", (container, args) => {
                    var results = new List<ScriptResult>();
                    foreach (string dir in args)
                    {
                        try
                        {
                            string toCreate = dir;
                            if (dir.StartsWith("CROOT"))
                            {
                                toCreate = dir.Replace("CROOT", Path.Combine(config.ContainerBasePath, container.Handle)); 
                            }
                            Directory.CreateDirectory(toCreate);
                            results.Add(new ScriptResult(0, String.Format("mkdir: created directory '{0}'", toCreate), null));
                        }
                        catch (Exception ex)
                        {
                            results.Add(new ScriptResult(1, null, ex.Message));
                        }
                    }
                    return results.ToArray();
                }
            }, // mkdir
        };

        private readonly Container container;
        private readonly RunCommand[] commands;

        /*
         * Find container
         * parse JSON
            commands = [
              { :cmd => 'mkdir', :args => [ 'CROOT/app' ] },
              { :cmd => 'touch', :args => [ 'CROOT/app/support_heroku_buildpacks' ] },
              # NB: chown not necessary as /app will inherit perms
            ]
         * create handlers for :cmd objects
         * execute as the user if necessary and ensure all succeed
         * return output
         */
        public ScriptRunner(Container container, string script)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            this.container = container;

            if (script.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("script");
            }

            commands = JsonConvert.DeserializeObject<RunCommand[]>(script);
            if (commands.IsNullOrEmpty())
            {
                throw new ArgumentException("Expected to run at least one command.");
            }
        }

        public IJobResult Run()
        {
            var results = new List<ScriptResult>();

            foreach (RunCommand cmd in commands)
            {
                if (commandHandlers.ContainsKey(cmd.Command))
                {
                    var handler = commandHandlers[cmd.Command];
                    try
                    {
                        // TODO impersonation vs. privileged
                        results.AddRange(handler(container, cmd.Args));
                    }
                    catch (Exception ex)
                    {
                        results.Add(new ScriptResult(1, null, ex.Message));
                    }
                }
            }

            return ScriptResult.Flatten(results);
        }
    }
}
