namespace IronFoundry.Misc.Utilities
{
    using System;
    using System.Threading;
    using IronFoundry.Misc.Logging;
    using IronFoundry.Misc.Properties;

    public class ExecCmd
    {
        private readonly ILog log;
        private readonly string cmd;
        private readonly string arguments;

        public ExecCmd(ILog log, string cmd, string arguments)
        {
            this.log = log;
            this.cmd = cmd;
            this.arguments = arguments;
        }

        public ExecCmdResult Run(ushort numTries = 1, TimeSpan? retrySleepInterval = null, bool expectError = false)
        {
            bool success = false;
            string output = null, errout = null;
            try
            {
                for (ushort i = 0; i < numTries && false == success; ++i)
                {
                    log.Debug(Resources.ExecCmd_Cmd_Fmt, cmd, arguments);
                    var p = new RedirectedProcess(cmd, arguments);
                    p.StartAndWait();
                    output = p.STDOUT;
                    errout = p.STDERR;
                    success = 0 == p.ExitCode;
                    if (false == success)
                    {
                        if (false == expectError)
                        {
                            log.Error(Resources.ExecCmd_CmdFailed_Fmt, cmd, arguments, errout);
                        }
                        if (numTries > 1 && retrySleepInterval.HasValue)
                        {
                            Thread.Sleep(retrySleepInterval.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                output = null;
                log.Error(ex);
            }
            return new ExecCmdResult(success, output);
        }

        public override string ToString()
        {
            return cmd + " " + arguments;
        }
    }
}