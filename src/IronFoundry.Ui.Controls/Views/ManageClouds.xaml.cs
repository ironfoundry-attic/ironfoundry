namespace IronFoundry.Ui.Controls.Views
{
    using System.Windows;
    using IronFoundry.Ui.Controls.ViewModel;

    public partial class ManageClouds : Window
    {
        public ManageClouds()
        {
            InitializeComponent();
            this.DataContext = new ManageCloudsViewModel();
        }
    }
}