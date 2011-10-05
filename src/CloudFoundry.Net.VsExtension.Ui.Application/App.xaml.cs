namespace CloudFoundry.Net.VsExtension.Ui.Application
{
    using Controls.Model;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private PreferencesProvider preferencesProvider;
        private CloudFoundryProvider cloudFoundryProvider;

        public App()
        {            
            preferencesProvider = new PreferencesProvider("CloudFoundryExplorerApp");
            cloudFoundryProvider = new CloudFoundryProvider(preferencesProvider, new Vmc.VcapClient(), new Vmc.VcapCredentialManager());
        }        
    }
}