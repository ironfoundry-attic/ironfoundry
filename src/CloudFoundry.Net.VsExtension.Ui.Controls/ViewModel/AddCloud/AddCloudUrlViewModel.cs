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
    [ExportViewModel("AddCloudUrl", false)]
    public class AddCloudUrlViewModel : ViewModelBase
    {
        public RelayCommand ConfirmedCommand { get; private set; }
        public RelayCommand CancelledCommand { get; private set; }
        private CloudUrl cloudUrl = new CloudUrl();

        public AddCloudUrlViewModel()
        {
            ConfirmedCommand = new RelayCommand(Confirmed);
            CancelledCommand = new RelayCommand(Cancelled);

            InitializeData();
            RegisterGetData();
        }

        private void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<AddCloudUrlViewModel>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.GetAddCloudUrlData))
                        message.Execute(this);
                    Messenger.Default.Unregister(this);
                });
        }

        private void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<CloudUrl>(Messages.SetAddCloudUrlData,
                (cloudUrl) =>
                {
                    this.cloudUrl = cloudUrl;
                }));
        }

        private void Confirmed()
        {           
            Messenger.Default.Send(new NotificationMessage<bool>(this, true, Messages.AddCloudUrlDialogResult));
        }

        private void Cancelled()
        {
            Messenger.Default.Send(new NotificationMessage<bool>(this, false, Messages.AddCloudUrlDialogResult));
            Messenger.Default.Unregister(this);
        }

        public string Name
        {
            get { return this.cloudUrl.ServerType; }
            set
            {
                this.cloudUrl.ServerType = value;
                RaisePropertyChanged("Name");
            }
        }

        public string Url
        {
            get { return this.cloudUrl.Url; }
            set
            {
                this.cloudUrl.Url = value;
                RaisePropertyChanged("Url");
            }
        }
    }
}
