namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    using Types;
    using System.Windows.Media;
    using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;
    using System.Windows.Interop;
    using System;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;

    public class FileTreeViewItemViewModel : TreeViewItemViewModel
    {
        private string name;
        private string fileExtension;

        public FileTreeViewItemViewModel(string name)
            : base(null, true)
        {
            this.name = name;
            this.fileExtension = System.IO.Path.GetExtension(name);
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
    }
}