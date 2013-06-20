namespace IronFoundry.Warden.Utilities
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;
    using Microsoft.Win32.SafeHandles;
    using NLog;
    using PInvoke;

    [System.ComponentModel.DesignerCategory("Code")]
    public class BackgroundProcess : IDisposable
    {
        private static readonly NativeMethods.DuplicateOptions handleOptions = NativeMethods.DuplicateOptions.DUPLICATE_SAME_ACCESS;
        private static readonly NativeMethods.SecurityAttributes defaultPipeSecurityAttributes = new NativeMethods.SecurityAttributes { bInheritHandle = true };

        private readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly string workingDirectory;
        private readonly NetworkCredential networkCredential;
        private readonly StringBuilder commandLine;

        private const int bufferSize = 4096;
        private StreamWriter standardInput;
        private StreamReader standardOutput;
        private StreamReader standardError;

        private Process process;

        private bool disposed = false;

        public BackgroundProcess(string workingDirectory, string executable, string arguments, NetworkCredential networkCredential)
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
            this.workingDirectory = workingDirectory;

            if (executable.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("executable");
            }

            if (networkCredential == null)
            {
                throw new ArgumentNullException("networkCredential");
            }
            this.networkCredential = networkCredential;

            this.commandLine = GetCommandLine(executable, arguments);
        }

        public int Id
        {
            get { return process.Id; }
        }

        public StreamReader StdoutStream
        {
            get { return standardOutput; }
        }

        public StreamReader StderrStream
        {
            get { return standardError; }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                if (standardInput != null)
                {
                    standardInput.Dispose();
                    standardInput = null;
                }
                if (standardOutput != null)
                {
                    standardOutput.Dispose();
                    standardOutput = null;
                }
                if (standardError != null)
                {
                    standardError.Dispose();
                    standardError = null;
                }
                if (process != null)
                {
                    process.Dispose();
                }
            }
        }

        public bool HasExited
        {
            get { return process.HasExited; }
        }

        public int ExitCode
        {
            // TODO get { return process.ExitCode; }
            get { return 0; }
        }

        /*
         * TODO
        public void WaitForExit()
        {
            process.WaitForExit();
        }
         */

        public bool Start(Action<Process> postStartAction = null)
        {
            bool started = DoStart();

            if (postStartAction != null)
            {
                postStartAction(process);
            }

            /*
            standardInput.WriteLine(Environment.NewLine);
             */

            return started;
        }

        private bool DoStart()
        {
            if (commandLine == null)
            {
                throw new ArgumentNullException("cmdLine");
            }

            bool result = false;
            SafeUserTokenHandle userToken = null;

            var processInformation = new NativeMethods.ProcessInformation();

            var startupInfo = new NativeMethods.StartupInfo();
            startupInfo.lpDesktop = String.Empty; // NB: this is CRITICAL to get the process to execute.

            SafeFileHandle stdinHandle = null;
            SafeFileHandle stdoutHandle = null;
            SafeFileHandle stderrHandle = null;
            SafeProcessHandle processHandle = null;

            try
            {
                userToken = Utils.LogonAndGetUserPrimaryToken(networkCredential);
                if (userToken.IsInvalid)
                {
                    throw new WardenException("Error logging in as user '{0}'", networkCredential.UserName);
                }

                CreatePipe(out stdinHandle, out startupInfo.hStdInput, true);
                CreatePipe(out stdoutHandle, out startupInfo.hStdOutput, false);
                CreatePipe(out stderrHandle, out startupInfo.hStdError, false);

                /*
                 * NB:
                 * http://www.installsetupconfig.com/win32programming/windowstationsdesktops13_4.html
                 * http://support.microsoft.com/kb/165194
                 * http://stackoverflow.com/questions/4206190/win32-createprocess-when-is-create-unicode-environment-really-needed
                 */

                var processSecurityAttributes = new NativeMethods.SecurityAttributes();
                // processSecurityAttributes.bInheritHandle = true;

                var threadSecurityAttributes = new NativeMethods.SecurityAttributes();
                // threadSecurityAttributes.bInheritHandle = true;


                var dwCreationFlags = NativeMethods.CreateProcessFlags.CREATE_BREAKAWAY_FROM_JOB |
                    NativeMethods.CreateProcessFlags.CREATE_NO_WINDOW |
                    NativeMethods.CreateProcessFlags.CREATE_UNICODE_ENVIRONMENT;

                log.Trace("CreateProcessAsUser: '{0}'", commandLine.ToString());

                result = NativeMethods.CreateProcessAsUser(
                    hToken: (IntPtr)userToken,
                    lpApplicationName: null,
                    lpCommandLine: commandLine,
                    lpProcessAttributes: processSecurityAttributes,
                    lpThreadAttributes: threadSecurityAttributes,
                    // bInheritHandles: true,
                    bInheritHandles: false,
                    dwCreationFlags: dwCreationFlags,
                    lpEnvironment: IntPtr.Zero,
                    lpCurrentDirectory: workingDirectory,
                    lpStartupInfo: startupInfo,
                    lpProcessInformation: out processInformation);

                if (result == false)
                {
                    throw new Win32Exception();
                }
                else
                {
                    processHandle = new SafeProcessHandle(processInformation.hProcess);
                }
            }
            finally
            {
                startupInfo.Dispose();
                userToken.Dispose();
            }

            /*
            standardInput = new StreamWriter(new FileStream(stdinHandle, FileAccess.Write, bufferSize, false), Console.InputEncoding, bufferSize);
            standardInput.AutoFlush = true;
            standardOutput = new StreamReader(new FileStream(stdoutHandle, FileAccess.Read, bufferSize, false), Console.OutputEncoding, true, bufferSize);
            standardError = new StreamReader(new FileStream(stderrHandle, FileAccess.Read, bufferSize, false), Console.OutputEncoding, true, bufferSize);
             */

            if (processHandle == null || processHandle.IsInvalid)
            {
                Dispose();
                result = false;
            }
            else
            {
                try
                {
                    process = Process.GetProcessById(processInformation.dwProcessId);
                    NativeMethods.CloseHandle(processInformation.hThread);
                    result = true;
                }
                catch (Exception ex)
                {
                    log.ErrorException(ex);
                    Dispose();
                    result = false;
                }
            }

            return result;
        }

        private void CreatePipe(out SafeFileHandle parentHandle, out SafeFileHandle childHandle, bool parentInputs)
        {
            SafeFileHandle safeFileHandle = null;
            try
            {
                if (parentInputs)
                {
                    DoCreatePipe(out childHandle, out safeFileHandle);
                }
                else
                {
                    DoCreatePipe(out safeFileHandle, out childHandle);
                }

                HandleRef sourceProcessRef = Utils.GetCurrentProcessRef(this);
                HandleRef targetProcessRef = Utils.GetCurrentProcessRef(this);

                if (!NativeMethods.DuplicateHandle(sourceProcessRef, safeFileHandle, targetProcessRef, out parentHandle, 0, false, handleOptions))
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                if (safeFileHandle != null && !safeFileHandle.IsInvalid)
                {
                    safeFileHandle.Close();
                }
            }
        }

        private static void DoCreatePipe(out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe)
        {
            bool flag = NativeMethods.CreatePipe(out hReadPipe, out hWritePipe, defaultPipeSecurityAttributes, 0);
            if (!flag || hReadPipe.IsInvalid || hWritePipe.IsInvalid)
            {
                throw new Win32Exception();
            }
        }

        private static StringBuilder GetCommandLine(string executableFileName, string arguments)
        {
            var commandBuilder = new StringBuilder(1024);

            string executableTrimmed = executableFileName.Trim();

            bool executableIsQuoted = executableTrimmed.StartsWith("\"", StringComparison.Ordinal) && executableTrimmed.EndsWith("\"", StringComparison.Ordinal);
            if (!executableIsQuoted)
            {
                commandBuilder.Append("\"");
            }

            commandBuilder.Append(executableTrimmed);

            if (!executableIsQuoted)
            {
                commandBuilder.Append("\"");
            }

            if (!arguments.IsNullOrEmpty())
            {
                commandBuilder.Append(" ");
                commandBuilder.Append(arguments);
            }

            return commandBuilder;
        }
    }
}
