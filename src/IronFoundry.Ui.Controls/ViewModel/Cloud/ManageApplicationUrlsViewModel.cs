namespace IronFoundry.Ui.Controls.ViewModel.Cloud
{
    using System;
    using Extensions;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using Mvvm;
    using Utilities;

    public class ManageApplicationUrlsViewModel : DialogViewModel
    {
        private string selectedUrl;
        private SafeObservableCollection<string> urls;

        public ManageApplicationUrlsViewModel() : base(Messages.ManageApplicationUrlsDialogResult)
        {
            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit, CanEdit);
            RemoveCommand = new RelayCommand(Remove, CanRemove);
        }

        public RelayCommand AddCommand { get; private set; }
        public RelayCommand EditCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }

        public string SelectedUrl
        {
            get { return selectedUrl; }
            set
            {
                selectedUrl = value;
                RaisePropertyChanged("SelectedUrl");
            }
        }

        public SafeObservableCollection<string> Urls
        {
            get { return urls; }
            set
            {
                urls = value;
                RaisePropertyChanged("Urls");
            }
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(
                new NotificationMessageAction<SafeObservableCollection<string>>(Messages.SetManageApplicationUrlsData,
                                                                                urls => Urls = urls.DeepCopy()));
        }

        protected override void RegisterGetData()
        {
            Messenger.Default.Register<NotificationMessageAction<ManageApplicationUrlsViewModel>>(this,
                                                                                                  message =>
                                                                                                  {
                                                                                                      if (
                                                                                                          message.
                                                                                                              Notification
                                                                                                              .Equals(
                                                                                                                  Messages
                                                                                                                      .
                                                                                                                      GetManageApplicationUrlsData))
                                                                                                          message.
                                                                                                              Execute(
                                                                                                                  this);
                                                                                                      Cleanup();
                                                                                                  });
        }

        private void Add()
        {
            EditUrl(true);
        }

        private bool CanEdit()
        {
            return (!String.IsNullOrEmpty(SelectedUrl));
        }

        private bool CanRemove()
        {
            return (!String.IsNullOrEmpty(SelectedUrl));
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
                                                                                  if (
                                                                                      message.Notification.Equals(
                                                                                          Messages.
                                                                                              SetAddApplicationUrlData))
                                                                                      message.Execute(SelectedUrl);
                                                                              });
            }

            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.AddApplicationUrl,
                                                                       (confirmed) =>
                                                                       {
                                                                           if (confirmed)
                                                                           {
                                                                               Messenger.Default.Send(
                                                                                   new NotificationMessageAction
                                                                                       <AddApplicationUrlViewModel>(
                                                                                       Messages.GetAddApplicationUrlData,
                                                                                       (viewModel) =>
                                                                                       {
                                                                                           if (!isNew)
                                                                                               Urls.Remove(SelectedUrl);
                                                                                           string url = viewModel.Url;
                                                                                           Urls.Add(url);
                                                                                           SelectedUrl = url;
                                                                                       }));
                                                                           }
                                                                       }));
        }

        private void Remove()
        {
            if (SelectedUrl != null)
                Urls.Remove(SelectedUrl);
        }
    }
}