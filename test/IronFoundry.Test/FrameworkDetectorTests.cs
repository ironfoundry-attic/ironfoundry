namespace IronFoundry.Test
{
    using System;
    using System.IO;
    using IronFoundry.Vcap;
    using Xunit;

    public class FrameworkDetectorTests
    {
        [Fact]
        public void AspDotNet()
        {
            using (var appPath = new PathWithFileNamed("Web.config"))
            {
                DetetectedFramework df = FrameworkDetetctor.Detect(appPath.DirectoryPath);
                Assert.Equal("aspdotnet", df.Framework);
                Assert.Equal("aspdotnet40", df.Runtime);
            }
        }

        [Fact]
        public void Rack()
        {
            using (var appPath = new PathWithFileNamed("config.ru"))
            {
                DetetectedFramework df = FrameworkDetetctor.Detect(appPath.DirectoryPath);
                Assert.Equal("rack", df.Framework);
                Assert.Equal("ruby19", df.Runtime);
            }
        }

        [Fact]
        public void Rails()
        {
            using (var appPath = new PathWithFileNamed("environment.rb", "config"))
            {
                DetetectedFramework df = FrameworkDetetctor.Detect(appPath.DirectoryPath);
                Assert.Equal("rails3", df.Framework);
                Assert.Equal("ruby19", df.Runtime);
            }
        }

        [Fact]
        public void Grails()
        {
            using (var appPath = new PathWithFileNamed("web.xml", "WEB-INF"))
            {
                DirectoryInfo appDir = appPath.DirectoryPath;

                string webInfLibDir = Path.Combine(appDir.FullName, "WEB-INF", "lib");

                Directory.CreateDirectory(webInfLibDir);

                File.WriteAllText(Path.Combine(webInfLibDir, "grails-web.foofoo1234.jar"), "CONTENTS");

                DetetectedFramework df = FrameworkDetetctor.Detect(appPath.DirectoryPath);

                Assert.Equal("grails", df.Framework);
                Assert.Equal("java", df.Runtime);
            }
        }

        [Fact]
        public void WarFile()
        {
            // testapp.war
            string currentDir = Environment.CurrentDirectory;
            Assert.True(File.Exists(Path.Combine(currentDir, "testapp.war")));
            DetetectedFramework df = FrameworkDetetctor.Detect(new DirectoryInfo(currentDir));
            Assert.Equal("java_web", df.Framework);
            Assert.Equal("java", df.Runtime);
        }

        private class PathWithFileNamed : IDisposable
        {
            private readonly DirectoryInfo path;

            public PathWithFileNamed(string fileName, string subDirectoryName = null, string fileContents = "TEST CONTENTS")
            {
                path = Utility.GetTempDirectory();
                DirectoryInfo fileDirectory = path;
                if (null != subDirectoryName)
                {
                    fileDirectory = Directory.CreateDirectory(Path.Combine(path.FullName, subDirectoryName));
                }
                string filePath = System.IO.Path.Combine(fileDirectory.FullName, fileName);
                // Console.WriteLine("FILE: {0}", filePath);
                File.WriteAllText(filePath, fileContents);
            }

            public DirectoryInfo DirectoryPath
            {
                get { return path; }
            }

            public void Dispose()
            {
                Directory.Delete(path.FullName, true);
            }
        }
    }
}