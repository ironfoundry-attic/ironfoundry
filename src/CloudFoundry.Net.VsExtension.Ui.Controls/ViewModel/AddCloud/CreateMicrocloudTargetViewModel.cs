using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using GalaSoft.MvvmLight.Messaging;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using System.Text.RegularExpressions;
using CloudFoundry.Net.Types;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class CreateMicrocloudTargetViewModel : ViewModelBase
    {
        public RelayCommand ConfirmedCommand { get; private set; }
        public RelayCommand CancelledCommand { get; private set; }
        private CloudUrl cloudUrl;
        private string replacementText;
        private string name;

        public CreateMicrocloudTargetViewModel()
        {
            ConfirmedCommand = new RelayCommand(Confirmed);
            CancelledCommand = new RelayCommand(Cancelled);

            InitializeData();            
            RegisterGetData();
        }

        private void Confirmed()
        {
            Messenger.Default.Send(new NotificationMessage<bool>(this, true, Messages.CreateMicrocloudTargetDialogResult));
        }

        private void Cancelled()
        {
            Messenger.Default.Send(new NotificationMessage<bool>(this, false, Messages.CreateMicrocloudTargetDialogResult));
            Messenger.Default.Unregister(this);
        }

        private void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<CreateMicrocloudTargetViewModel>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.GetMicrocloudTargetData))
                    {
                        this.cloudUrl.ServerType = this.name;
                        var toReplace = "{" + Regex.Match(this.cloudUrl.Url, @"\{(\w+)\}").Groups[1].Value + "}";
                        this.cloudUrl.Url = this.cloudUrl.Url.Replace(toReplace,this.replacementText);
                        message.Execute(this);
                    }
                    Messenger.Default.Unregister(this);
                });
        }

        private void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<CloudUrl>(Messages.SetAddCloudUrlData,
                (cloudUrl) =>
                {
                    this.cloudUrl = cloudUrl;
                    this.replacementText = Regex.Match(this.cloudUrl.Url, @"\{(\w+)\}").Groups[1].Value;
                    this.name = string.Format("Microcloud ({0})", this.replacementText);
                }));
        }

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                RaisePropertyChanged("Name");
            }
        }

        public string ReplacementText
        {
            get { return this.replacementText; }
            set
            {
                this.replacementText = value;
                RaisePropertyChanged("ReplacementText");
                this.Name = string.Format("Microcloud ({0})", this.replacementText);
            }
        }

        public CloudUrl CloudUrl
        {
            get { return this.cloudUrl; }
        }

    }
}



