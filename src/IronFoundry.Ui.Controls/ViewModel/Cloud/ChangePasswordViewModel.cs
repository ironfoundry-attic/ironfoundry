namespace IronFoundry.Ui.Controls.ViewModel.Cloud
{
    using System;
    using System.ComponentModel;
    using GalaSoft.MvvmLight.Messaging;
    using Model;
    using Models;
    using Mvvm;
    using Utilities;

    public class ChangePasswordViewModel : DialogViewModel
    {
        private Cloud cloud;
        private string email;
        private string newPassword;
        private string verifyPassword;

        public ChangePasswordViewModel() : base(Messages.ChangePasswordDialogResult)
        {
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
            get { return email; }
            set
            {
                email = value;
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

        protected override void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<ChangePasswordViewModel>>(this,
                                                                                           message =>
                                                                                           {
                                                                                               if (
                                                                                                   message.Notification.
                                                                                                       Equals(
                                                                                                           Messages.
                                                                                                               GetChangePasswordData))
                                                                                                   message.Execute(this);
                                                                                               Messenger.Default.
                                                                                                   Unregister(this);
                                                                                           });
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<Cloud>(Messages.SetChangePasswordData, c =>
            {
                EMail = c.Email;
                cloud = c;
            }));
        }

        protected override void OnConfirmed(CancelEventArgs e)
        {
            if (String.IsNullOrEmpty(NewPassword) ||
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
                ProviderResponse<bool> result = provider.ChangePassword(cloud, NewPassword);
                if (!result.Response)
                {
                    ErrorMessage = result.Message;
                    e.Cancel = true;
                }
            }
        }
    }
}