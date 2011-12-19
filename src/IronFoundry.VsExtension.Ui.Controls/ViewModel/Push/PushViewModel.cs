namespace IronFoundry.VsExtension.Ui.Controls.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using IronFoundry.Types;
    using IronFoundry.Utilities;
    using IronFoundry.VsExtension.Ui.Controls.Mvvm;
    using IronFoundry.VsExtension.Ui.Controls.Utilities;

    public class PushViewModel : DialogViewModel
    {
        private Cloud selectedCloud;
        private SafeObservableCollection<ProvisionedService> applicationServices;
        private string name;
        private string url;
        private string pushFromDirectory;
        private int selectedMemory;
        private int instances = 1;
        private bool canChangeDirectory = true;

        public RelayCommand ManageCloudsCommand { get; private set; }
        public RelayCommand AddAppServiceCommand { get; private set; }
        public RelayCommand ChooseDirectoryCommand { get; private set; }

        public PushViewModel() : base(Messages.PushDialogResult)
        {
            this.applicationServices = new SafeObservableCollection<ProvisionedService>();
            ManageCloudsCommand = new RelayCommand(ManageClouds);
            AddAppServiceCommand = new RelayCommand(AddAppService, CanAddAppService);
            ChooseDirectoryCommand = new RelayCommand(ChooseDirectory, CanChooseDirectory);
            SelectedMemory = MemoryLimits[0];
        }

        protected override void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<PushViewModel>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.GetPushAppData))
                        message.Execute(this);
                    Messenger.Default.Unregister(this);
                });
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<Guid>(Messages.SetPushAppData,
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
       
        private void AddAppService()
        {
            
            Messenger.Default.Register<NotificationMessageAction<Cloud>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.SetAddApplicationServiceData))
                        message.Execute(this.SelectedCloud);
                });

            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.AddApplicationService, (confirmed) => 
                Messenger.Default.Send(new NotificationMessageAction<AddApplicationServiceViewModel>(Messages.GetAddApplicationServiceData,
                (viewModel) =>
                {
                    if (!this.ApplicationServices.Contains(viewModel.SelectedService,new ProvisionedServiceEqualityComparer()))
                        this.ApplicationServices.Add(viewModel.SelectedService);    
                }))));            
        }

        private bool CanChooseDirectory()
        {
            return this.canChangeDirectory;
        }

        private bool CanAddAppService()
        {
            return this.SelectedCloud != null && this.SelectedCloud.Services.Count > 0;
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
            }
        }
        
        public string Name
        {
            get { return this.name; }
            set { this.name = value; RaisePropertyChanged("Name"); }
        }

        public string Url
        {
            get { return this.url; }
            set { this.url = value; RaisePropertyChanged("Url"); }
        }

        public string PushFromDirectory
        {
            get { return this.pushFromDirectory; }
            set { this.pushFromDirectory = value; RaisePropertyChanged("PushFromDirectory"); }
        }

        public SafeObservableCollection<Cloud> Clouds
        {
            get { return provider.Clouds; }
        }

        public int[] MemoryLimits 
        { 
            get { return Constants.MemoryLimits; } 
        }      

        public int SelectedMemory
        {
            get { return this.selectedMemory; }
            set { this.selectedMemory = value; RaisePropertyChanged("SelectedMemory"); }
        }

        public int Instances
        {
            get { return this.instances; }
            set { this.instances = value; RaisePropertyChanged("Instances"); }
        }

        public SafeObservableCollection<ProvisionedService> ApplicationServices
        {
            get { return this.applicationServices; }
            set { this.applicationServices = value; RaisePropertyChanged("ApplicationServices"); }
        }

        public bool CanChangeDirectory
        {
            get { return this.canChangeDirectory; }
            set { this.canChangeDirectory = value; RaisePropertyChanged("CanChangeDirectory"); }
        }
    }
}
