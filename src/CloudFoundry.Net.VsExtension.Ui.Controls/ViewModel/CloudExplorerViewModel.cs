using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
using GalaSoft.MvvmLight;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using System.Collections.ObjectModel;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    [ExportViewModel("CloudExplorer", true)]
    public class CloudExplorerViewModel : ViewModelBase
    {
        readonly ReadOnlyCollection<CloudTreeViewItemViewModel> clouds;

        public CloudExplorerViewModel()
        {
            var cloudList = new List<Cloud>()
            {
                new Cloud()
                {
                    ServerName = "CF Server 01"
                },
                new Cloud()
                {
                    ServerName = "CF Server 02"                
                },
                new Cloud()
                {
                    ServerName = "CF Server 03"
                }
            };
            this.clouds = new ReadOnlyCollection<CloudTreeViewItemViewModel>(
                (from cloud in cloudList
                 select new CloudTreeViewItemViewModel(cloud)).ToList());
        }

        public ReadOnlyCollection<CloudTreeViewItemViewModel> Clouds
        {
            get { return this.clouds; }
        }
    }
}
