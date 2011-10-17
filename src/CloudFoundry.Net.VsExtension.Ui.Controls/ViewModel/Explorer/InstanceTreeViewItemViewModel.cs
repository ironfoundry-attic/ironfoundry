namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    using Types;
    using CloudFoundry.Net.Vmc;

    public class InstanceTreeViewItemViewModel : TreeViewItemViewModel
    {
        private StatInfo statInfo;
        private Application app;        

        public InstanceTreeViewItemViewModel(StatInfo statInfo, ApplicationTreeViewItemViewModel parentApplication) : base(parentApplication, true)
        {
            this.app = parentApplication.Application;
            this.statInfo = statInfo;         
        }

        public string Host
        {
            get
            {
                string host = null;
                if (null != statInfo.Stats) // TODO probably not a null check we want to do
                {
                    host = statInfo.Stats.Host;
                }
                return host;
            }
        }

        public override void LoadChildren()
        {
            Children.Clear();

            IVcapClient client = new VcapClient(app.Parent);
            var clientResult = client.Files(app.Name,"/",(ushort)statInfo.ID);
            foreach (var dir in clientResult.Directories)
                base.Children.Add(new FolderTreeViewItemViewModel(dir.Name, dir.Name, app, (ushort)statInfo.ID));
            foreach (var file in clientResult.Files)
                base.Children.Add(new FileTreeViewItemViewModel(file.Name, file.Name, app, (ushort)statInfo.ID));
        }
    }
}