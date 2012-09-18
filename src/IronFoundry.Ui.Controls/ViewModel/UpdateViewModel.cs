namespace IronFoundry.Ui.Controls.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using Model;
    using Mvvm;
    using Types;
    using Utilities;

    public class UpdateViewModel : DialogViewModel
    {
        private Application application;
        private bool canChangeDirectory = true;
        private string name;
        private string pushFromDirectory;
        private Types.Cloud selectedCloud;

        public UpdateViewModel() : base(Messages.UpdateDialogResult)
        {
            ManageCloudsCommand = new RelayCommand(ManageClouds);
            ChooseDirectoryCommand = new RelayCommand(ChooseDirectory, CanChooseDirectory);
        }

        public RelayCommand ManageCloudsCommand { get; private set; }
        public RelayCommand ChooseDirectoryCommand { get; private set; }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
        }

        // public SafeObservableCollection<Types.Cloud> Clouds
        public IEnumerable<Types.Cloud> Clouds
        {
            get { return provider.Clouds; }
        }

        public Types.Cloud SelectedCloud
        {
            get { return selectedCloud; }
            set
            {
                selectedCloud = value;
                if (selectedCloud != null)
                {
                    ProviderResponse<Types.Cloud> local = provider.Connect(selectedCloud);
                    if (local.Response != null)
                    {
                        selectedCloud.Services.Synchronize(local.Response.Services,
                                                           new ProvisionedServiceEqualityComparer());
                        selectedCloud.Applications.Synchronize(local.Response.Applications,
                                                               new ApplicationEqualityComparer());
                        selectedCloud.AvailableServices.Synchronize(local.Response.AvailableServices,
                                                                    new SystemServiceEqualityComparer());
                    }
                    else
                    {
                        ErrorMessage = local.Message;
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
                if (selectedCloud == null)
                    return null;
                else
                    return selectedCloud.Applications;
            }
        }

        public Application SelectedApplication
        {
            get { return application; }
            set
            {
                application = value;
                RaisePropertyChanged("SelectedApplication");
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
            Messenger.Default.Register<NotificationMessageAction<UpdateViewModel>>(this,
                                                                                   message =>
                                                                                   {
                                                                                       if (
                                                                                           message.Notification.Equals(
                                                                                               Messages.GetUpdateAppData))
                                                                                           message.Execute(this);
                                                                                       Cleanup();
                                                                                   });
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<Guid>(Messages.SetUpdateAppData,
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

        protected override bool CanExecuteConfirmed()
        {
            return SelectedCloud != null && SelectedApplication != null;
        }

        private bool CanChooseDirectory()
        {
            return canChangeDirectory;
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
    }
}