using System;
using System.Collections.ObjectModel;
using System.Linq;
using CloudFoundry.Net.Types;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class AddCloudViewModel : DialogViewModel
    {
        public RelayCommand ManageCloudUrlsCommand { get; private set; }
        public RelayCommand ValidateAccountCommand { get; private set; }
        public RelayCommand RegisterAccountCommand { get; private set; }

        public Cloud Cloud { get; private set; }  
        private ObservableCollection<CloudUrl> cloudUrls;
        private CloudUrl selectedCloudUrl;
        private bool isAccountValid;
        

        public AddCloudViewModel() : base(Messages.AddCloudDialogResult)
        {
            Cloud = new Types.Cloud();
            ValidateAccountCommand = new RelayCommand(ValidateAccount, CanValidate);
            RegisterAccountCommand = new RelayCommand(RegisterAccount, CanRegister);
            ManageCloudUrlsCommand = new RelayCommand(ManageCloudUrls);
            this.cloudUrls = provider.CloudUrls;
            this.SelectedCloudUrl = cloudUrls.SingleOrDefault((i) => i.IsDefault);
            OnConfirmed += (s, e) => { this.provider.Clouds.Add(this.Cloud); Cleanup(); };
        }

        private void ValidateAccount()
        {
            this.ErrorMessage = string.Empty;
            this.IsAccountValid = false;
            var result = this.provider.ValidateAccount(this.Cloud);
            if (result.Response)
                this.IsAccountValid = true;
            else
                this.ErrorMessage = result.Message;
        }

        private void RegisterAccount()
        {
            IsAccountValid = false;
            Messenger.Default.Register<NotificationMessageAction<Cloud>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.SetRegisterAccountData))
                        message.Execute(this.Cloud);
                });

            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.RegisterAccount,
                (confirmed) =>
                {
                    if (confirmed)
                    {

                        Messenger.Default.Send(new NotificationMessageAction<RegisterAccountViewModel>(Messages.GetRegisterAccountData,
                            (viewModel) =>
                            {
                                this.EMail = viewModel.EMail;
                                this.Password = viewModel.NewPassword;
                            }));
                    }
                }));
        }

        private bool CanValidate()
        {
            return !String.IsNullOrEmpty(this.EMail) && !String.IsNullOrEmpty(this.Password);
        }

        private bool CanRegister()
        {
            return SelectedCloudUrl.ServerType.Equals("Local cloud", StringComparison.InvariantCultureIgnoreCase);
        }

        private void ManageCloudUrls()
        {            
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.ManageCloudUrls, (confirmed) => {}));
        }

        public CloudUrl SelectedCloudUrl
        {
            get { return this.selectedCloudUrl; }
            set
            {
                IsAccountValid = false;
                this.selectedCloudUrl = value;
                if (this.selectedCloudUrl != null)
                    this.Cloud.Url = selectedCloudUrl.Url;
                else
                    this.Cloud.Url = string.Empty;
                RaisePropertyChanged("SelectedCloudUrl");
            }
        }

        public bool IsAccountValid
        {
            get { return this.isAccountValid; }
            set { this.isAccountValid = value; RaisePropertyChanged("IsAccountValid"); }
        }

        public ObservableCollection<CloudUrl> CloudUrls
        {
            get { return this.cloudUrls; }
            set { this.cloudUrls = value; RaisePropertyChanged("CloudUrls"); }
        }

        public string ServerName
        {
            get { return this.Cloud.ServerName; }
            set { this.Cloud.ServerName = value; RaisePropertyChanged("ServerName"); }
        }

        public string EMail
        {
            get { return this.Cloud.Email; }
            set { this.Cloud.Email = value; RaisePropertyChanged("EMail"); }
        }

        public string Password
        {
            get { return this.Cloud.Password; }
            set { this.Cloud.Password = value; RaisePropertyChanged("Password"); }
        }
    }
}
