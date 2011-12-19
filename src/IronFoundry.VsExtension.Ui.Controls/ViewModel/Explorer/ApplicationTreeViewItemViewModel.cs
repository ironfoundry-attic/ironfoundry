namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Windows.Input;
    using System.Windows.Threading;
    using CloudFoundry.Net.Types;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;

    public class ApplicationTreeViewItemViewModel : TreeViewItemViewModel
    {
        private ICloudFoundryProvider provider;
        private Application application;
        public RelayCommand<MouseButtonEventArgs> OpenApplicationCommand { get; private set; }
        public RelayCommand StartApplicationCommand { get; private set; }
        public RelayCommand StopApplicationCommand { get; private set; }
        public RelayCommand RestartApplicationCommand { get; private set; }
        public RelayCommand DeleteApplicationCommand { get; private set; }
        public Dispatcher dispatcher;
        
        public ApplicationTreeViewItemViewModel(Application application, CloudTreeViewItemViewModel parentCloud) : base(parentCloud, true)
        {            
            Messenger.Default.Send<NotificationMessageAction<ICloudFoundryProvider>>(new NotificationMessageAction<ICloudFoundryProvider>(Messages.GetCloudFoundryProvider, p => this.provider = p));
            OpenApplicationCommand = new RelayCommand<MouseButtonEventArgs>(OpenApplication);
            StartApplicationCommand = new RelayCommand(StartApplication, CanStart);
            StopApplicationCommand = new RelayCommand(StopApplication, CanStop);
            RestartApplicationCommand = new RelayCommand(RestartApplication, CanStop);
            DeleteApplicationCommand = new RelayCommand(DeleteApplication);

            this.Application = application;
            this.Application.InstanceCollection.CollectionChanged += InstanceCollection_CollectionChanged;
            this.dispatcher = Dispatcher.CurrentDispatcher;
        }        
        
        public Application Application
        {
            get { return this.application; }
            set { this.application = value; RaisePropertyChanged("Application"); }
        }

        private void OpenApplication(MouseButtonEventArgs e)
        {
            if (e == null || e.ClickCount >= 2)
                Messenger.Default.Send(new NotificationMessage<Application>(this, this.Application, Messages.OpenApplication));
        }

        private bool CanStart()
        {
            return Application.CanStart;
        }

        private bool CanStop()
        {
            return Application.CanStop;
        }
        
        private void DeleteApplication()
        {
            Messenger.Default.Send(new NotificationMessage<Application>(this, this.Application, Messages.DeleteApplication));
        }

        private void StartApplication()
        {
            Messenger.Default.Send(new NotificationMessage<Application>(this, this.Application, Messages.StartApplication));
        }

        private void StopApplication()
        {
            Messenger.Default.Send(new NotificationMessage<Application>(this, this.Application, Messages.StopApplication));
        }

        private void RestartApplication()
        {
            Messenger.Default.Send(new NotificationMessage<Application>(this, this.Application, Messages.RestartApplication));
        }        

        public override void LoadChildren()
        {
            foreach (var instance in Application.InstanceCollection)
                base.Children.Add(new InstanceTreeViewItemViewModel(instance, this));
        }

        private void InstanceCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var comparer = new InstanceEqualityComparer();
            if (e.Action.Equals(NotifyCollectionChangedAction.Add))
            {
                foreach (var item in e.NewItems)
                {
                    var instance = item as Instance;
                    base.Children.Add(new InstanceTreeViewItemViewModel(instance, this));
                }
            }
            else if (e.Action.Equals(NotifyCollectionChangedAction.Remove))
            {                
                foreach (var item in e.OldItems)
                {
                    var toRemove = new List<TreeViewItemViewModel>();
                    var instance = item as Instance;
                    foreach (var treeView in base.Children)
                    {
                        var instanceTreeView = treeView as InstanceTreeViewItemViewModel;
                        if (instanceTreeView != null)
                        {
                            if (comparer.Equals(instanceTreeView.Instance, instance))
                                toRemove.Add(treeView);
                        }
                    }
                    foreach (var treeView in toRemove)
                        base.Children.Remove(treeView);
                }
            }
            else if (e.Action.Equals(NotifyCollectionChangedAction.Replace))
            {
                foreach(var item in e.NewItems)
                {
                    var instance = item as Instance;
                    foreach (var treeView in base.Children)
                    {
                        var instanceTreeView = treeView as InstanceTreeViewItemViewModel;
                        if (instanceTreeView != null)
                        {
                            if (comparer.Equals(instanceTreeView.Instance, instance))
                            {
                                instanceTreeView.Instance = instance;
                                if (!instanceTreeView.HasNotBeenPopulated)
                                    instanceTreeView.LoadChildren();
                            }
                        }
                    }
                }
            }
            else if (e.Action.Equals(NotifyCollectionChangedAction.Reset))
                base.Children.Clear();        
        }
    }
}