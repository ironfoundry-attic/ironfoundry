namespace IronFoundry.Ui.Controls.Views
{
    using System.Windows;
    using GalaSoft.MvvmLight.Messaging;
    using Utilities;
    using ViewModel.Push;

    /// <summary>
    /// Interaction logic for FoundryProperties.xaml
    /// </summary>
    public partial class Push : Window
    {
        public Push()
        {
            InitializeComponent();
            this.DataContext = new PushViewModel();
            this.Closed += (s, e) => Messenger.Default.Unregister(this);

            Messenger.Default.Register<NotificationMessageAction<bool>>(
                this,
                message =>
                {
                    if (message.Notification.Equals(Messages.ManageClouds))
                    {
                        var view = new Views.Explorer();
                        Window parentWindow = Window.GetWindow(this);
                        view.Owner = parentWindow;
                        var result = view.ShowDialog();
                        message.Execute(result.GetValueOrDefault());
                    }
                });

            Messenger.Default.Register<NotificationMessageAction<bool>>(
                this,
                message =>
                {
                    if (message.Notification.Equals(Messages.AddApplicationService))
                    {
                        var view = new Views.AddApplicationService();
                        Window parentWindow = Window.GetWindow(this);
                        view.Owner = parentWindow;
                        var result = view.ShowDialog();
                        message.Execute(result.GetValueOrDefault());
                    }
                });

            Messenger.Default.Register<NotificationMessage<bool>>(this,
                message =>
                {
                    if (message.Notification.Equals(Messages.PushDialogResult))
                    {
                        this.DialogResult = message.Content;
                        this.Close();
                        Messenger.Default.Unregister(this);
                    }
                });

            Messenger.Default.Register<NotificationMessageAction<string>>(
                this,
                message =>
                {
                    if (message.Notification.Equals(Messages.ChooseDirectory))
                    {
                        var dialog = new System.Windows.Forms.FolderBrowserDialog
                                         {Description = "Choose a directory with a pre-compiled ASP.NET application."};
                        var result = dialog.ShowDialog();
                        if (result == System.Windows.Forms.DialogResult.OK)
                            message.Execute(dialog.SelectedPath);
                        else
                            message.Execute(null);
                    }
                });
        }
    }
}