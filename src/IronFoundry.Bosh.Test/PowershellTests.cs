namespace IronFoundry.Bosh.Test
{
    using System;
    using System.IO;
    using IronFoundry.Misc.Utilities;
    using Xunit;

    public class PowershellTests
    {
        [Fact]
        public void Test_Running_Powershell_Script()
        {
            string scriptContents = @"Write-Host 'ONE'
Write-Host 'TWO'
Write-Host 'THREE'
Write-Host ""Install Target: $env:BoshInstallTarget""";
            string script = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".ps1");
            File.WriteAllText(script, scriptContents);
            string exe = @"C:\Windows\System32\WindowsPowerShell\V1.0\powershell.exe";
            string args = @"-NoLogo -NonInteractive -WindowStyle Hidden -ExecutionPolicy Unrestricted -File " + script;
            // string exe = @"C:\windows\system32\inetsrv\appcmd.exe";
            // string args = @"/?";
            var p = new RedirectedProcess(exe, args);
            p.AddEnvironmentVariable("BoshInstallTarget", Path.GetTempPath());
            p.StartAndWait();
            Console.WriteLine("STDOUT:");
            Console.Write(p.STDOUT);
            Console.WriteLine("STDERR:");
            Console.Write(p.STDERR);
        }

        [Fact]
        public void Test_Running_Powershell_Script_Two()
        {
            string scriptContents = @"Write-Host 'ONE'
Write-Host 'TWO'
Write-Host 'THREE'
Write-Host ""Install Target: $env:BoshInstallTarget""";
            string script = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".ps1");
            File.WriteAllText(script, scriptContents);
            var p = new PowershellExecutor(script);
            p.AddEnvironmentVariable("BoshInstallTarget", Path.GetTempPath());
            p.StartAndWait();
            Console.WriteLine("STDOUT:");
            Console.Write(p.STDOUT);
            Console.WriteLine("STDERR:");
            Console.Write(p.STDERR);
        }
    }
}