#if TODO
namespace IronFoundry.Ui.Controls.ViewModel.AddCloud
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Threading;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using Model;
    using Mvvm;
    using Types;
    using Utilities;

    public class AddCloudViewModel : DialogViewModel
    {
        private readonly Dispatcher dispatcher;
        private SafeObservableCollection<CloudUrl> cloudUrls;
        private bool isAccountValid;
        private CloudUrl selectedCloudUrl;

        public AddCloudViewModel() : base(Messages.AddCloudDialogResult)
        {
            Cloud = new Cloud();
            dispatcher = Dispatcher.CurrentDispatcher;
            ValidateAccountCommand = new RelayCommand(ValidateAccount, CanValidate);
            RegisterAccountCommand = new RelayCommand(RegisterAccount, CanRegister);
            // TODO ManageCloudUrlsCommand = new RelayCommand(ManageCloudUrls);
            cloudUrls = provider.CloudUrls;
            SelectedCloudUrl = cloudUrls.SingleOrDefault((i) => i.IsDefault);
        }

        // public RelayCommand ManageCloudUrlsCommand { get; private set; }

        public RelayCommand ValidateAccountCommand { get; private set; }
        public RelayCommand RegisterAccountCommand { get; private set; }

        public Cloud Cloud { get; private set; }

        public CloudUrl SelectedCloudUrl
        {
            get { return selectedCloudUrl; }
            set
            {
                IsAccountValid = false;
                selectedCloudUrl = value;
                if (selectedCloudUrl != null)
                    Cloud.Url = selectedCloudUrl.Url;
                else
                    Cloud.Url = string.Empty;
                RaisePropertyChanged("SelectedCloudUrl");
            }
        }

        public bool IsAccountValid
        {
            get { return isAccountValid; }
            set
            {
                isAccountValid = value;
                RaisePropertyChanged("IsAccountValid");
            }
        }

        public SafeObservableCollection<CloudUrl> CloudUrls
        {
            get { return cloudUrls; }
            set
            {
                cloudUrls = value;
                RaisePropertyChanged("CloudUrls");
            }
        }

        public string ServerName
        {
            get { return Cloud.ServerName; }
            set
            {
                Cloud.ServerName = value;
                RaisePropertyChanged("ServerName");
            }
        }

        public string EMail
        {
            get { return Cloud.Email; }
            set
            {
                Cloud.Email = value;
                RaisePropertyChanged("EMail");
            }
        }

        public string Password
        {
            get { return Cloud.Password; }
            set
            {
                Cloud.Password = value;
                RaisePropertyChanged("Password");
            }
        }

        protected override void OnConfirmed(CancelEventArgs e)
        {
            dispatcher.BeginInvoke(new Action(() => { provider.Clouds.SafeAdd(Cloud); }));
            Cleanup();
        }

        private void ValidateAccount()
        {
            ErrorMessage = string.Empty;
            IsAccountValid = false;
            ProviderResponse<bool> result = provider.ValidateAccount(Cloud);
            if (result.Response)
            {
                IsAccountValid = true;
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }

        private void RegisterAccount()
        {
            IsAccountValid = false;
            Messenger.Default.Register<NotificationMessageAction<Cloud>>(this,
                 message =>
                 {
                     if (message.Notification.Equals(Messages.SetRegisterAccountData))
                         message.Execute(Cloud);
                 });

            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.RegisterAccount,
               (confirmed) =>
               {
                   if (confirmed)
                   {
                       Messenger.Default.Send(new NotificationMessageAction<RegisterAccountViewModel>(Messages.GetRegisterAccountData,
                           (viewModel) =>
                           {
                               EMail = viewModel.EMail;
                               Password = viewModel.NewPassword;
                           }));
                   }
               }));
        }

        private bool CanValidate()
        {
            return !String.IsNullOrEmpty(EMail) && !String.IsNullOrEmpty(Password);
        }

        private bool CanRegister()
        {
            return SelectedCloudUrl.ServerName.Equals("Local cloud", StringComparison.InvariantCultureIgnoreCase);
        }

        private void ManageCloudUrls()
        {
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.ManageCloudUrls, (confirmed) => { }));
        }
    }
}
#endif