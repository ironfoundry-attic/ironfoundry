namespace IronFoundry.Ui.Controls.Model
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Runtime.Serialization.Formatters.Binary;
    using IronFoundry.Types;

    public class PreferencesProvider : IPreferencesProvider
    {
        private readonly string preferencesPath;
        private const string PreferencesFileName = "preferences.bin";

        public PreferencesProvider(string preferencesPath)
        {
            this.preferencesPath = preferencesPath;            
        }
        
        public PreferencesV2 Load()
        {
            var preferences = new PreferencesV2();

            try
            {
                var fullPath = this.preferencesPath + "/" + PreferencesFileName;
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);                
                if (isoStore.DirectoryExists(this.preferencesPath) && isoStore.FileExists(fullPath))
                {
                    using (IsolatedStorageFileStream configStream = isoStore.OpenFile(fullPath, FileMode.Open))
                    {
                        var formatter = new BinaryFormatter();
                        object tmp = formatter.Deserialize(configStream); // as PreferencesV2;
                        PreferencesV2 v2prefs = tmp as PreferencesV2;
                        if (null != v2prefs)
                        {
                            preferences = v2prefs;
                        }
                        else
                        {
                            Preferences v1prefs = tmp as Preferences;
                            if (null != v1prefs)
                            {
                                preferences = convertPreferences(v1prefs);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If preferences fail to load, swallow the exception.
            }

            return preferences;
        }

        private PreferencesV2 convertPreferences(Preferences v1prefs)
        {
            return new PreferencesV2
            {
                Clouds = v1prefs.Clouds.ToArrayOrNull()
            };
        }        

        public void Save(PreferencesV2 preferences)
        {           
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            IsolatedStorageFileStream configStream;
            
            if (!isoStore.DirectoryExists(this.preferencesPath))
                isoStore.CreateDirectory(this.preferencesPath);

            var fullPath = this.preferencesPath + "/" + PreferencesFileName;
            if (!isoStore.FileExists(fullPath))
                configStream = isoStore.CreateFile(fullPath);
            else
                configStream = isoStore.OpenFile(fullPath, FileMode.Open);
            var formatter = new BinaryFormatter();            
            formatter.Serialize(configStream, preferences);            
            configStream.Flush();
            configStream.Close();
        }
    }
}
