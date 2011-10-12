using System.Windows;
using System.Windows.Controls;
using CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Explorer : Window
    {
        public Explorer()
        {
            InitializeComponent();
            this.DataContext = new ExplorerViewModel();
        }
    }
}
