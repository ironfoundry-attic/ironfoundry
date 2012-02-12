namespace IronFoundry.Ui.Controls.ViewModel.Explorer
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Windows.Input;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using Model;
    using Mvvm;
    using Types;
    using Utilities;

    public class CloudTreeViewItemViewModel : TreeViewItemViewModel
    {
        private readonly BackgroundWorker connector = new BackgroundWorker();
        private Cloud cloud;
        private ICloudFoundryProvider provider;

        public CloudTreeViewItemViewModel(Cloud cloud)
            : base(null, false)
        {
            Messenger.Default.Send(new NotificationMessageAction<ICloudFoundryProvider>(
                Messages.GetCloudFoundryProvider,
                p =>
                {
                    provider = p;
                    provider.CloudChanged += provider_CloudChanged;
                }));

            OpenCloudCommand   = new RelayCommand<MouseButtonEventArgs>(OpenCloud);
            RemoveCloudCommand = new RelayCommand(RemoveCloud);
            ConnectCommand     = new RelayCommand(Connect, CanExecuteConnect);
            DisconnectCommand  = new RelayCommand(Disconnect, CanExecuteDisconnect);
            RefreshCommand     = new RelayCommand(Refresh);

            Cloud = cloud;
            if (Cloud.IsConnected)
            {
                foreach (Application application in Cloud.Applications)
                {
                    Children.Add(new ApplicationTreeViewItemViewModel(application, this));
                }
            }

            Cloud.Applications.CollectionChanged += Applications_CollectionChanged;
            connector.DoWork += BeginConnect;
            connector.RunWorkerCompleted += EndConnect;
            lock (connector)
            {
                if (false == connector.IsBusy)
                {
                    connector.RunWorkerAsync();
                }
            }
        }

        public RelayCommand<MouseButtonEventArgs> OpenCloudCommand { get; private set; }
        public RelayCommand RemoveCloudCommand { get; private set; }
        public RelayCommand ConnectCommand { get; private set; }
        public RelayCommand DisconnectCommand { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }

        public Cloud Cloud
        {
            get { return cloud; }
            set
            {
                cloud = value;
                RaisePropertyChanged("Cloud");
            }
        }

        private void provider_CloudChanged(object sender, CloudEventArgs e)
        {
            Refresh(e.Cloud);
        }

        private void OpenCloud(MouseButtonEventArgs e)
        {
            if (e == null || e.ClickCount >= 2)
                Messenger.Default.Send(new NotificationMessage<Cloud>(this, Cloud, Messages.OpenCloud));
        }

        private void BeginConnect(object sender, DoWorkEventArgs args)
        {
            args.Result = provider.Connect(Cloud);
        }

        private void EndConnect(object sender, RunWorkerCompletedEventArgs args)
        {
            var result = args.Result as ProviderResponse<Cloud>;
            if (result.Response != null)
            {
                Cloud.Merge(result.Response);
            }
            else
            {
                Messenger.Default.Send(new NotificationMessage<string>(result.Message, Messages.ErrorMessage));
            }
        }

        private void Refresh(Cloud cloud)
        {
            lock (connector)
            {
                if (false == connector.IsBusy)
                {
                    connector.RunWorkerAsync(cloud);
                }
            }
        }

        private void Refresh()
        {
            Refresh(Cloud);
        }

        private void RemoveCloud()
        {
            provider.RemoveCloud(cloud);
        }

        private void Connect()
        {
            lock (connector)
            {
                if (false == connector.IsBusy)
                {
                    connector.RunWorkerAsync();
                }
            }
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
                foreach (object item in e.NewItems)
                {
                    var app = item as Application;
                    base.Children.Add(new ApplicationTreeViewItemViewModel(app, this));
                }
            }
            else if (e.Action.Equals(NotifyCollectionChangedAction.Remove))
            {
                foreach (object item in e.OldItems)
                {
                    var appsToRemove = new List<TreeViewItemViewModel>();
                    var app = item as Application;
                    foreach (TreeViewItemViewModel treeView in base.Children)
                    {
                        var appTreeView = treeView as ApplicationTreeViewItemViewModel;
                        if (comparer.Equals(appTreeView.Application, app))
                            appsToRemove.Add(treeView);
                    }
                    foreach (TreeViewItemViewModel treeView in appsToRemove)
                        base.Children.Remove(treeView);
                }
            }
            else if (e.Action.Equals(NotifyCollectionChangedAction.Replace))
            {
                foreach (object item in e.NewItems)
                {
                    var app = item as Application;
                    foreach (TreeViewItemViewModel treeView in base.Children)
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
