using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace e610.NET
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccountsPage : Page
    {
        // Page Load Functions //
        public AccountsPage()
        {
            this.InitializeComponent();
            GetSettings();
        }
        private void GetSettings()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            GlobalVars.Username = (string)localSettings.Values["username"];
            if (GlobalVars.Username != null)
            {
                UsernameBox.Text = GlobalVars.Username;
            }
            GlobalVars.APIKey = (string)localSettings.Values["apikey"];
            if (GlobalVars.APIKey != null)
            {
                APIKeyBox.Text = GlobalVars.APIKey;
            }
        }

        // Button Functions //
        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["username"] = UsernameBox.Text;
            localSettings.Values["apikey"] = APIKeyBox.Text;
            GlobalVars.Username = UsernameBox.Text;
            GlobalVars.APIKey = APIKeyBox.Text;
        }
    }
}
