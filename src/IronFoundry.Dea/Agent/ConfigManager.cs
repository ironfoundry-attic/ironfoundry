namespace IronFoundry.Dea.Agent
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using IronFoundry.Dea.Config;
    using IronFoundry.Dea.Types;
    using Newtonsoft.Json;

    public class ConfigManager : IConfigManager
    {
        private readonly IConfig config;

        public ConfigManager(IConfig config)
        {
            this.config = config;
        }

        public void BindServices(Droplet droplet, Instance instance)
        {
            string appPath = instance.Staged;

            if (false == droplet.Services.IsNullOrEmpty())
            {
                Configuration c = getConfiguration(instance);
                if (null != c)
                {
                    ConnectionStringsSection connectionStringsSection = c.GetSection("connectionStrings") as ConnectionStringsSection;
                    if (null != connectionStringsSection)
                    {
                        foreach (Service svc in droplet.Services.Where(s => s.IsMSSqlServer))
                        {
                            if (null != svc.Credentials)
                            {
                                SqlConnectionStringBuilder builder;
                                ConnectionStringSettings defaultConnectionStringSettings = connectionStringsSection.ConnectionStrings["Default"];
                                if (null == defaultConnectionStringSettings)
                                {
                                    builder = new SqlConnectionStringBuilder();
                                }
                                else
                                {
                                    builder = new SqlConnectionStringBuilder(defaultConnectionStringSettings.ConnectionString);
                                }

                                builder.DataSource = svc.Credentials.Host;
                                builder.ConnectTimeout = 30;

                                if (svc.Credentials.Password.IsNullOrWhiteSpace() || svc.Credentials.Username.IsNullOrWhiteSpace())
                                {
                                    builder.IntegratedSecurity = true;
                                }
                                else
                                {
                                    builder.IntegratedSecurity = false;
                                    builder.UserID = svc.Credentials.Username;
                                    builder.Password = svc.Credentials.Password;
                                }

                                if (false == svc.Credentials.Name.IsNullOrWhiteSpace())
                                {
                                    builder.InitialCatalog = svc.Credentials.Name;
                                }

                                if (null == defaultConnectionStringSettings)
                                {
                                    connectionStringsSection.ConnectionStrings.Add(new ConnectionStringSettings("Default", builder.ConnectionString));
                                }
                                else
                                {
                                    defaultConnectionStringSettings.ConnectionString = builder.ConnectionString;
                                }
                                break;
                            }
                        }
                    }
                    c.Save();
                }
            }
        }

        public void SetupEnvironment(Droplet droplet, Instance instance)
        {
            Configuration c = getConfiguration(instance);
            if (null != c)
            {
                AppSettingsSection appSettingsSection = c.GetSection("appSettings") as AppSettingsSection;
                if (null != appSettingsSection)
                {
                    var appSettings = appSettingsSection.Settings;
                    replaceSetting(appSettings, "HOME", Path.Combine(config.AppDir, instance.Staged));

                    var applicationDict = new Dictionary<string, object>
                    {
                        { "instance_id", droplet.ID },
                        { "instance_index", droplet.InstanceIndex },
                        { "name", droplet.Name },
                        { "uris", droplet.Uris },
                        { "users", droplet.Users },
                        { "version", droplet.Version },
                        { "runtime", droplet.Runtime },
                    };
                    string vcapApplicationJson = JsonConvert.SerializeObject(applicationDict);
                    replaceSetting(appSettings, "VCAP_APPLICATION", vcapApplicationJson);

                    if (false == droplet.Services.IsNullOrEmpty())
                    {
                        string vcapServicesJson = JsonConvert.SerializeObject(droplet.Services.ToDictionary(s => s.Label));
                        replaceSetting(appSettings, "VCAP_SERVICES", vcapServicesJson);
                    }

                    replaceSetting(appSettings, "VCAP_APP_HOST", config.LocalIPAddress.ToString());
                    // TODO appSettingsSection.Settings["VCAP_APP_PORT"].Value

                    if (false == droplet.Env.IsNullOrEmpty())
                    {
                        foreach (string env in droplet.Env)
                        {
                            string[] envSplit = env.Split(new[] { '=' });
                            if (false == envSplit.IsNullOrEmpty() && envSplit.Length == 2)
                            {
                                string key = envSplit[0];
                                string value = envSplit[1];
                                replaceSetting(appSettings, key, value);
                            }
                        }
                    }
                }
                c.Save();
            }
        }

        private Configuration getConfiguration(Instance instance)
        {
            var webConfigPath = Path.Combine(config.AppDir, instance.Staged, "app", "Web.config");
            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = webConfigPath };
            return ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        }

        private static void replaceSetting(KeyValueConfigurationCollection appSettings, string key, string value)
        {
            appSettings.Remove(key);
            appSettings.Add(key, value);
        }
    }
}