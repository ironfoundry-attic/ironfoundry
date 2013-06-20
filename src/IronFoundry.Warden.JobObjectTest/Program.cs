namespace IronFoundry.Warden.JobObjectTest
{
    using System;
    using System.Diagnostics;
    using CommandLine;
    using CommandLine.Text;
    using IronFoundry.Warden.Utilities;
    using NLog;
    using Tasks;
    using Utilities.JobObjects;

    class Program
    {
        private static JobObject jobObject;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            log.Trace("Parsing options...");
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options))
            {
                Environment.Exit(1);
            }

            log.Trace(String.Concat("Command: IronFoundry.Warden.JobObjectTest.exe ", String.Join(" ", args)));

            var executable = "powershell.exe";
            var arguments = String.Format("Write-Host \"Running as: $(whoami)\"; Get-Date; Sleep {0}; Get-Date", options.DelaySeconds);
            var postStartAction = new Action<Process>(process =>
            {
                try
                {
                    log.Trace("Adding process to job object...");
                    jobObject = new JobObject("IronFoundry.Warden.JobObjectTest");
                    jobObject.DieOnUnhandledException = true;
                    jobObject.KillProcessesOnJobClose = true;
                    jobObject.AddProcess(process);
                }
                catch (Exception ex)
                {
                    log.ErrorException("Error adding process to job object", ex);
                }
            });

            using (var process = new BackgroundProcess(Environment.CurrentDirectory, executable, arguments))
            {
                process.ErrorDataReceived += (s,e) => { if(e.Data != null) Console.WriteLine(e.Data); };
                process.OutputDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };

                log.Trace("Starting test process...");
                process.StartAndWait(false, postStartAction);

                log.Trace("Process ended with exit code: {0}", process.ExitCode);
            }

            Console.WriteLine("Press [\\R\\N] to end");
            Console.ReadLine();

            if (jobObject != null)
            {
                jobObject.Dispose();
            }
        }
    }

    class Options
    {
        [Option('d', "delaySeconds", Required = false, DefaultValue = 10, HelpText = "The time in seconds of how long the test command pauses before exit.")]
        public int DelaySeconds { get; set; }

        [HelpOption]
        public string Usage()
        {
            return HelpText.AutoBuild(this, c => HelpText.DefaultParsingErrorsHandler(this, c));
        }
    }
}
