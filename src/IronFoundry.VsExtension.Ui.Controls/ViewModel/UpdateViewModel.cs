namespace IronFoundry.VsExtension.Ui.Controls.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using IronFoundry.Types;
    using IronFoundry.VsExtension.Ui.Controls.Mvvm;
    using IronFoundry.VsExtension.Ui.Controls.Utilities;
    
    public class UpdateViewModel : DialogViewModel
    {
        private Cloud selectedCloud;
        private Application application;
        private string pushFromDirectory;
        private bool canChangeDirectory = true;
        private string name;
        public RelayCommand ManageCloudsCommand { get; private set; }
        public RelayCommand ChooseDirectoryCommand { get; private set; }

        public UpdateViewModel() : base(Messages.UpdateDialogResult)
        {
            ManageCloudsCommand = new RelayCommand(ManageClouds);
            ChooseDirectoryCommand = new RelayCommand(ChooseDirectory, CanChooseDirectory);
        }

        protected override void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<UpdateViewModel>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.GetUpdateAppData))
                        message.Execute(this);
                    Cleanup();
                });
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<Guid>(Messages.SetUpdateAppData,
                (id) =>
                {                    
                    this.SelectedCloud = Clouds.SingleOrDefault(i => i.ID == id);
                }));

            Messenger.Default.Send(new NotificationMessageAction<string>(Messages.SetPushAppDirectory,
                (directory) =>
                {
                    this.PushFromDirectory = directory;
                    this.CanChangeDirectory = false;
                }));
        }

        protected override bool CanExecuteConfirmed()
        {
            return SelectedCloud != null && SelectedApplication != null;
        }

        private bool CanChooseDirectory()
        {
            return this.canChangeDirectory;
        }

        public string Name
        {
            get { return this.name; }
            set { this.name = value; RaisePropertyChanged("Name"); }
        }

        public SafeObservableCollection<Cloud> Clouds
        {
            get { return provider.Clouds; }
        }

        public Cloud SelectedCloud
        {
            get { return this.selectedCloud; }
            set
            {
                this.selectedCloud = value;
                if (this.selectedCloud != null)
                {
                    var local = this.provider.Connect(this.selectedCloud);
                    if (local.Response != null)
                    {
                        this.selectedCloud.Services.Synchronize(local.Response.Services, new ProvisionedServiceEqualityComparer());
                        this.selectedCloud.Applications.Synchronize(local.Response.Applications, new ApplicationEqualityComparer());
                        this.selectedCloud.AvailableServices.Synchronize(local.Response.AvailableServices, new SystemServiceEqualityComparer());
                    }
                    else
                    {
                        this.ErrorMessage = local.Message;
                    }
                }
                RaisePropertyChanged("SelectedCloud");
                RaisePropertyChanged("Applications");
            }
        }

        public SafeObservableCollection<Application> Applications
        {
            get
            {
                if (this.selectedCloud == null)
                    return null;
                else
                    return this.selectedCloud.Applications;
            }

        }

        public Application SelectedApplication
        {
            get { return this.application; }
            set { this.application = value; RaisePropertyChanged("SelectedApplication"); }
        }

        public string PushFromDirectory
        {
            get { return this.pushFromDirectory; }
            set { this.pushFromDirectory = value; RaisePropertyChanged("PushFromDirectory"); }
        }

        public bool CanChangeDirectory
        {
            get { return this.canChangeDirectory; }
            set { this.canChangeDirectory = value; RaisePropertyChanged("CanChangeDirectory"); }
        }

        private void ManageClouds()
        {
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.ManageClouds, (confirmed) => { }));
        }

        private void ChooseDirectory()
        {
            Messenger.Default.Send(new NotificationMessageAction<string>(Messages.ChooseDirectory, (directory) =>
            {
                if (directory != null)
                    this.PushFromDirectory = directory;
            }));
        }
    }
}
