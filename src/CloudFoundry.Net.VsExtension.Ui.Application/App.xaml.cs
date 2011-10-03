using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.ObjectModel;
using CloudFoundry.Net.Types;
using System.IO.IsolatedStorage;
using CloudFoundry.Net.VsExtension.Ui.Controls.Model;
using CloudFoundry.Net.VsExtension.Ui.Controls.Utilities;

namespace CloudFoundry.Net.VsExtension.Ui.Application
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private PreferencesProvider preferencesProvider;

        public App()
        {
            preferencesProvider = new PreferencesProvider("CloudFoundryExplorerApp");            
        }        
    }
}
