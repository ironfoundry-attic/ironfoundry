namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    using System.Windows.Input;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
    using GalaSoft.MvvmLight.Messaging;

    public class ProgressViewModel : DialogViewModel
    {
        private int progressValue;
        private string progressText;
        private string title;
        private bool canExecuteConfirmed;
        private bool canExecuteCancelled;
        private bool isCancelVisible = true;

        public ProgressViewModel() : base(Messages.ProgressDialogResult)
        {
            this.canExecuteConfirmed = false;
            this.canExecuteCancelled = true;
            Messenger.Default.Register<ProgressMessage>(this,
                message =>
                {
                    this.ProgressValue = message.Value;
                    this.ProgressText = message.Text;
                    if (this.ProgressValue == 100)
                    {
                        this.canExecuteCancelled = false;
                        this.canExecuteConfirmed = true;                        
                    }
                    CommandManager.InvalidateRequerySuggested();
                });

            Messenger.Default.Register<ProgressError>(this,
                error =>
                    {
                        this.ProgressText = error.Text;
                        this.ProgressValue = 100;
                        this.ErrorMessage = error.Text;
                        this.canExecuteCancelled = false;
                        this.canExecuteConfirmed = true;
                        CommandManager.InvalidateRequerySuggested();
                });

            OnConfirmed += (s, e) => Cleanup();
            OnCancelled += (s, e) => Cleanup();
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<string>(Messages.SetProgressData, (s) => this.Title = s));
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.SetProgressCancelButtonVisible, (b) => this.IsCancelVisible = b));
        }

        protected override bool CanExecuteConfirmed()
        {
            return this.canExecuteConfirmed;
        }

        protected override bool CanExecuteCancelled()
        {
            return this.canExecuteCancelled;
        }

        public string Title
        {
            get { return this.title; }
            set { this.title = value; RaisePropertyChanged("Title"); }
        }

        public int ProgressValue
        {
            get { return this.progressValue; }
            set { this.progressValue = value; RaisePropertyChanged("ProgressValue"); }
        }

        public string ProgressText
        {
            get { return this.progressText; }
            set { this.progressText = value; RaisePropertyChanged("ProgressText"); }
        }

        public bool IsCancelVisible
        {
            get { return this.isCancelVisible; }
            set { this.isCancelVisible = value; RaisePropertyChanged("IsCancelVisible"); }
        }

    }
}