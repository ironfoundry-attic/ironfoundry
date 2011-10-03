using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using CloudFoundry.Net.Types;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Model
{
    public class PreferencesProvider
    {
        private string preferencesPath;
        private const string preferencesFileName = "preferences.bin";
        private Preferences preferences;

        public PreferencesProvider(string preferencesPath)
        {
            this.preferences = new Preferences()
            {
                Clouds = new ObservableCollection<Cloud>(),
                CloudUrls = CloudUrl.GetDefaultCloudUrls()
            };
            this.preferencesPath = preferencesPath;

            Messenger.Default.Register<NotificationMessage<ObservableCollection<CloudUrl>>>(this, SaveCloudUrls);
            Messenger.Default.Register<NotificationMessage<ObservableCollection<Cloud>>>(this, SaveClouds);
            Messenger.Default.Register<NotificationMessageAction<Preferences>>(this, LoadPreferences);
        }

        private void SaveClouds(NotificationMessage<ObservableCollection<Cloud>> message)
        {
            if (message.Notification.Equals(Messages.SaveClouds))
            {
                this.preferences.Clouds = message.Content;
                SavePreferences();
            }
        }

        private void SaveCloudUrls(NotificationMessage<ObservableCollection<CloudUrl>> message)
        {
            if (message.Notification.Equals(Messages.SaveCloudUrls))
            {
                this.preferences.CloudUrls = message.Content;
                SavePreferences();
            }
        }

        private void LoadPreferences(NotificationMessageAction<Preferences> message)
        {
            if (message.Notification.Equals(Messages.LoadPreferences))
            {
                var preferences = LoadPreferences();
                message.Execute(preferences);
            }
        }

        public Preferences LoadPreferences()
        {
            try
            {
                var fullPath = this.preferencesPath + "/" + preferencesFileName;
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);                
                if (isoStore.DirectoryExists(this.preferencesPath) && isoStore.FileExists(fullPath))
                {
                    using (IsolatedStorageFileStream configStream = isoStore.OpenFile(fullPath, FileMode.Open))
                    {
                        var binary = new BinaryFormatter();
                        preferences = binary.Deserialize(configStream) as Preferences;
                    }
                }
            }
            catch (Exception)
            {
                // If preferences fail to load, swallow the exception.
            }
            return preferences;
        }        

        public void SavePreferences()
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            IsolatedStorageFileStream configStream;
            
            if (!isoStore.DirectoryExists(this.preferencesPath))
                isoStore.CreateDirectory(this.preferencesPath);

            var fullPath = this.preferencesPath + "/" + preferencesFileName;
            if (!isoStore.FileExists(fullPath))
                configStream = isoStore.CreateFile(fullPath);
            else
                configStream = isoStore.OpenFile(fullPath, FileMode.Open);
            var binary = new BinaryFormatter();                      
            binary.Serialize(configStream, this.preferences);            
            configStream.Flush();
            configStream.Close();
        }
    }
}
