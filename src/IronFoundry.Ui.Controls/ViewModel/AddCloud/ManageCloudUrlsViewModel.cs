#if TODO
namespace IronFoundry.Ui.Controls.ViewModel.AddCloud
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using Mvvm;
    using Types;
    using Utilities;

    public class ManageCloudUrlsViewModel : DialogViewModel
    {
        private SafeObservableCollection<CloudUrl> cloudUrls;
        private CloudUrl selectedCloudUrl;

        public ManageCloudUrlsViewModel()
            : base(Messages.ManageCloudUrlsDialogResult)
        {
            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit, CanEdit);
            RemoveCommand = new RelayCommand(Remove, CanRemove);
            CloudUrls = provider.CloudUrls.DeepCopy();
        }

        public RelayCommand AddCommand { get; private set; }
        public RelayCommand EditCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }

        public CloudUrl SelectedCloudUrl
        {
            get { return selectedCloudUrl; }
            set
            {
                selectedCloudUrl = value;
                RaisePropertyChanged("SelectedCloudUrl");
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

        protected override void OnConfirmed(CancelEventArgs args)
        {
            provider.CloudUrls.Synchronize(CloudUrls.DeepCopy(), new CloudUrlEqualityComparer());
            provider.SaveChanges();
        }

        private void Add()
        {
            EditUrl(true);
        }

        private bool CanEdit()
        {
            return (SelectedCloudUrl != null && SelectedCloudUrl.IsConfigurable);
        }

        private bool CanRemove()
        {
            return (SelectedCloudUrl != null && SelectedCloudUrl.IsRemovable);
        }

        private void Edit()
        {
            EditUrl(false);
        }

        private void EditUrl(bool isNew)
        {
            if (!isNew && SelectedCloudUrl.IsMicroCloud)
            {
                PrepareMicroCloud();
            }
            else
            {
                AddCloudUrl(isNew);
            }
        }

        private void AddCloudUrl(bool isNew)
        {
            if (!isNew)
            {
                Messenger.Default.Register<NotificationMessageAction<CloudUrl>>(this,
                    message =>
                    {
                        if (message.Notification.Equals(Messages.SetAddCloudUrlData))
                        {
                            message.Execute(SelectedCloudUrl);
                        }
                    });
            }

            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.AddCloudUrl,
               (confirmed) =>
               {
                   if (confirmed)
                   {
                       Messenger.Default.Send(new NotificationMessageAction<AddCloudUrlViewModel>(Messages.GetAddCloudUrlData, (viewModel) =>
                           {
                               if (!isNew)
                               {
                                   CloudUrls.Remove(SelectedCloudUrl);
                               }
                               var newCloudUrl = new CloudUrl
                                   {
                                       ServerName     = viewModel.Name,
                                       Url            = viewModel.Url,
                                       IsConfigurable = true,
                                       IsRemovable    = true
                                   };
                               CloudUrls.Add(newCloudUrl);
                               SelectedCloudUrl = newCloudUrl;
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
                        var newCloudUrl = new CloudUrl
                        {
                            ServerName     = SelectedCloudUrl.ServerName,
                            Url            = SelectedCloudUrl.Url,
                            IsConfigurable = true,
                            IsRemovable    = true
                        };
                        message.Execute(newCloudUrl);
                    }
                });

            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.CreateMicrocloudTarget,
               (confirmed) =>
               {
                   if (confirmed)
                   {
                       Messenger.Default.Send(new NotificationMessageAction<CreateMicrocloudTargetViewModel>(
                           Messages.GetMicrocloudTargetData,
                            (viewModel) =>
                            {
                                CloudUrls.Add(viewModel.CloudUrl);
                                SelectedCloudUrl = viewModel.CloudUrl;
                            }));
                   }
               }));
        }

        private void Remove()
        {
            if (SelectedCloudUrl != null)
            {
                CloudUrls.Remove(SelectedCloudUrl);
            }
        }
    }
}
#endif