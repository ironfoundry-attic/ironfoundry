namespace IronFoundry.Ui.Controls.ViewModel.Explorer
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;
    using Model;
    using Mvvm;
    using Utilities;
    using Vcap;
    using Application = Types.Application;

    public class FileTreeViewItemViewModel : TreeViewItemViewModel
    {
        private readonly Application app;
        private readonly string fileExtension;
        private readonly ushort id;
        private readonly string name;
        private readonly string path;
        private ICloudFoundryProvider provider;

        public FileTreeViewItemViewModel(string name, string path, Application app, ushort id)
            : base(null, true)
        {
            Messenger.Default.Send(new NotificationMessageAction<ICloudFoundryProvider>(
                                       Messages.GetCloudFoundryProvider, p => provider = p));
            this.name = name;
            this.app = app;
            this.path = path;
            this.id = id;
            fileExtension = Path.GetExtension(name);
            OpenFileCommand = new RelayCommand<MouseButtonEventArgs>(OpenFile);
            OpenFileFromContextCommand = new RelayCommand(OpenFileFromContext);
        }

        public RelayCommand<MouseButtonEventArgs> OpenFileCommand { get; private set; }
        public RelayCommand OpenFileFromContextCommand { get; private set; }

        public string Name
        {
            get { return name; }
        }

        public ImageSource Icon
        {
            get
            {
                Bitmap hBitmap = IconUtil.IconFromExtension(fileExtension, IconUtil.SystemIconSize.Small).ToBitmap();
                return Imaging.CreateBitmapSourceFromHBitmap(hBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                                                             BitmapSizeOptions.FromEmptyOptions());
            }
        }

        private void OpenFile(MouseButtonEventArgs e)
        {
            if (e == null || e.ClickCount >= 2)
            {
                OpenFile();
            }
        }

        private void OpenFileFromContext()
        {
            OpenFile();
        }

        private void OpenFile()
        {
            ProviderResponse<VcapFilesResult> result = provider.GetFiles(app.Parent, app, "/" + path, id);
            if (result.Response == null)
            {
                Messenger.Default.Send(new NotificationMessage<string>(result.Message, Messages.ErrorMessage));
                return;
            }
            string pathToFile = Path.GetTempPath() + name;
            using (FileStream fs = File.Create(pathToFile))
            using (var bw = new BinaryWriter(fs))
                bw.Write(result.Response.File);

            Process.Start(pathToFile);
        }
    }
}
