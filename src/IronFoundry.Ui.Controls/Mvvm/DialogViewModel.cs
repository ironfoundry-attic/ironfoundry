namespace IronFoundry.Ui.Controls.Mvvm
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using IronFoundry.Ui.Controls.ViewModel;
    using Model;
    using Utilities;

    public abstract class DialogViewModel : ViewModelBaseEx
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
            Messenger.Default.Send<NotificationMessageAction<ICloudFoundryProvider>>(
                new NotificationMessageAction<ICloudFoundryProvider>(
                    Messages.GetCloudFoundryProvider, p =>
                    {
                        this.provider = p;
                        OnProviderRetrieved();
                    })
                );

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

        protected virtual void OnProviderRetrieved() { }

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
                if (errorMessage != value)
                {
                    this.errorMessage = value;
                    RaisePropertyChanged("ErrorMessage");
                    if (false == this.errorMessage.IsNullOrWhiteSpace())
                    {
                        var worker = new BackgroundWorker();
                        worker.DoWork += worker_DoWork;
                        worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                        worker.RunWorkerAsync();
                    }
                }
            }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            worker.DoWork -= worker_DoWork;
            Thread.Sleep(TimeSpan.FromSeconds(7));
            e.Result = true;
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            worker.RunWorkerCompleted -= worker_RunWorkerCompleted;
            this.ErrorMessage = String.Empty;
        }
    }
}