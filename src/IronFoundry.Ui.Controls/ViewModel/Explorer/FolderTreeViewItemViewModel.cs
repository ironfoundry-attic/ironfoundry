namespace IronFoundry.Ui.Controls.ViewModel.Explorer
{
    using System;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using GalaSoft.MvvmLight.Messaging;
    using Model;
    using Mvvm;
    using Utilities;
    using Vcap;
    using Application = Types.Application;

    public class FolderTreeViewItemViewModel : TreeViewItemViewModel
    {
        private readonly Application app;
        private readonly ushort id;
        private readonly string name;
        private readonly string path;
        private ICloudFoundryProvider provider;

        public FolderTreeViewItemViewModel(string name, string path, Application app, ushort id)
            : base(null, true)
        {
            Messenger.Default.Send(new NotificationMessageAction<ICloudFoundryProvider>(
                                       Messages.GetCloudFoundryProvider, p => provider = p));
            this.name = name;
            this.app = app;
            this.path = path;
            this.id = id;
        }

        public string Name
        {
            get { return name; }
        }

        public ImageSource Icon
        {
            get
            {
                Bitmap hBitmap = IconUtil.IconFromExtension("Directory", IconUtil.SystemIconSize.Small).ToBitmap();
                return Imaging.CreateBitmapSourceFromHBitmap(hBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                                                             BitmapSizeOptions.FromEmptyOptions());
            }
        }

        public override void LoadChildren()
        {
            Children.Clear();
            ProviderResponse<VcapFilesResult> result = provider.GetFiles(app.Parent, app, path, id);
            if (result.Response == null)
            {
                Messenger.Default.Send(new NotificationMessage<string>(result.Message, Messages.ErrorMessage));
                return;
            }

            foreach (VcapFilesResult.FilesResultData dir in result.Response.Directories)
                base.Children.Add(new FolderTreeViewItemViewModel(dir.Name, path + "/" + dir.Name, app, id));
            foreach (VcapFilesResult.FilesResultData file in result.Response.Files)
                base.Children.Add(new FileTreeViewItemViewModel(file.Name, path + "/" + file.Name, app, id));
        }
    }
}