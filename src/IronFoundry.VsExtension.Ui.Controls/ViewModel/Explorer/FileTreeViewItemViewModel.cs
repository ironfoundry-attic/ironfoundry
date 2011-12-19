namespace IronFoundry.VsExtension.Ui.Controls.ViewModel
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using IronFoundry.VsExtension.Ui.Controls.Model;
    using IronFoundry.VsExtension.Ui.Controls.Utilities;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;

    public class FileTreeViewItemViewModel : TreeViewItemViewModel
    {
        private string name;
        private IronFoundry.Types.Application app;
        private string path;
        private ushort id;
        private string fileExtension;
        public RelayCommand<MouseButtonEventArgs> OpenFileCommand { get; private set; }
        public RelayCommand OpenFileFromContextCommand { get; private set; } 
        private ICloudFoundryProvider provider;

        public FileTreeViewItemViewModel(string name, string path, IronFoundry.Types.Application app, ushort id)
            : base(null, true)
        {
            Messenger.Default.Send<NotificationMessageAction<ICloudFoundryProvider>>(new NotificationMessageAction<ICloudFoundryProvider>(Messages.GetCloudFoundryProvider, p => this.provider = p));
            this.name = name;
            this.app = app;
            this.path = path;
            this.id = id;
            this.fileExtension = System.IO.Path.GetExtension(name);
            OpenFileCommand = new RelayCommand<MouseButtonEventArgs>(OpenFile);
            OpenFileFromContextCommand = new RelayCommand(OpenFileFromContext);
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }       

        public ImageSource Icon
        {
            get
            {
                var hBitmap = IconUtil.IconFromExtension(fileExtension, IconUtil.SystemIconSize.Small).ToBitmap();
                return Imaging.CreateBitmapSourceFromHBitmap(hBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
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
            var result = provider.GetFiles(app.Parent, app, "/" + path, id);
            if (result.Response == null)
            {
                Messenger.Default.Send(new NotificationMessage<string>(result.Message, Messages.ErrorMessage));
                return;
            }
            var pathToFile = System.IO.Path.GetTempPath() + name;
            using (var fs = File.Create(pathToFile))
                using (var bw = new BinaryWriter(fs))
                    bw.Write(result.Response.File);

            Process.Start(pathToFile);
        }
    }
}