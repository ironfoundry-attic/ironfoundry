namespace IronFoundry.Bosh.Test
{
    using System;
    using System.IO;
    using IronFoundry.Misc.Logging;
    using Xunit;

    public class LoggingTests
    {
        [Fact]
        public void Add_File_Target()
        {
            string tmpFile = Path.GetTempFileName();
            ILog log = new NLogLogger("Test_Logger");
            log.AddFileTarget("Test_Target", tmpFile);
            log.Debug("DEBUG MESSAGE");
            log.Info("INFO MESSAGE");
            log.Error("ERROR MESSAGE");
            log.Warn("WARN MESSAGE");
            log.Fatal("FATAL MESSAGE");
            log.Flush();

            string logContents = File.ReadAllText(tmpFile);
            Console.Write(logContents);

            Assert.True(logContents.Contains("DEBUG MESSAGE"));
            Assert.True(logContents.Contains("INFO MESSAGE"));
            Assert.True(logContents.Contains("ERROR MESSAGE"));
            Assert.True(logContents.Contains("WARN MESSAGE"));
            Assert.True(logContents.Contains("FATAL MESSAGE"));

            File.Delete(tmpFile);
        }
    }
}