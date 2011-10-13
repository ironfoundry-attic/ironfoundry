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
    
    public class UpdateViewModel : DialogViewModel
    {
        private Cloud selectedCloud;
        private Application application;        
        private string name;
        public RelayCommand ManageCloudsCommand { get; private set; }

        public UpdateViewModel() : base(Messages.UpdateDialogResult)
        {
            ManageCloudsCommand = new RelayCommand(ManageClouds);
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
        }

        protected override bool CanExecuteConfirmed()
        {
            return SelectedCloud != null && SelectedApplication != null;
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
            set { this.application = value; RaisePropertyChanged("SelectedApplication"); }
        }

        private void ManageClouds()
        {
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.ManageClouds, (confirmed) => { }));
        }
    }
}
