using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Input;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    public class ProgressViewModel : DialogViewModel
    {
        private int progressValue;
        private string progressText;
        private string title;
        private bool canExecuteConfirmed;
        private bool canExecuteCancelled;

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
                    this.ErrorMessage = error.Text;
                    this.canExecuteCancelled = false;
                    this.canExecuteConfirmed = true;
                    CommandManager.InvalidateRequerySuggested();
                });
        }

        protected override void InitializeData()
        {
            Messenger.Default.Send(new NotificationMessageAction<string>(Messages.SetProgressData, (s) => this.Title = s));
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
    }
}
