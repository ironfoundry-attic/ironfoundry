/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:CloudFoundry.Net.VsExtension.Ui.Controls"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        private static FoundryPropertiesViewModel foundryProperties;
        private static ChangePasswordViewModel changePassword;

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ////if (ViewModelBase.IsInDesignModeStatic)            

            foundryProperties = new FoundryPropertiesViewModel();
            changePassword = new ChangePasswordViewModel();
        }

        /// <summary>
        /// Gets the Main property which defines the main viewmodel.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1822:MarkMembersAsStatic",Justification = "This non-static member is needed for data binding purposes.")]
        public FoundryPropertiesViewModel FoundryProperties
        {
            get
            {
                return foundryProperties;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
        public ChangePasswordViewModel ChangePassword
        {
            get
            {
                return changePassword;
            }
        }
    }
}