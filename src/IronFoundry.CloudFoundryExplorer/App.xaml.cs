namespace IronFoundry.CloudFoundryExplorer
{
    using IronFoundry.Ui.Controls.Model;

    public partial class App : System.Windows.Application
    {
        public App()
        {            
            var preferencesProvider = new PreferencesProvider("CloudFoundryExplorerApp");
            var cloudFoundryProvider = new CloudFoundryProvider(preferencesProvider);
        }        
    }
}