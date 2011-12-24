namespace IronFoundry.Ui.Controls.ViewModel
{
    using System.ComponentModel;
    using System.Windows.Input;
    using GalaSoft.MvvmLight.Messaging;
    using Mvvm;
    using Utilities;

    public class ProgressViewModel : DialogViewModel
    {
        private bool canExecuteCancelled;
        private bool canExecuteConfirmed;
        private bool isCancelVisible = true;
        private string progressText;
        private int progressValue;
        private string title;

        public ProgressViewModel() : base(Messages.ProgressDialogResult)
        {
            canExecuteConfirmed = false;
            canExecuteCancelled = true;
            Messenger.Default.Register<ProgressMessage>(this,
                                                        message =>
                                                        {
                                                            ProgressValue = message.Value;
                                                            ProgressText = message.Text;
                                                            if (ProgressValue == 100)
                                                            {
                                                                canExecuteCancelled = false;
                                                                canExecuteConfirmed = true;
                                                            }
                                                            CommandManager.InvalidateRequerySuggested();
                                                        });

            Messenger.Default.Register<ProgressError>(this,
                                                      error =>
                                                      {
                                                          ProgressText = error.Text;
                                                          ProgressValue = 100;
                                                          ErrorMessage = error.Text;
                                                          canExecuteCancelled = false;
                                                          canExecuteConfirmed = true;
                                                          CommandManager.InvalidateRequerySuggested();
                                                      });
        }

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                RaisePropertyChanged("Title");
            }
        }

        public int ProgressValue
        {
            get { return progressValue; }
            set
            {
                progressValue = value;
                RaisePropertyChanged("ProgressValue");
            }
        }

        public string ProgressText
        {
            get { return progressText; }
            set
            {
                progressText = value;
                RaisePropertyChanged("ProgressText");
            }
        }

        public bool IsCancelVisible
        {
            get { return isCancelVisible; }
            set
            {
                isCancelVisible = value;
                RaisePropertyChanged("IsCancelVisible");
            }
        }

        protected override void OnConfirmed(CancelEventArgs args)
        {
            Cleanup();
        }

        protected override void OnCancelled()
        {
            Cleanup();
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<string>(Messages.SetProgressData, (s) => Title = s));
            Messenger.Default.Send(new NotificationMessageAction<bool>(Messages.SetProgressCancelButtonVisible,
                                                                       (b) => IsCancelVisible = b));
        }

        protected override bool CanExecuteConfirmed()
        {
            return canExecuteConfirmed;
        }

        protected override bool CanExecuteCancelled()
        {
            return canExecuteCancelled;
        }
    }
}