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
                        Url = "http://api.cloudfoundry.com"                       
                    }
                }, Messages.InitializeClouds);
            Messenger.Default.Send(message);
        }
    }
}
