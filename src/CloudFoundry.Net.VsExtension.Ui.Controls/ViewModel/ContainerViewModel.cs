using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm;
using System.Collections.ObjectModel;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    [ExportViewModel("Container", true)]
    public class ContainerViewModel : ViewModelBase
    {
        static Random rnd = new Random();
        public ContainerViewModel()
        {
            var sampleData = GetSampleData();
            this.Clouds = new ObservableCollection<CloudViewModel>(
                (from cloud in sampleData
                 select new CloudViewModel(cloud)));
            this.CloudExplorer = new CloudExplorerViewModel(sampleData);
        }

        private ObservableCollection<CloudViewModel> clouds;

        public ObservableCollection<CloudViewModel> Clouds
        {
            get { return this.clouds; }
            set
            {
                this.clouds = value;
                RaisePropertyChanged("Clouds"); 
            }
        }

        private CloudExplorerViewModel cloudExplorer;

        public CloudExplorerViewModel CloudExplorer
        {
            get { return this.cloudExplorer; }
            set
            {
                this.cloudExplorer = value;
                RaisePropertyChanged("CloudExplorer");
            }
        }

        private ObservableCollection<Cloud> GetSampleData()
        {
            return new ObservableCollection<Cloud>()
            {
                GetSampleCloud(),
                GetSampleCloud(),
                GetSampleCloud(),
                GetSampleCloud()
            };
        }

        private Cloud GetSampleCloud()
        {
            return new Cloud()
            {
                ServerName = "CF Server " + Guid.NewGuid().ToString("D"),
                Applications = new List<Application>()
                    {
                        new Application()
                        {
                            Name = "App " + Guid.NewGuid().ToString("D"),
                            Instances = new List<Instance>()
                            {
                                new Instance() {
                                    Host = GetRandomIP()
                                },
                                new Instance() {
                                    Host = GetRandomIP()
                                },
                                new Instance() {
                                    Host = GetRandomIP()
                                }
                            }
                        },
                        new Application()
                        {
                            Name = "App " + Guid.NewGuid().ToString("D"),
                            Instances = new List<Instance>()
                            {
                                new Instance() {
                                    Host = GetRandomIP()
                                },
                                new Instance() {
                                    Host = GetRandomIP()
                                },
                                new Instance() {
                                    Host = GetRandomIP()
                                }
                            }
                        }
                        ,
                        new Application()
                        {
                            Name = "App " + Guid.NewGuid().ToString("D"),
                            Instances = new List<Instance>()
                            {
                                new Instance() {
                                    Host = GetRandomIP()
                                },
                                new Instance() {
                                    Host = GetRandomIP()
                                },
                                new Instance() {
                                    Host = GetRandomIP()
                                }
                            }
                        }
                    }
            };
        }

        private string GetRandomIP()
        {
            return string.Format("{0}.{1}.{2}.{3}",
                          Convert.ToInt32(rnd.NextDouble() * 255.0),
                          Convert.ToInt32(rnd.NextDouble() * 255.0),
                          Convert.ToInt32(rnd.NextDouble() * 255.0),
                          Convert.ToInt32(rnd.NextDouble() * 255.0)
                          );
        }
    }
}
