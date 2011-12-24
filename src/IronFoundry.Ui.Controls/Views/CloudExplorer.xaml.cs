using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using IronFoundry.Ui.Controls.Utilities;
using IronFoundry.Ui.Controls.ViewModel.Explorer;

namespace IronFoundry.Ui.Controls.Views
{
    using Utilities;
    using ViewModel.Explorer;

    /// <summary>
    /// Interaction logic for CloudExplorer.xaml
    /// </summary>
    public partial class CloudExplorer : UserControl
    {
        public CloudExplorer()
        {
            InitializeComponent();
            this.DataContext = new CloudExplorerViewModel();
            this.Unloaded += (s,e) => Messenger.Default.Unregister(this);
            
            Messenger.Default.Register<NotificationMessageAction<bool>>(
                this,
                message =>
                {
                    if (message.Notification.Equals(Messages.AddCloud))
                    {
                        var view = new Views.AddCloud();
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
                    if (message.Notification.Equals(Messages.PushApp))
                    {
                        var view = new Views.Push();
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
                    if (message.Notification.Equals(Messages.UpdateApp))
                    {
                        var view = new Views.Update();
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
                    if (message.Notification.Equals(Messages.Progress))
                    {
                        var view = new Views.ProgressDialog();
                        Window parentWindow = Window.GetWindow(this);
                        view.Owner = parentWindow;
                        var result = view.ShowDialog();
                        message.Execute(result.GetValueOrDefault());
                    }
                });
        }
    }
}
