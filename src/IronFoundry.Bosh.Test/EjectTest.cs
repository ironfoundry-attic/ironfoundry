namespace IronFoundry.Bosh.Test
{
    using System.IO;
    using System.Linq;
    using IronFoundry.Bosh.Agent;
    using Xunit;

    public class EjectTest
    {
        [Fact]
        public void Can_Eject_CD()
        {
            bool settingsFound = false;
            DirectoryInfo driveRootDirectory = null;
            string settingsJsonStr = null;

            for (int i = 0; i < 5 && false == settingsFound; ++i)
            {
                DriveInfo[] drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom && d.IsReady).ToArray();
                if (null != drives)
                {
                    foreach (var drive in drives)
                    {
                        driveRootDirectory = drive.RootDirectory;
                        string envPath = Path.Combine(driveRootDirectory.FullName, "env");
                        if (File.Exists(envPath))
                        {
                            settingsJsonStr = File.ReadAllText(envPath);
                            settingsFound = true;
                            break;
                        }
                    }
                }
            }
            EjectMedia.Eject(driveRootDirectory.FullName);
        }
    }
}