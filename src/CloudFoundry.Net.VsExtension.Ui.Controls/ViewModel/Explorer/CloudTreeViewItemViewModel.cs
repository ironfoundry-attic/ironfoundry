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
using System.Collections.Specialized;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class CloudTreeViewItemViewModel : TreeViewItemViewModel
    {
        private Cloud cloud;
        public RelayCommand<MouseButtonEventArgs> OpenCloudCommand { get; private set; }

        public CloudTreeViewItemViewModel(Cloud cloud) : base(null,false)
        {
            OpenCloudCommand = new RelayCommand<MouseButtonEventArgs>(OpenCloud);
            
            this.Cloud = cloud;
            this.Cloud.Applications.CollectionChanged += new NotifyCollectionChangedEventHandler(Applications_CollectionChanged);       
        }

        private void Applications_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.Children.Clear();
            foreach (Application app in this.cloud.Applications)
                base.Children.Add(new ApplicationTreeViewItemViewModel(app, this));                
        }

        public Cloud Cloud
        {
            get { return this.cloud; }
            set { this.cloud = value; RaisePropertyChanged("Cloud"); }
        }

        private void OpenCloud(MouseButtonEventArgs e)
        {
            if (e == null || e.ClickCount >= 2)
                Messenger.Default.Send(new NotificationMessage<Cloud>(this, this.cloud, Messages.OpenCloud));
        }
    }
}
