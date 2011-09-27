using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using CloudFoundry.Net.Types;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Utilities
{
    public static class SampleData
    {
        private static Random rnd = new Random();

        public static ObservableCollection<Cloud> Create()
        {
            return null;
            //return new ObservableCollection<Cloud>(GetRandomObjects<Cloud, object>(null, GetSampleCloud));
        }

        #region Sample Data For DELETE

        private static ObservableCollection<CloudUrl> GetBaseCloudUrls()
        {
            ObservableCollection<CloudUrl> cloudUrls = new ObservableCollection<CloudUrl>() {
                new CloudUrl() { ServerType = "Local cloud", Url = "http://api.vcap.me", IsConfigurable = false},
                new CloudUrl() { ServerType = "Microcloud", Url = "http://api.{mycloud}.cloudfoundry.me", IsConfigurable = true, IsMicroCloud = true },
                new CloudUrl() { ServerType = "VMware Cloud Foundry", Url = "https://api.cloudfoundry.com", IsDefault = true },
                new CloudUrl() { ServerType = "vmforce", Url = "http://api.alpha.vmforce.com", IsConfigurable = false}
            };
            return cloudUrls;
        }

        //private static Cloud GetSampleCloud(int i, object parent)
        //{
        //    var hostname = string.Format("api.cloud-{0}.org", GetRandomString(5));
        //    var cloud = new Cloud()
        //    {
        //        ServerName = "Cloud " + rnd.Next(10000),
        //        Connected = false,
        //        Email = string.Format("{0}@{1}.com", GetRandomString(15), GetRandomString(10)),
        //        HostName = hostname,
        //        Url = string.Format("http://{0}", hostname),
        //        Password = GetRandomString(10),
        //        TimeoutStart = rnd.Next(9999),
        //        TimeoutStop = rnd.Next(9999),
        //    };

        //    cloud.Applications = GetRandomObjects<Application, Cloud>(cloud, GetSampleApplication);
        //    cloud.Services = GetRandomObjects<Service, Cloud>(cloud, GetSampleService);

        //    return cloud;
        //}

        //private static Application GetSampleApplication(int i, Cloud parent)
        //{
        //    var app = new Application()
        //    {
        //        Name = "App " + rnd.Next(10000),
        //        Cpus = rnd.Next(4),
        //        State = CloudFoundry.Net.Types.Instance.InstanceState.STOPPED,
        //        MappedUrls = GetRandomUrls(),
        //        MemoryLimit = CloudFoundry.Net.VsExtension.Ui.Controls.Model.Constants.MemoryLimits[rnd.Next(0, 5)],
        //        Parent = parent
        //    };

        //    app.Instances = GetRandomObjects<Instance, Application>(app, GetSampleInstance);
        //    app.InstanceCount = app.Instances.Count;
        //    app.Services = GetRandomObjects<Service, Cloud>(parent, GetSampleService);

        //    return app;
        //}

        //private static ObservableCollection<string> GetRandomUrls()
        //{
        //    string[] com = { "com", "org", "ca", "net", "uk" };
        //    ObservableCollection<string> retString = new ObservableCollection<string>();
        //    for (int i = 2; i < rnd.Next(4, 7); i++)
        //        retString.Add(GetRandomString(12) + "." + com[rnd.Next(4)]);
        //    return retString;
        //}

        //private static Service GetSampleService(int i, Cloud parent)
        //{
        //    string[] serviceTypes = { "Database", "Web Service", "Service Type 1", "Service Type 2" };
        //    string[] vendors = { "MySql", "Postgres", "Sql Server", "Sybase" };
        //    string[] versions = { "4.0", "5.1", "10.0", "15.3.1" };
        //    var service = new Service()
        //    {
        //        Name = "Service " + rnd.Next(10000),
        //        ServiceType = serviceTypes[rnd.Next(0, 3)],
        //        Vendor = vendors[rnd.Next(0, 3)],
        //        Version = versions[rnd.Next(0, 3)],
        //        Parent = parent
        //    };
        //    return service;
        //}

        //private static Instance GetSampleInstance(int i, Application parent)
        //{
        //    var instance = new Instance()
        //    {
        //        CpuPercent = Convert.ToDecimal(rnd.NextDouble() * 100.00),
        //        Disk = rnd.Next(0, 2048),
        //        Host = GetRandomIP(),
        //        ID = i,
        //        Memory = CloudFoundry.Net.VsExtension.Ui.Controls.Model.Constants.MemoryLimits[rnd.Next(0, 5)],
        //        Parent = parent,
        //        Uptime = DateTime.Now - DateTime.Now.Subtract(new TimeSpan((long)rnd.Next(200000)))
        //    };
        //    return instance;
        //}


        //private static string GetRandomIP()
        //{
        //    return string.Format("{0}.{1}.{2}.{3}", 192, 168, rnd.Next(255), rnd.Next(255));
        //}

        //private static ObservableCollection<T> GetRandomObjects<T, U>(U parent, Func<int, U, T> objectCreate)
        //{
        //    var list = new ObservableCollection<T>();
        //    int count = Convert.ToInt32(rnd.Next(2, 10));
        //    for (int i = 0; i <= count; i++)
        //        list.Add(objectCreate(i, parent));
        //    return list;
        //}

        //private static string GetRandomString(int size)
        //{
        //    StringBuilder builder = new StringBuilder();
        //    char ch;
        //    for (int i = 0; i < size; i++)
        //    {
        //        ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * rnd.NextDouble() + 65)));
        //        builder.Append(ch);
        //    }

        //    return builder.ToString().ToLower();
        //}

        #endregion
    }
}
