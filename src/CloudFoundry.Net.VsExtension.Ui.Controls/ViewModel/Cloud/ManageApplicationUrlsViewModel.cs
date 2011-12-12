namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using CloudFoundry.Net.Extensions;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;

    public class ManageApplicationUrlsViewModel : DialogViewModel
    {
        public RelayCommand AddCommand { get; private set; }
        public RelayCommand EditCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }

        private ObservableCollection<string> urls;
        private string selectedUrl;

        public ManageApplicationUrlsViewModel() : base(Messages.ManageApplicationUrlsDialogResult)
        {
            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit, CanEdit);
            RemoveCommand = new RelayCommand(Remove, CanRemove);
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<ObservableCollection<string>>(Messages.SetManageApplicationUrlsData, urls => this.Urls = urls.DeepCopy()));
        }

        protected override void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<ManageApplicationUrlsViewModel>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.GetManageApplicationUrlsData))
                        message.Execute(this);
                    Cleanup();
                });
        }

        public string SelectedUrl
        {
            get { return this.selectedUrl; }
            set { this.selectedUrl = value; RaisePropertyChanged("SelectedUrl"); }
        }

        public ObservableCollection<string> Urls
        {
            get { return this.urls; }
            set { this.urls = value; RaisePropertyChanged("Urls"); }
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
                this.Urls.Remove(this.SelectedUrl);
        }

    }
}