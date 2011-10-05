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

    [ExportViewModel("Update",true)]
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
            ConfirmedCommand = new RelayCommand(Confirmed);
            CancelledCommand = new RelayCommand(Cancelled);
            ManageCloudsCommand = new RelayCommand(ManageClouds);            
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
            set { this.selectedCloud = value; 
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
        }

        private void ManageClouds()
        {
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.ManageClouds, (confirmed) => { }));
        }
    }
}
