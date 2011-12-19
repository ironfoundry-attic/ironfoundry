namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Windows.Input;
    using CloudFoundry.Net.Types;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;

    public class CloudTreeViewItemViewModel : TreeViewItemViewModel
    {
        private Cloud cloud;
        private ICloudFoundryProvider provider;
        public RelayCommand<MouseButtonEventArgs> OpenCloudCommand { get; private set; }
        public RelayCommand RemoveCloudCommand { get; private set; }
        public RelayCommand ConnectCommand { get; private set; }
        public RelayCommand DisconnectCommand { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }
        private BackgroundWorker connector = new BackgroundWorker();

        public CloudTreeViewItemViewModel(Cloud cloud)
            : base(null, false)
        {
            Messenger.Default.Send<NotificationMessageAction<ICloudFoundryProvider>>(new NotificationMessageAction<ICloudFoundryProvider>(Messages.GetCloudFoundryProvider, p => this.provider = p));
            OpenCloudCommand = new RelayCommand<MouseButtonEventArgs>(OpenCloud);
            RemoveCloudCommand = new RelayCommand(RemoveCloud);
            ConnectCommand = new RelayCommand(Connect, CanExecuteConnect);
            DisconnectCommand = new RelayCommand(Disconnect, CanExecuteDisconnect);
            RefreshCommand = new RelayCommand(Refresh);

            this.Cloud = cloud;
            if (Cloud.IsConnected)
                foreach (var application in Cloud.Applications)
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
                this.Cloud.Merge(result.Response);
            else
                Messenger.Default.Send(new NotificationMessage<string>(result.Message, Messages.ErrorMessage));
        }

        private void Refresh()
        {
            var worker = new BackgroundWorker();
            worker.DoWork += BeginConnect;
            worker.RunWorkerCompleted += EndConnect;
            worker.RunWorkerAsync();
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
            var comparer = new ApplicationEqualityComparer();
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
            else if (e.Action.Equals(NotifyCollectionChangedAction.Replace))
            {
                foreach(var item in e.NewItems)
                {
                    var app = item as Application;
                    foreach (var treeView in base.Children)
                    {
                        var appTreeView = treeView as ApplicationTreeViewItemViewModel;
                        if (comparer.Equals(appTreeView.Application, app))
                        {
                            appTreeView.Application = app;
                            if (!appTreeView.HasNotBeenPopulated)
                                appTreeView.LoadChildren();
                        }
                    }
                }
            }
            else if (e.Action.Equals(NotifyCollectionChangedAction.Reset))
                base.Children.Clear();
        }      
    }
}