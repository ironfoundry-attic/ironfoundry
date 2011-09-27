using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.ObjectModel;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using CloudFoundry.Net.Types;

namespace CloudFoundry.Net.VsExtension.Ui.Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var message = new NotificationMessage<ObservableCollection<Cloud>>(
                new ObservableCollection<Cloud>()
                {
                    new Cloud() {
                        ServerName = "VMware Cloud Foundry",
                        Email = "caledh@gmail.com",
                        Password = "password",
                        HostName = "localhost",
                        Url = "http://api.cloudfoundry.com", 
                        AccessToken = "04085b0849221563616c65646840676d61696c2e636f6d063a0645546c2b07a64c8b4e2219d48bc74501e8837a187aca53d371998a758b74d0"
                    }
                }, Messages.InitializeClouds);
            Messenger.Default.Send(message);
        }
    }
}
