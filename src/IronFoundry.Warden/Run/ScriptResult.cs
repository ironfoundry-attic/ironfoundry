namespace IronFoundry.Warden.Run
{
    using System.Collections.Generic;
    using System.Text;
    using IronFoundry.Warden.Jobs;

    public class ScriptResult : IJobResult
    {
        private readonly int exitCode;
        private readonly string stdout;
        private readonly string stderr;

        public ScriptResult(int exitCode, string stdout, string stderr)
        {
            this.exitCode = exitCode;
            this.stdout = stdout;
            this.stderr = stderr;
        }

        public int ExitCode
        {
            get { return exitCode; }
        }

        public string Stdout
        {
            get { return stdout; }
        }

        public string Stderr
        {
            get { return stderr; }
        }

        public static IJobResult Flatten(IEnumerable<ScriptResult> results)
        {
            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            int lastExitCode = 0;
            foreach (ScriptResult rslt in results)
            {
                stdout.SmartAppendLine(rslt.Stdout);
                stderr.SmartAppendLine(rslt.Stderr);
                if (rslt.ExitCode != 0)
                {
                    lastExitCode = rslt.ExitCode;
                    break;
                }
            }

            return new ScriptResult(lastExitCode, stdout.ToString(), stderr.ToString());
        }
    }
}
