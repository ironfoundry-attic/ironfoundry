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
using CloudFoundry.Net.Types;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class AddCloudUrlViewModel : DialogViewModel
    {
        private CloudUrl cloudUrl = new CloudUrl();

        public AddCloudUrlViewModel()
            : base(Messages.AddCloudUrlDialogResult)
        {
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<CloudUrl>(Messages.SetAddCloudUrlData,
                (cloudUrl) =>
                {
                    this.cloudUrl = cloudUrl;
                }));
        }

        protected override void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<AddCloudUrlViewModel>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.GetAddCloudUrlData))
                        message.Execute(this);
                    Cleanup();
                });
        }

        public string Name
        {
            get { return this.cloudUrl.ServerType; }
            set { this.cloudUrl.ServerType = value; RaisePropertyChanged("Name"); }
        }

        public string Url
        {
            get { return this.cloudUrl.Url; }
            set { this.cloudUrl.Url = value; RaisePropertyChanged("Url"); }
        }
    }
}
