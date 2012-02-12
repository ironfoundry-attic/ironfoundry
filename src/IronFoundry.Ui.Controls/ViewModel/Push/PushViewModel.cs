namespace IronFoundry.Ui.Controls.ViewModel.Push
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using IronFoundry.Utilities;
    using Model;
    using Mvvm;
    using Types;
    using Utilities;

    public class PushViewModel : DialogViewModel
    {
        private SafeObservableCollection<ProvisionedService> applicationServices;
        private bool canChangeDirectory = true;
        private int instances = 1;
        private string name;
        private string pushFromDirectory;
        private Cloud selectedCloud;
        private int selectedMemory;
        private string url;

        public PushViewModel() : base(Messages.PushDialogResult)
        {
            applicationServices = new SafeObservableCollection<ProvisionedService>();
            ManageCloudsCommand = new RelayCommand(ManageClouds);
            AddAppServiceCommand = new RelayCommand(AddAppService, CanAddAppService);
            ChooseDirectoryCommand = new RelayCommand(ChooseDirectory, CanChooseDirectory);
            SelectedMemory = MemoryLimits[0];
        }

        public RelayCommand ManageCloudsCommand { get; private set; }
        public RelayCommand AddAppServiceCommand { get; private set; }
        public RelayCommand ChooseDirectoryCommand { get; private set; }

        public Cloud SelectedCloud
        {
            get { return selectedCloud; }
            set
            {
                Cloud newCloud = value;
                if (ValuesAreDifferent(selectedCloud, newCloud) && null != newCloud)
                {
                    ProviderResponse<Cloud> local = provider.Connect(newCloud);
                    if (local.Response != null)
                    {
                        newCloud.Services.Synchronize(local.Response.Services, new ProvisionedServiceEqualityComparer());
                        newCloud.Applications.Synchronize(local.Response.Applications, new ApplicationEqualityComparer());
                        newCloud.AvailableServices.Synchronize(local.Response.AvailableServices, new SystemServiceEqualityComparer());
                    }
                    else
                    {
                        ErrorMessage = local.Message;
                    }
                }
                SetValue(ref selectedCloud, newCloud, "SelectedCloud");
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (SetValue(ref name, value, "Name") && null != SelectedCloud)
                {
                    Url = SelectedCloud.BuildTypicalApplicationUrl(name);
                }
            }
        }

        public string Url
        {
            get { return url; }
            set
            {
                url = value;
                RaisePropertyChanged("Url");
            }
        }

        public string PushFromDirectory
        {
            get { return pushFromDirectory; }
            set
            {
                pushFromDirectory = value;
                RaisePropertyChanged("PushFromDirectory");
            }
        }

        // public SafeObservableCollection<Cloud> Clouds
        public IEnumerable<Cloud> Clouds
        {
            get { return provider.Clouds; }
        }

        public int[] MemoryLimits
        {
            get { return Constants.MemoryLimits; }
        }

        public int SelectedMemory
        {
            get { return selectedMemory; }
            set
            {
                selectedMemory = value;
                RaisePropertyChanged("SelectedMemory");
            }
        }

        public int Instances
        {
            get { return instances; }
            set
            {
                instances = value;
                RaisePropertyChanged("Instances");
            }
        }

        public SafeObservableCollection<ProvisionedService> ApplicationServices
        {
            get { return applicationServices; }
            set
            {
                applicationServices = value;
                RaisePropertyChanged("ApplicationServices");
            }
        }

        public bool CanChangeDirectory
        {
            get { return canChangeDirectory; }
            set
            {
                canChangeDirectory = value;
                RaisePropertyChanged("CanChangeDirectory");
            }
        }

        protected override void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<PushViewModel>>(this,
                                                                                 message =>
                                                                                 {
                                                                                     if (
                                                                                         message.Notification.Equals(
                                                                                             Messages.GetPushAppData))
                                                                                         message.Execute(this);
                                                                                     Messenger.Default.Unregister(this);
                                                                                 });
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<Guid>(Messages.SetPushAppData,
                                                                       (id) =>
                                                                       {
                                                                           SelectedCloud =
                                                                               Clouds.SingleOrDefault(i => i.ID == id);
                                                                       }));

            Messenger.Default.Send(new NotificationMessageAction<string>(Messages.SetPushAppDirectory,
                                                                         (directory) =>
                                                                         {
                                                                             PushFromDirectory = directory;
                                                                             CanChangeDirectory = false;
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
                    PushFromDirectory = directory;
            }));
        }

        private void AddAppService()
        {
            Messenger.Default.Register<NotificationMessageAction<Cloud>>(this,
                                                                         message =>
                                                                         {
                                                                             if (
                                                                                 message.Notification.Equals(
                                                                                     Messages.
                                                                                         SetAddApplicationServiceData))
                                                                                 message.Execute(SelectedCloud);
                                                                         });

            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.AddApplicationService, (confirmed) =>
                                                                                                       Messenger.Default
                                                                                                           .Send(
                                                                                                               new NotificationMessageAction
                                                                                                                   <
                                                                                                                   AddApplicationServiceViewModel
                                                                                                                   >(
                                                                                                                   Messages
                                                                                                                       .
                                                                                                                       GetAddApplicationServiceData,
                                                                                                                   (
                                                                                                                       viewModel)
                                                                                                                   =>
                                                                                                                   {
                                                                                                                       if
                                                                                                                           (
                                                                                                                           !ApplicationServices
                                                                                                                                .
                                                                                                                                Contains
                                                                                                                                (viewModel
                                                                                                                                     .
                                                                                                                                     SelectedService,
                                                                                                                                 new ProvisionedServiceEqualityComparer
                                                                                                                                     ()))
                                                                                                                           ApplicationServices
                                                                                                                               .
                                                                                                                               Add
                                                                                                                               (viewModel
                                                                                                                                    .
                                                                                                                                    SelectedService);
                                                                                                                   }))));
        }

        private bool CanChooseDirectory()
        {
            return canChangeDirectory;
        }

        private bool CanAddAppService()
        {
            return SelectedCloud != null && SelectedCloud.Services.Count > 0;
        }
    }
}