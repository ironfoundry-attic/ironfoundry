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
    [ExportViewModel("ManageApplicationUrls", false)]
    public class ManageApplicationUrlsViewModel : ViewModelBase
    {
        public RelayCommand ConfirmedCommand { get; private set; }
        public RelayCommand CancelledCommand { get; private set; }
        public RelayCommand AddCommand { get; private set; }
        public RelayCommand EditCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }
        private ObservableCollection<string> urls;
        private string selectedUrl;

        public ManageApplicationUrlsViewModel()
        {
            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit, CanEdit);
            RemoveCommand = new RelayCommand(Remove, CanRemove);
            ConfirmedCommand = new RelayCommand(Confirmed);
            CancelledCommand = new RelayCommand(Cancelled);

            Messenger.Default.Send(new NotificationMessageAction<ObservableCollection<string>>(Messages.SetManageApplicationUrlsData,
                (urls) =>
                {
                    this.Urls = new ObservableCollection<string>(from url in urls select url);
                }));

            Messenger.Default.Register<NotificationMessageAction<ManageApplicationUrlsViewModel>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.GetManageApplicationUrlsData))
                        message.Execute(this);
                    Messenger.Default.Unregister(this);
                });
        }

        public string SelectedUrl
        {
            get { return this.selectedUrl; }
            set
            {
                this.selectedUrl = value;
                RaisePropertyChanged("SelectedUrl");
            }
        }

        public ObservableCollection<string> Urls
        {
            get { return this.urls; }
            set
            {
                this.urls = value;
                RaisePropertyChanged("Urls");
            }
        }

        private void Confirmed()
        {
            Messenger.Default.Send(new NotificationMessage<bool>(this, true, Messages.ManageApplicationUrlsDialogResult));
        }

        private void Cancelled()
        {
            Messenger.Default.Send(new NotificationMessage<bool>(this, false, Messages.ManageApplicationUrlsDialogResult));
        }

        private void Add()
        {
            EditUrl(true);
        }

        private bool CanEdit()
        {
            return (!String.IsNullOrEmpty(this.SelectedUrl));
        }

        private bool CanRemove()
        {
            return (!String.IsNullOrEmpty(this.SelectedUrl));
        }

        private void Edit()
        {
            EditUrl(false);
        }

        private void EditUrl(bool isNew)
        {
            if (!isNew)
            {
                Messenger.Default.Register<NotificationMessageAction<string>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.SetAddApplicationUrlData))
                        message.Execute(this.SelectedUrl);
                });
            }

            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.AddApplicationUrl,
                (confirmed) =>
                {
                    if (confirmed)
                    {
                        Messenger.Default.Send(new NotificationMessageAction<AddApplicationUrlViewModel>(Messages.GetAddApplicationUrlData,
                        (viewModel) =>
                        {
                            if (!isNew)
                                this.Urls.Remove(this.SelectedUrl);
                            var url = viewModel.Url;
                            this.Urls.Add(url);
                            this.SelectedUrl = url;
                        }));
                    }
                }));
        }

        private void Remove()
        {
            if (this.SelectedUrl != null)
            {
                this.Urls.Remove(this.SelectedUrl);
            }
        }

    }
}


