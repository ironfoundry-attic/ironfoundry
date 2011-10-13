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
    public class ChangePasswordViewModel : DialogViewModel
    {
        private string newPassword;
        private string verifyPassword;
        private string email;

        public ChangePasswordViewModel() : base(Messages.ChangePasswordDialogResult)
        {
        }

        protected override void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<ChangePasswordViewModel>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.GetChangePasswordData))
                        message.Execute(this);
                    Messenger.Default.Unregister(this);
                });
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<Cloud>(Messages.SetChangePasswordData, c => this.EMail = c.Email));
        }

        public string NewPassword
        {
            get { return newPassword; }
            set { newPassword = value; RaisePropertyChanged("NewPassword"); }
        }

        public string EMail
        {
            get { return email; }
            set { email = value; RaisePropertyChanged("EMail"); }
        }

        public string VerifyPassword
        {
            get { return verifyPassword; }
            set { verifyPassword = value; RaisePropertyChanged("VerifyPassword"); }
        }
    }
}
