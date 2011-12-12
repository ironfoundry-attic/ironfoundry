namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    using System;
    using System.ComponentModel;
    using CloudFoundry.Net.Types;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
    using GalaSoft.MvvmLight.Messaging;

    public class RegisterAccountViewModel : DialogViewModel
    {
        private Cloud cloud;
        private string newPassword = string.Empty;
        private string verifyPassword = string.Empty;
        private string eMail = string.Empty;

        public RegisterAccountViewModel() : base(Messages.RegisterAccountDialogResult)
        {
            OnConfirmed += ConfirmRegister;
        }

        void ConfirmRegister(object sender, CancelEventArgs e)
        {
            if (String.IsNullOrEmpty(EMail))
            {
                e.Cancel = true;
                ErrorMessage = "Email Address cannot be empty.";
            }
            else if (String.IsNullOrEmpty(NewPassword) ||
                String.IsNullOrEmpty(VerifyPassword))
            {
                e.Cancel = true;
                ErrorMessage = "Passwords cannot be empty.";
            }
            else if (!NewPassword.Equals(VerifyPassword))
            {
                e.Cancel = true;
                ErrorMessage = "Passwords must match.";
            }
            else
            {
                var result = provider.RegisterAccount(this.cloud, EMail, NewPassword);
                if (!result.Response)
                {
                    ErrorMessage = result.Message;
                    e.Cancel = true;
                }
            }
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<Cloud>(Messages.SetRegisterAccountData,
                (cloud) =>
                {
                    this.cloud = cloud;
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