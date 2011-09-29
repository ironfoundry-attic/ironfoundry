using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.ObjectModel;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    [ExportViewModel("ManageCloudUrls", false)]
    public class ManageCloudUrlsViewModel : ViewModelBase
    {
        public RelayCommand ConfirmedCommand { get; private set; }
        public RelayCommand CancelledCommand { get; private set; }
        public RelayCommand AddCommand { get; private set; }
        public RelayCommand EditCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }
        private ObservableCollection<CloudUrl> cloudUrls;
        private CloudUrl selectedCloudUrl;

        public ManageCloudUrlsViewModel()
        {
            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit, CanEdit);
            RemoveCommand = new RelayCommand(Remove, CanRemove);
            ConfirmedCommand = new RelayCommand(Confirmed);
            CancelledCommand = new RelayCommand(Cancelled);

            Messenger.Default.Send(new NotificationMessageAction<ObservableCollection<CloudUrl>>(Messages.SetManageCloudUrlsData,
                (message) =>
                {
                    this.CloudUrls = message;
                }));

            Messenger.Default.Register<NotificationMessageAction<ManageCloudUrlsViewModel>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.GetManageCloudUrlsData))
                        message.Execute(this);
                    Messenger.Default.Unregister(this);
                });
        }

        public CloudUrl SelectedCloudUrl
        {
            get { return this.selectedCloudUrl; }
            set
            {
                this.selectedCloudUrl = value;
                RaisePropertyChanged("SelectedCloudUrl");
            }
        }

        public ObservableCollection<CloudUrl> CloudUrls
        {
            get { return this.cloudUrls; }
            set
            {
                this.cloudUrls = value;
                RaisePropertyChanged("CloudUrls");
            }
        }

        private void Confirmed()
        {
            Messenger.Default.Send(new NotificationMessage<bool>(this, true, Messages.ManageCloudUrlsDialogResult));
        }

        private void Cancelled()
        {
            Messenger.Default.Send(new NotificationMessage<bool>(this, false, Messages.ManageCloudUrlsDialogResult));
        }

        private void Add()
        {
            EditUrl(true);
        }

        private bool CanEdit()
        {
            return (this.SelectedCloudUrl != null && this.SelectedCloudUrl.IsConfigurable);
        }

        private bool CanRemove()
        {
            return (this.SelectedCloudUrl != null && this.SelectedCloudUrl.IsRemovable);
        }

        private void Edit()
        {
            EditUrl(false);
        }

        private void EditUrl(bool isNew)
        {
            if (!isNew && this.SelectedCloudUrl.IsMicroCloud)
                PrepareMicroCloud();
            else
                AddCloudUrl(isNew);
            
        }

        private void AddCloudUrl(bool isNew)
        {
            if (!isNew)
            {
                Messenger.Default.Register<NotificationMessageAction<CloudUrl>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.SetAddCloudUrlData))
                        message.Execute(this.SelectedCloudUrl);
                });
            }

            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.AddCloudUrl,
                (confirmed) =>
                {
                    if (confirmed)
                    {
                        Messenger.Default.Send(new NotificationMessageAction<AddCloudUrlViewModel>(Messages.GetAddCloudUrlData,
                        (viewModel) =>
                        {
                            if (!isNew)
                                this.CloudUrls.Remove(this.SelectedCloudUrl);
                            var newCloudUrl = new CloudUrl() { ServerType = viewModel.Name, Url = viewModel.Url, IsConfigurable = true, IsRemovable = true };
                            this.CloudUrls.Add(newCloudUrl);
                            this.SelectedCloudUrl = newCloudUrl;
                        }));
                    }
                }));
        }

        private void PrepareMicroCloud()
        {
            Messenger.Default.Register<NotificationMessageAction<CloudUrl>>(this,
            message =>
            {
                if (message.Notification.Equals(Messages.SetAddCloudUrlData))
                {
                    var newCloudUrl = new CloudUrl()
                    {
                        ServerType = this.SelectedCloudUrl.ServerType,
                        Url = this.SelectedCloudUrl.Url,
                        IsConfigurable = true,
                        IsRemovable = true
                    };
                    message.Execute(newCloudUrl);
                }
            });

            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.CreateMicrocloudTarget,
            (confirmed) =>
            {
                if (confirmed)
                {
                    Messenger.Default.Send(new NotificationMessageAction<CreateMicrocloudTargetViewModel>(Messages.GetMicrocloudTargetData,
                        (viewModel) =>
                        {
                            this.CloudUrls.Add(viewModel.CloudUrl);
                            this.SelectedCloudUrl = viewModel.CloudUrl;
                        }));
                }
            }));
        }        

        private void Remove()
        {
            if (this.SelectedCloudUrl != null)
            {
                this.CloudUrls.Remove(this.SelectedCloudUrl);
            }
        }

    }
}


