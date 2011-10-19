namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    using Types;
    using System.Windows.Media;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
    using System.Windows.Interop;
    using System;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using CloudFoundry.Net.Vmc;
    using GalaSoft.MvvmLight.Messaging;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Model;

    public class FolderTreeViewItemViewModel : TreeViewItemViewModel
    {
        private ICloudFoundryProvider provider;
        private string name;
        private CloudFoundry.Net.Types.Application app;
        private string path;
        private ushort id;

        public FolderTreeViewItemViewModel(string name, string path, CloudFoundry.Net.Types.Application app, ushort id)
            : base(null, true)
        {
            Messenger.Default.Send<NotificationMessageAction<ICloudFoundryProvider>>(new NotificationMessageAction<ICloudFoundryProvider>(Messages.GetCloudFoundryProvider, p => this.provider = p));
            this.name = name;
            this.app = app;
            this.path = path;
            this.id = id;
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public override void LoadChildren()
        {
            Children.Clear();
            var result = provider.GetFiles(app.Parent, app, path, id);
            if (result.Response == null)
            {
                Messenger.Default.Send(new NotificationMessage<string>(result.Message, Messages.ErrorMessage));
                return;
            }

            foreach (var dir in result.Response.Directories)
                base.Children.Add(new FolderTreeViewItemViewModel(dir.Name, path + "/" + dir.Name, app, id));
            foreach (var file in result.Response.Files)
                base.Children.Add(new FileTreeViewItemViewModel(file.Name, path + "/" + file.Name, app, id));
        }

        public ImageSource Icon
        {
            get
            {
                var hBitmap = IconUtil.IconFromExtension("Directory", IconUtil.SystemIconSize.Small).ToBitmap();
                return Imaging.CreateBitmapSourceFromHBitmap(hBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }
    }
}