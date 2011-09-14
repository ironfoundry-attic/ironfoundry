using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ExportViewModel : ExportAttribute
    {
        public string Name { get; private set; }

        public ExportViewModel(string name, bool isStatic)
            : base("ViewModel")
        {
            Name = name;
        }
    }

    public interface IViewModelMetadata
    {
        string Name { get; }
    }
}
