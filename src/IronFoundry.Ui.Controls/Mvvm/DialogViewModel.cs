namespace IronFoundry.Ui.Controls.Mvvm
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using Model;
    using Utilities;

    public abstract class DialogViewModel : ViewModelBase
    {
        public RelayCommand ConfirmedCommand { get; private set; }
        public RelayCommand CancelledCommand { get; private set; }
        private string resultMessageId;
        protected ICloudFoundryProvider provider;
        private string errorMessage;

        protected virtual void OnConfirmed(CancelEventArgs args) { }
        protected virtual void OnCancelled() { }

        public DialogViewModel(string resultMessageId)
        {
            Messenger.Default.Send<NotificationMessageAction<ICloudFoundryProvider>>(new NotificationMessageAction<ICloudFoundryProvider>(Messages.GetCloudFoundryProvider, p => this.provider = p));
            this.resultMessageId = resultMessageId;
            this.ConfirmedCommand = new RelayCommand(Confirmed, CanExecuteConfirmed);
            this.CancelledCommand = new RelayCommand(Cancelled, CanExecuteCancelled);

            InitializeData();
            RegisterGetData();
        }

        protected virtual void InitializeData() { }
        protected virtual void RegisterGetData() { }

        private void Confirmed()
        {
            var args = new CancelEventArgs();    
            OnConfirmed(args);
            if (false == args.Cancel)
            {
                Messenger.Default.Send(new NotificationMessage<bool>(this, true, resultMessageId));
            }
        }

        protected virtual bool CanExecuteConfirmed()
        {
            return true;
        }

        private void Cancelled()
        {
            OnCancelled();
            Messenger.Default.Send(new NotificationMessage<bool>(this, false, resultMessageId));
            Cleanup();
        }

        protected virtual bool CanExecuteCancelled()
        {
            return true;
        }

        public string ErrorMessage
        {
            get { return this.errorMessage; }
            set
            {
                this.errorMessage = value; RaisePropertyChanged("ErrorMessage");
                if (!String.IsNullOrWhiteSpace(this.errorMessage))
                {
                    var worker = new BackgroundWorker();
                    worker.DoWork += (s, e) => Thread.Sleep(TimeSpan.FromSeconds(7));
                    worker.RunWorkerCompleted += (s, e) => this.ErrorMessage = string.Empty;
                    worker.RunWorkerAsync();
                }
            }
        }
    }
}