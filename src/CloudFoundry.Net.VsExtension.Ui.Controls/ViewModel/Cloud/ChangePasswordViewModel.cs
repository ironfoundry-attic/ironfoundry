using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    [ExportViewModel("ChangePassword",false)]
    public class ChangePasswordViewModel : ViewModelBase
    {
        private string newPassword = string.Empty;
        private string verifyPassword = string.Empty;
        private string eMail = string.Empty;

        public RelayCommand ConfirmedCommand { get; private set; }
        public RelayCommand CancelledCommand { get; private set; }

        public ChangePasswordViewModel()
        {
            ConfirmedCommand = new RelayCommand(Confirmed);
            CancelledCommand = new RelayCommand(Cancelled);
            
            // Send a message back to the caller, to intialize data
            // in this case - email address.
            Messenger.Default.Send(new NotificationMessageAction<string>(Messages.ChangePasswordEmailAddress,
                (emailAddress) => {
                    this.EMail = emailAddress;
                }));
            
            if (IsInDesignMode)
            {
                this.NewPassword = "TestPassword";
                this.VerifyPassword = "TestPassword";
                this.EMail = "test.email@cloudfoundry.com";
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

        public string EMail
        {
            get { return eMail; }
            set
            {
                eMail = value;
                RaisePropertyChanged("EMail");
            }
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


        private void Confirmed()
        {
            Messenger.Default.Send(new NotificationMessage<bool>(this, true, Messages.ChangePasswordDialogResult));
        }

        private void Cancelled()
        {
            Messenger.Default.Send(new NotificationMessage<bool>(this, false, Messages.ChangePasswordDialogResult));
        }
    }
}
