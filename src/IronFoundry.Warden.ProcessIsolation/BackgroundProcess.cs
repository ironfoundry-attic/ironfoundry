namespace IronFoundry.Warden.ProcessIsolation
{
    using System;
    using System.Diagnostics;

    [System.ComponentModel.DesignerCategory("Code")]
    public class BackgroundProcess : Process
    {
        public BackgroundProcess(string workingDirectory, string executable, string arguments)
        {
            StartInfo.FileName = executable;
            StartInfo.Arguments = arguments;
            StartInfo.WorkingDirectory = workingDirectory;
            StartInfo.CreateNoWindow = false;
            StartInfo.UseShellExecute = false;
            StartInfo.RedirectStandardInput = true;
            StartInfo.RedirectStandardOutput = true;
            StartInfo.RedirectStandardError = true;

            EnableRaisingEvents = true;
        }
    }
}
