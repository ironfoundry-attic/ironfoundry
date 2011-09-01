using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;


namespace CloudFoundry.Net.Dea.Service
{
    [RunInstaller(true)]
    public partial class DeaWindowsServiceInstaller : System.Configuration.Install.Installer
    {
        public DeaWindowsServiceInstaller()
        {
            InitializeComponent();
        }
    }
}
