namespace IronFoundry.Warden.Service
{
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.ServiceProcess;

    /// <summary>
    /// NB: this class is probably not necessary.
    /// http://perezgb.com/2011/10/19/installing-a-topshelf-service-using-wix
    /// </summary>
    [RunInstaller(true)]
    public class WinServiceInstaller : Installer
    {
        private ServiceInstaller _serviceInstaller;
        private ServiceProcessInstaller _serviceProcessInstaller;
 
        public WinServiceInstaller()
        {
            this.InitializeComponent();
        }
 
        private void InitializeComponent()
        {
            this._serviceInstaller = new ServiceInstaller();
            _serviceProcessInstaller = new ServiceProcessInstaller();

            this._serviceProcessInstaller.Account = ServiceAccount.User;

            this._serviceInstaller.DisplayName = Constants.DisplayName;
            this._serviceInstaller.ServiceName = Constants.ServiceName;

            this.Installers.AddRange(new Installer[] { this._serviceProcessInstaller, this._serviceInstaller });
        }
    }
}
