namespace CloudFoundry.Net.Dea.Service
{
    partial class DeaWindowsServiceInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.deaWindowsServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.deaWindowsServiceServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // deaWindowsServiceProcessInstaller
            // 
            this.deaWindowsServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            this.deaWindowsServiceProcessInstaller.Password = null;
            this.deaWindowsServiceProcessInstaller.Username = null;
            // 
            // deaWindowsServiceServiceInstaller
            // 
            this.deaWindowsServiceServiceInstaller.Description = "Droplet Execution Agent for Deployment of ASP.NET applications through Cloud Foun" +
    "dry.";
            this.deaWindowsServiceServiceInstaller.DisplayName = "Cloud Foundry Dea Windows Service";
            this.deaWindowsServiceServiceInstaller.ServiceName = "DeaWindowsService";
            // 
            // DeaWindowsServiceInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.deaWindowsServiceProcessInstaller,
            this.deaWindowsServiceServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller deaWindowsServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller deaWindowsServiceServiceInstaller;
    }
}