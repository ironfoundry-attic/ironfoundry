using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using CloudFoundry.Net.Types;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class CloudTreeViewItemViewModel : TreeViewItemViewModel
    {
        private Cloud cloud;
        public RelayCommand<MouseButtonEventArgs> OpenCloudCommand { get; private set; }

        public CloudTreeViewItemViewModel(Cloud cloud) : base(null,false)
        {
            OpenCloudCommand = new RelayCommand<MouseButtonEventArgs>(OpenCloud);

            this.cloud = cloud;
            foreach (Application app in cloud.Applications)
                base.Children.Add(new ApplicationTreeViewItemViewModel(app, this));
        }

        public string ServerName
        {
            get { return this.cloud.ServerName; }
        }

        private void OpenCloud(MouseButtonEventArgs e)
        {
            if (e == null || e.ClickCount >= 2)
                Messenger.Default.Send(new NotificationMessage<Cloud>(this, this.cloud, Messages.OpenCloud));
        }
    }
}
