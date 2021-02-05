using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace e610.NET
{
    public sealed partial class MainPage : Page
    {
        // App Startup + Main Page Load // 
        public MainPage()
        {
            this.InitializeComponent();
            GetSettings();
            InitializeGlobalVars();
            //NavShadow.Receivers.Add(MainContentFrame);
            MainContentFrame.Navigate(typeof(PostsViewPage)); // Navigate to posts view on load
            //Thread navThread = new Thread(PageNavigation);
            //navThread.Start();
        }
        private void GetSettings()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            GlobalVars.Username = (string)localSettings.Values["username"];
            if (GlobalVars.Username == null)
            {
                GlobalVars.Username = "";
                localSettings.Values["username"] = "";
            }
            GlobalVars.APIKey = (string)localSettings.Values["apikey"];
            if (GlobalVars.APIKey == null)
            {
                GlobalVars.APIKey = "";
                localSettings.Values["apikey"] = "";
            }
            GlobalVars.Rating = (string)localSettings.Values["rating"];
            if (GlobalVars.Rating == null)
            {
                GlobalVars.Rating = "rating:safe";
                localSettings.Values["rating"] = "rating:safe";
            }
            try
            {
                GlobalVars.ShowComments = (bool)localSettings.Values["comments"];
            }
            catch (Exception) 
            { 
                GlobalVars.ShowComments = true;
                localSettings.Values["comments"] = true;
            }
            try
            {
                GlobalVars.MuteVolume = (bool)localSettings.Values["volume"];
            }
            catch (Exception)
            {
                GlobalVars.MuteVolume = true;
                localSettings.Values["volume"] = true;
            }
        }
        private void InitializeGlobalVars()
        {
            GlobalVars.postCount = 75;
            GlobalVars.ViewModel = new PostsViewModel();
            GlobalVars.newSearch = false;
            GlobalVars.searchText = "";
            GlobalVars.pageCount = 1;
            GlobalVars.Binding = "Sample Height";
        }

        // When Page needs to navigate back // 
        private void MainNav_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            On_BackRequested();
            //if(GlobalVars.currentFrame == 0)
            //{
            //    MainContentFrame.Navigate(typeof(SinglePostView), null, new DrillInNavigationTransitionInfo());
            //    GlobalVars.currentFrame = 1;
            //}
            //else
            //{
            //    MainContentFrame.Navigate(typeof(PostsViewPage), null, new DrillInNavigationTransitionInfo());
            //    GlobalVars.currentFrame = 0;
            //}
        }
        private bool On_BackRequested()
        {
            if (MainContentFrame.CanGoBack)
            {
                MainContentFrame.GoBack();
                return true;
            }
            return false;
        }

        // NavBar Navigations Buttons // 
        private void PostsViewTapped(object sender, TappedRoutedEventArgs e)
        {
            MainContentFrame.Navigate(typeof(PostsViewPage), null, new DrillInNavigationTransitionInfo());
            GlobalVars.ViewModel.Posts.Clear();
        }
        private void PoolsViewTapped(object sender, TappedRoutedEventArgs e)
        {
            MainContentFrame.Navigate(typeof(PostsViewPage), null, new DrillInNavigationTransitionInfo());
        }
        private void AccountViewTapped(object sender, TappedRoutedEventArgs e)
        {
            MainContentFrame.Navigate(typeof(AccountsPage), null, new DrillInNavigationTransitionInfo());
        }

        private void MainNav_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                SettingsDialog.ShowAsync();
            }
        }

        private void SettingsDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            switch ((string)RatingSelection.SelectedItem)
            {
                case "rating:safe":
                    GlobalVars.Rating = "rating:safe";
                    localSettings.Values["rating"] = "rating:safe";
                    break;
                case "rating:questionable":
                    GlobalVars.Rating = "rating:questionable rating:safe";
                    localSettings.Values["rating"] = "rating:questionable";
                    break;
                case "rating:explicit":
                    GlobalVars.Rating = "";
                    localSettings.Values["rating"] = "";
                    break;
            }
            GlobalVars.ShowComments = CommentSwitch.IsOn;
            localSettings.Values["comments"] = GlobalVars.ShowComments;

        }

        private void RatingSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

    }
}
