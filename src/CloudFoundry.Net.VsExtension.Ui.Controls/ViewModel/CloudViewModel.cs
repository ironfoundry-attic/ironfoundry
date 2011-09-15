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
        private string serverName = string.Empty;
        private string hostName = string.Empty;
        private string eMail = string.Empty;
        private string password = string.Empty;
        private string changedPassword = string.Empty;
        private string url = string.Empty;
        private int timeoutStart = 0;
        private int timeoutEnd = 60;
        private bool isConnected = false;

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

            this.ServerName = cloud.ServerName;            
        }

        private void ChangePassword()
        {
            // Register to initialize data in dialog
            Messenger.Default.Register<NotificationMessageAction<string>>(this,
                message => {
                    if (message.Notification.Equals(Messages.ChangePasswordEmailAddress))
                        message.Execute(this.eMail);
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
            isConnected = true;
        }

        private bool CanExecuteConnect()
        {
            return !isConnected;
        }

        private void Disconnect()
        {
            isConnected = false;
        }

        private bool CanExecuteDisconnect()
        {
            return isConnected;
        }
        
        public string ServerName
        {
            get { return serverName; }
            set
            {
                serverName = value;
                RaisePropertyChanged("ServerName");
            }
        }

        public string HostName
        {
            get { return hostName; }
            set
            {
                hostName = value;
                RaisePropertyChanged("HostName");
            }
        }

        public string EMail
        {
            get { return eMail; }
            set
            {
                eMail = value;
                RaisePropertyChanged("EMail");
            }
        }

        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                RaisePropertyChanged("Password");
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

        public int TimeoutStart
        {
            get { return timeoutStart; }
            set
            {
                timeoutStart = value;
                RaisePropertyChanged("TimeoutStart");
            }
        }

        public int TimeoutEnd
        {
            get { return timeoutEnd; }
            set
            {
                timeoutEnd = value;
                RaisePropertyChanged("TimeoutEnd");
            }
        }
    }
}
