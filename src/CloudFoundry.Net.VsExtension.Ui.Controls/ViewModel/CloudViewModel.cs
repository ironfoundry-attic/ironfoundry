using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    //[ExportViewModel("FoundryProperties", false)]
    public class CloudViewModel : ViewModelBase
    {       
        private Cloud cloud;

        public RelayCommand ChangePasswordCommand { get; private set; }
        public RelayCommand ValidateAccountCommand { get; private set; }
        public RelayCommand ConnectCommand { get; private set; }
        public RelayCommand DisconnectCommand { get; private set; }

        public CloudViewModel(Cloud cloud)
        {
            ChangePasswordCommand = new RelayCommand(ChangePassword, () => true);
            ValidateAccountCommand = new RelayCommand(ValidateAccount, CanExecuteValidateAccount);
            ConnectCommand = new RelayCommand(Connect, CanExecuteConnect);
            DisconnectCommand = new RelayCommand(Disconnect, CanExecuteDisconnect);

            this.cloud = cloud;
        }

        private void ChangePassword()
        {
            // Register to initialize data in dialog
            Messenger.Default.Register<NotificationMessageAction<string>>(this,
                message => {
                    if (message.Notification.Equals(Messages.ChangePasswordEmailAddress))
                        message.Execute(this.EMail);
                });

            // Fire message to open dialog
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.ChangePassword,
                (confirmed) =>
                {
                    if (confirmed)
                    {

                    }
                }));
        }
        
        private void ValidateAccount()
        {

        }

        private bool CanExecuteValidateAccount()
        {
            return !String.IsNullOrEmpty(EMail) &&
                   !String.IsNullOrEmpty(Password) &&
                   !String.IsNullOrEmpty(Url);
        }

        private void Connect()
        {
            this.cloud.Connected = true;
        }

        private bool CanExecuteConnect()
        {
            return !this.cloud.Connected;
        }

        private void Disconnect()
        {
            this.cloud.Connected = false;
        }

        private bool CanExecuteDisconnect()
        {
            return this.cloud.Connected;
        }
        
        public string ServerName
        {
            get { return this.cloud.ServerName; }
            set
            {
                this.cloud.ServerName = value;
                RaisePropertyChanged("ServerName");
            }
        }

        public string HostName
        {
            get { return this.cloud.HostName; }
            set
            {
                this.cloud.HostName = value;
                RaisePropertyChanged("HostName");
            }
        }

        public string EMail
        {
            get { return this.cloud.Email; }
            set
            {
                this.cloud.Email = value;
                RaisePropertyChanged("EMail");
            }
        }

        public string Password
        {
            get { return this.cloud.Password; }
            set
            {
                this.cloud.Password = value;
                RaisePropertyChanged("Password");
            }
        }

        public string Url
        {
            get { return this.cloud.Url; }
            set
            {
                this.cloud.Url = value;
                RaisePropertyChanged("Url");
            }
        }

        public int TimeoutStart
        {
            get { return this.cloud.TimeoutStart; }
            set
            {
                this.cloud.TimeoutStart = value;
                RaisePropertyChanged("TimeoutStart");
            }
        }

        public int TimeoutStop
        {
            get { return this.cloud.TimeoutStop; }
            set
            {
                this.cloud.TimeoutStop = value;
                RaisePropertyChanged("TimeoutEnd");
            }
        }

        public List<CloudFoundry.Net.VsExtension.Ui.Controls.Model.Application> Applications
        {
            get { return this.cloud.Applications; }
        }

        private CloudFoundry.Net.VsExtension.Ui.Controls.Model.Application selectedApplication;
        public CloudFoundry.Net.VsExtension.Ui.Controls.Model.Application SelectedApplication
        {
            get { return this.selectedApplication; }
            set
            {
                this.selectedApplication = value;
                this.Instances = this.selectedApplication.Instances;
                RaisePropertyChanged("SelectedApplication");
            }
        }

        public List<CloudFoundry.Net.VsExtension.Ui.Controls.Model.Service> Services
        {
            get { return this.cloud.Services; }
        }

        private List<Instance> instances;
        public List<Instance> Instances
        {
            get { return this.instances; }
            set
            {
                this.instances = value;
                RaisePropertyChanged("Instances");
            }
        }
    }
}
