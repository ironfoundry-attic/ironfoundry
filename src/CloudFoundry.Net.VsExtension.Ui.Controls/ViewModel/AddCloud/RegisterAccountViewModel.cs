using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using CloudFoundry.Net.Types;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class RegisterAccountViewModel : DialogViewModel
    {
        private string newPassword = string.Empty;
        private string verifyPassword = string.Empty;
        private string eMail = string.Empty;

        public RegisterAccountViewModel() : base(Messages.RegisterAccountDialogResult)
        {            
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<Cloud>(Messages.SetRegisterAccountData,
                (cloud) =>
                {
                    this.EMail = cloud.Email;
                    this.NewPassword = cloud.Password;
                }));
        }

        protected override void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<RegisterAccountViewModel>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.GetRegisterAccountData))
                    {
                        message.Execute(this);
                        Messenger.Default.Unregister(this);
                    }
                });
        }

        public string NewPassword
        {
            get { return newPassword; }
            set { newPassword = value; RaisePropertyChanged("NewPassword"); }
        }

        public string EMail
        {
            get { return eMail; }
            set { eMail = value; RaisePropertyChanged("EMail"); }
        }

        public string VerifyPassword
        {
            get { return verifyPassword; }
            set { verifyPassword = value; RaisePropertyChanged("VerifyPassword"); }
        }
    }
}
