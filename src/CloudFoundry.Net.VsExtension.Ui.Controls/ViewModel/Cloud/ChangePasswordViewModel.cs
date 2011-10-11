using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
using CloudFoundry.Net.Types;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    [ExportViewModel("ChangePassword",false)]
    public class ChangePasswordViewModel : ViewModelBase
    {
        private string newPassword;
        private string verifyPassword;
        private string eMail;

        public RelayCommand ConfirmedCommand { get; private set; }
        public RelayCommand CancelledCommand { get; private set; }

        public ChangePasswordViewModel()
        {
            ConfirmedCommand = new RelayCommand(Confirmed);
            CancelledCommand = new RelayCommand(Cancelled);

            RegisterGetData();
            InitializeData();
        }

        private void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<ChangePasswordViewModel>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.GetChangePasswordData))
                        message.Execute(this);
                    Messenger.Default.Unregister(this);
                });
        }

        private void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<Cloud>(Messages.SetChangePasswordData,
                (cloud) =>
                {
                    this.EMail = cloud.Email;
                }));
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
            Messenger.Default.Unregister(this);
        }
    }
}
