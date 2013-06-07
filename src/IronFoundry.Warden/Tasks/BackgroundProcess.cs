namespace IronFoundry.Warden.Tasks
{
    using System;
    using System.Diagnostics;
    using System.IO;

    [System.ComponentModel.DesignerCategory("Code")]
    public class BackgroundProcess : Process
    {
        private readonly ProcessStartInfo processStartInfo;

        public BackgroundProcess(string workingDirectory, string executable, string arguments)
            : base()
        {
            if (workingDirectory.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("workingDirectory");
            }
            if (!Directory.Exists(workingDirectory))
            {
                throw new ArgumentException("workingDirectory must exist.");
            }

            if (executable.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("executable");
            }

            this.processStartInfo                        = new ProcessStartInfo(executable);
            this.processStartInfo.Arguments              = arguments;
            this.processStartInfo.CreateNoWindow         = true;
            this.processStartInfo.RedirectStandardInput  = true;
            this.processStartInfo.RedirectStandardOutput = true;
            this.processStartInfo.RedirectStandardError  = true;
            this.processStartInfo.UseShellExecute        = false;
            this.processStartInfo.WindowStyle            = ProcessWindowStyle.Hidden;
            this.processStartInfo.WorkingDirectory       = workingDirectory;

            this.StartInfo = this.processStartInfo;
            this.EnableRaisingEvents = true;
        }

        public void StartBackground()
        {
            Start();
            BeginErrorReadLine();
            BeginOutputReadLine();
            StandardInput.WriteLine(Environment.NewLine);
            WaitForExit(); // TODO timeout
        }
    }
}
