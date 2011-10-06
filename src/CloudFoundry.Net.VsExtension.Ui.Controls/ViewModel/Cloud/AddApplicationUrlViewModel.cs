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

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    [ExportViewModel("AddApplicationUrl", false)]
    public class AddApplicationUrlViewModel : ViewModelBase
    {
        public RelayCommand ConfirmedCommand { get; private set; }
        public RelayCommand CancelledCommand { get; private set; }
        string url;

        public AddApplicationUrlViewModel()
        {
            ConfirmedCommand = new RelayCommand(Confirmed);
            CancelledCommand = new RelayCommand(Cancelled);

            InitializeData();
            RegisterGetData();
        }

        private void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<ProvisionedServiceViewModel>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.GetAddApplicationUrlData))
                        message.Execute(this);
                    Messenger.Default.Unregister(this);
                });
        }

        private void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<string>(Messages.SetAddApplicationUrlData,
                (url) =>
                {
                    this.url = url;
                }));
        }

        private void Confirmed()
        {           
            Messenger.Default.Send(new NotificationMessage<bool>(this, true, Messages.AddApplicationUrlDialogResult));
        }

        private void Cancelled()
        {
            Messenger.Default.Send(new NotificationMessage<bool>(this, false, Messages.AddApplicationUrlDialogResult));
        }

        public string Url
        {
            get { return this.url; }
            set
            {
                this.url = value;
                RaisePropertyChanged("Url");
            }
        }
    }
}
