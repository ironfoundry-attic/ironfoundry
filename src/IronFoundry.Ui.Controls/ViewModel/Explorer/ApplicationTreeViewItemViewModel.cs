namespace IronFoundry.Ui.Controls.ViewModel.Explorer
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Windows.Input;
    using System.Windows.Threading;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using Model;
    using Mvvm;
    using Types;
    using Utilities;

    public class ApplicationTreeViewItemViewModel : TreeViewItemViewModel
    {
        private Application application;
        public Dispatcher dispatcher;
        private ICloudFoundryProvider provider;

        public ApplicationTreeViewItemViewModel(Application application, CloudTreeViewItemViewModel parentCloud)
            : base(parentCloud, true)
        {
            Messenger.Default.Send(new NotificationMessageAction<ICloudFoundryProvider>(
                                       Messages.GetCloudFoundryProvider, p => provider = p));
            OpenApplicationCommand = new RelayCommand<MouseButtonEventArgs>(OpenApplication);
            StartApplicationCommand = new RelayCommand(StartApplication, CanStart);
            StopApplicationCommand = new RelayCommand(StopApplication, CanStop);
            RestartApplicationCommand = new RelayCommand(RestartApplication, CanStop);
            DeleteApplicationCommand = new RelayCommand(DeleteApplication);

            Application = application;
            Application.InstanceCollection.CollectionChanged += InstanceCollection_CollectionChanged;
            dispatcher = Dispatcher.CurrentDispatcher;
        }

        public RelayCommand<MouseButtonEventArgs> OpenApplicationCommand { get; private set; }
        public RelayCommand StartApplicationCommand { get; private set; }
        public RelayCommand StopApplicationCommand { get; private set; }
        public RelayCommand RestartApplicationCommand { get; private set; }
        public RelayCommand DeleteApplicationCommand { get; private set; }

        public Application Application
        {
            get { return application; }
            set
            {
                application = value;
                RaisePropertyChanged("Application");
            }
        }

        private void OpenApplication(MouseButtonEventArgs e)
        {
            if (e == null || e.ClickCount >= 2)
                Messenger.Default.Send(new NotificationMessage<Application>(this, Application, Messages.OpenApplication));
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
            Messenger.Default.Send(new NotificationMessage<Application>(this, Application, Messages.DeleteApplication));
        }

        private void StartApplication()
        {
            Messenger.Default.Send(new NotificationMessage<Application>(this, Application, Messages.StartApplication));
        }

        private void StopApplication()
        {
            Messenger.Default.Send(new NotificationMessage<Application>(this, Application, Messages.StopApplication));
        }

        private void RestartApplication()
        {
            Messenger.Default.Send(new NotificationMessage<Application>(this, Application, Messages.RestartApplication));
        }

        public override void LoadChildren()
        {
            foreach (Instance instance in Application.InstanceCollection)
                base.Children.Add(new InstanceTreeViewItemViewModel(instance, this));
        }

        private void InstanceCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var comparer = new InstanceEqualityComparer();
            if (e.Action.Equals(NotifyCollectionChangedAction.Add))
            {
                foreach (object item in e.NewItems)
                {
                    var instance = item as Instance;
                    base.Children.Add(new InstanceTreeViewItemViewModel(instance, this));
                }
            }
            else if (e.Action.Equals(NotifyCollectionChangedAction.Remove))
            {
                foreach (object item in e.OldItems)
                {
                    var toRemove = new List<TreeViewItemViewModel>();
                    var instance = item as Instance;
                    foreach (TreeViewItemViewModel treeView in base.Children)
                    {
                        var instanceTreeView = treeView as InstanceTreeViewItemViewModel;
                        if (instanceTreeView != null)
                        {
                            if (comparer.Equals(instanceTreeView.Instance, instance))
                                toRemove.Add(treeView);
                        }
                    }
                    foreach (TreeViewItemViewModel treeView in toRemove)
                        base.Children.Remove(treeView);
                }
            }
            else if (e.Action.Equals(NotifyCollectionChangedAction.Replace))
            {
                foreach (object item in e.NewItems)
                {
                    var instance = item as Instance;
                    foreach (TreeViewItemViewModel treeView in base.Children)
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