using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class ChangePasswordViewModel : ViewModelBase
    {
        private string newPassword = string.Empty;
        private string verifyPassword = string.Empty;
        private string eMail = string.Empty;

        public RelayCommand ConfirmedPasswordCommand { get; private set; }
        public RelayCommand CancelledPasswordCommand { get; private set; }

        public ChangePasswordViewModel()
        {
            ConfirmedPasswordCommand = new RelayCommand(ConfirmedPassword, () => true);
            CancelledPasswordCommand = new RelayCommand(CancelledPassword, () => true);
            
            Messenger.Default.Register<NotificationMessage<string>>(
                this,
                message =>
                {
                    if (message.Notification.Equals(Messages.ChangePasswordEmailAddress))
                        this.eMail = message.Content;
                });

            if (IsInDesignMode)
            {
                this.NewPassword = "TestPassword";
                this.VerifyPassword = "TestPassword";
            }
        }

        public string NewPassword
        {
            get { return newPassword; }
            set
            {
                newPassword = value;
                RaisePropertyChanged("NewPassword");
            }
        }

        public string Email
        {
            get { return eMail; }
        }

        public string VerifyPassword
        {
            get { return verifyPassword; }
            set
            {
                verifyPassword = value;
                RaisePropertyChanged("VerifyPassword");
            }
        }


        private void ConfirmedPassword()
        {
            var message = new NotificationMessage<bool>(this, true, Messages.ChangePasswordDialogResult);
            Messenger.Default.Send(message);
        }

        private void CancelledPassword()
        {
            var message = new NotificationMessage<bool>(this, false, Messages.ChangePasswordDialogResult);
            Messenger.Default.Send(message);
        }
    }
}
