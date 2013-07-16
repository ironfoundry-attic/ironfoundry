namespace IronFoundry.Warden.ProcessIsolation
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public static class ProcessExtensions
    {
        /// <summary>
        /// Redirects the standard error and standard output streams and captures them in return value.
        /// </summary>
        /// <param name="process">The process to start with redirected output IO.</param>
        /// <param name="timeout">The time to wait for process to complete.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static ExecutableResult StartWithRedirectedOutputIO(this Process process, TimeSpan timeout, CancellationToken cancellationToken)
        {
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;

            string output = null;
            string errors = null;

            cancellationToken.Register(() => process.Kill());

            process.Start();

            var tasks = new Task[]
            {
                Task.Run(async () => output = await process.StandardOutput.ReadToEndAsync(), cancellationToken),
                Task.Run(async () => errors = await process.StandardError.ReadToEndAsync(), cancellationToken)
            };

            var ioReadTask = Task.WhenAll(tasks);
            Task.WhenAny(Task.Delay(timeout), ioReadTask).Wait();

            process.WaitForExit();

            var result = new ExecutableResult();

            if (!process.HasExited)
            {
                result.TimedOut = true;
                process.Kill();
            }

            result.ExitCode = process.ExitCode;
            result.StandardError = errors;
            result.StandardOut = output;
            return result;
        }

        public static async Task<ExecutableResult> StartWithRedirectedOutputIOAsync(this Process process, TimeSpan timeout, CancellationToken cancellationToken)
        {
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;

            string output = null;
            string errors = null;

            cancellationToken.Register(() => process.Kill());

            process.Start();

            var tasks = new Task[]
            {
                Task.Run(async () => output = await process.StandardOutput.ReadToEndAsync(), cancellationToken),
                Task.Run(async () => errors = await process.StandardError.ReadToEndAsync(), cancellationToken)
            };

            var ioReadTask = Task.WhenAll(tasks);
            await Task.WhenAny(Task.Delay(timeout), ioReadTask);

            process.WaitForExit();

            var result = new ExecutableResult();

            if (!process.HasExited)
            {
                result.TimedOut = true;
                process.Kill();
            }

            result.ExitCode = process.ExitCode;
            result.StandardError = errors;
            result.StandardOut = output;
            return result;
        }
    }

    public class ExecutableResult
    {
        public string StandardOut { get; set; }
        public string StandardError { get; set; }
        public bool TimedOut { get; set; }
        public int ExitCode { get; set; }
    }
}
