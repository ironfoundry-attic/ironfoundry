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
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class CloudTreeViewItemViewModel : TreeViewItemViewModel
    {
        private Cloud cloud;
        private CloudFoundryProvider provider;
        public RelayCommand<MouseButtonEventArgs> OpenCloudCommand { get; private set; }
        public RelayCommand RemoveCloudCommand { get; private set; }
        public RelayCommand ConnectCommand { get; private set; }
        public RelayCommand DisconnectCommand { get; private set; }
        private BackgroundWorker connector = new BackgroundWorker();

        public CloudTreeViewItemViewModel(Cloud cloud) : base(null,false)
        {
            Messenger.Default.Send<NotificationMessageAction<CloudFoundryProvider>>(new NotificationMessageAction<CloudFoundryProvider>(Messages.GetCloudFoundryProvider, p => this.provider = p));
            OpenCloudCommand = new RelayCommand<MouseButtonEventArgs>(OpenCloud);
            RemoveCloudCommand = new RelayCommand(RemoveCloud);
            ConnectCommand = new RelayCommand(Connect, CanExecuteConnect);
            DisconnectCommand = new RelayCommand(Disconnect, CanExecuteDisconnect);
            
            this.Cloud = cloud;
            if (Cloud.IsConnected)
                foreach(var application in Cloud.Applications)
                    Children.Add(new ApplicationTreeViewItemViewModel(application, this));

            this.Cloud.Applications.CollectionChanged += Applications_CollectionChanged;
            connector.DoWork += BeginConnect;
            connector.RunWorkerCompleted += EndConnect;
            connector.RunWorkerAsync(); 
        }

        public Cloud Cloud
        {
            get { return this.cloud; }
            set { this.cloud = value; RaisePropertyChanged("Cloud"); }
        }
       
        private void OpenCloud(MouseButtonEventArgs e)
        {
            if (e == null || e.ClickCount >= 2)
                Messenger.Default.Send(new NotificationMessage<Cloud>(this, this.Cloud, Messages.OpenCloud));
        }

        private void BeginConnect(object sender, DoWorkEventArgs args)
        {
            args.Result = provider.Connect(this.Cloud);
        }

        private void EndConnect(object sender, RunWorkerCompletedEventArgs args)
        {
            var result = args.Result as ProviderResponse<Cloud>;
            if (result.Response != null)
            {
                this.Cloud.AccessToken = result.Response.AccessToken;
                this.Cloud.Applications.Synchronize(result.Response.Applications, new ApplicationEqualityComparer());
                this.Cloud.AvailableServices.Synchronize(result.Response.AvailableServices, new SystemServiceEqualityComparer());
                this.Cloud.Services.Synchronize(result.Response.Services, new ProvisionedServiceEqualityComparer());
            }
            else
            {
                // Set error info here.
            }
        }

        private void RemoveCloud()
        {
            provider.Clouds.Remove(cloud);
        }

        private void Connect()
        {
            connector.RunWorkerAsync(); 
        }

        private bool CanExecuteConnect()
        {
            return Cloud.IsDisconnected;
        }

        private void Disconnect()
        {
            provider.Disconnect(cloud);
        }

        private bool CanExecuteDisconnect()
        {
            return Cloud.IsConnected;
        }

        private void Applications_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action.Equals(NotifyCollectionChangedAction.Add))
            {
                foreach (var item in e.NewItems)
                {
                    var app = item as Application;
                    base.Children.Add(new ApplicationTreeViewItemViewModel(app, this));
                }
            }
            else if (e.Action.Equals(NotifyCollectionChangedAction.Remove))
            {
                var comparer = new ApplicationEqualityComparer();
                foreach (var item in e.OldItems)
                {
                    var appsToRemove = new List<TreeViewItemViewModel>();
                    var app = item as Application;
                    foreach (var treeView in base.Children)
                    {
                        var appTreeView = treeView as ApplicationTreeViewItemViewModel;
                        if (comparer.Equals(appTreeView.Application, app))
                            appsToRemove.Add(treeView);
                    }
                    foreach (var treeView in appsToRemove)
                        base.Children.Remove(treeView);
                }
            }
            else if (e.Action.Equals(NotifyCollectionChangedAction.Reset))
                base.Children.Clear();
        }
    }
}
