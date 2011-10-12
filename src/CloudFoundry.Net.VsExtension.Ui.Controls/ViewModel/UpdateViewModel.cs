namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Threading;
    using CloudFoundry.Net.Types;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
    
    public class UpdateViewModel : ViewModelBase
    {
        private Cloud selectedCloud;
        private Application application;
        private string errorMessage;
        private CloudFoundryProvider provider;
        private string name;
        public RelayCommand ConfirmedCommand { get; private set; }
        public RelayCommand CancelledCommand { get; private set; }
        public RelayCommand ManageCloudsCommand { get; private set; }

        public UpdateViewModel()
        {
            Messenger.Default.Send<NotificationMessageAction<CloudFoundryProvider>>(new NotificationMessageAction<CloudFoundryProvider>(Messages.GetCloudFoundryProvider, p => this.provider = p));
            ConfirmedCommand = new RelayCommand(Confirmed, () => SelectedCloud != null && SelectedApplication != null);
            CancelledCommand = new RelayCommand(Cancelled);
            ManageCloudsCommand = new RelayCommand(ManageClouds);

            InitializeData();
            RegisterGetData();
        }

        private void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<UpdateViewModel>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.GetUpdateAppData))
                        message.Execute(this);
                    Messenger.Default.Unregister(this);
                });
        }

        private void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<Guid>(Messages.SetUpdateAppData,
                (id) =>
                {                    
                    this.SelectedCloud = Clouds.SingleOrDefault(i => i.ID == id);
                }));
        }

        public string ErrorMessage
        {
            get { return this.errorMessage; }
            set { this.errorMessage = value; RaisePropertyChanged("ErrorMessage"); }
        }

        public string Name
        {
            get { return this.name; }
            set { this.name = value; RaisePropertyChanged("Name"); }
        }

        public ObservableCollection<Cloud> Clouds
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
                    Cloud local = this.provider.Connect(this.selectedCloud);
                    this.selectedCloud.Services.Synchronize(local.Services, new ProvisionedServiceEqualityComparer());
                    this.selectedCloud.Applications.Synchronize(local.Applications, new ApplicationEqualityComparer());
                    this.selectedCloud.AvailableServices.Synchronize(local.AvailableServices, new SystemServiceEqualityComparer());
                }
                RaisePropertyChanged("SelectedCloud");
                RaisePropertyChanged("Applications");
            }
        }

        public ObservableCollection<Application> Applications
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
            set
            {
                this.application = value;
                RaisePropertyChanged("SelectedApplication");
            }
        }

        private void Confirmed()
        {
            Messenger.Default.Send(new NotificationMessage<bool>(this, true, Messages.UpdateDialogResult));
        }

        private void Cancelled()
        {
            Messenger.Default.Send(new NotificationMessage<bool>(this, false, Messages.UpdateDialogResult));
            Messenger.Default.Unregister(this);
        }

        private void ManageClouds()
        {
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.ManageClouds, (confirmed) => { }));
        }
    }
}
