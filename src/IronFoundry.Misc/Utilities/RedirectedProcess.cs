namespace IronFoundry.Misc.Utilities
{
    using System;
    using System.Diagnostics;

    [System.ComponentModel.DesignerCategory(@"Code")]
    public class RedirectedProcess : Process
    {
        public RedirectedProcess(string exe, string args)
        {
            var psi = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = exe,
                Arguments = args,
                WorkingDirectory = Environment.CurrentDirectory,
            };
            this.StartInfo = psi;
        }

        public void StartAndWait()
        {
            this.Start();
            this.WaitForExit();
            STDOUT = this.StandardOutput.ReadToEnd().TrimEnd('\r', '\n');
            STDERR = this.StandardError.ReadToEnd().TrimEnd('\r', '\n');
        }

        public string STDOUT { get; private set; }
        public string STDERR { get; private set; }

        public void AddEnvironmentVariable(string variable, string value)
        {
            var env = this.StartInfo.EnvironmentVariables;
            if (false == env.ContainsKey(variable))
            {
                env.Add(variable, value);
            }
        }
    }
}