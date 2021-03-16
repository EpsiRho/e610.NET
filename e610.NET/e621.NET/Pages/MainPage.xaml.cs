using Microsoft.UI.Xaml.Controls;
using System;
using System.Drawing;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace e610.NET
{
    public sealed partial class MainPage : Page
    {
        public static TabView MainTabViewAccess;

        // App Startup + Main Page Load // 
        public MainPage()
        {
            this.InitializeComponent();
            GetSettings();

            MainTabViewAccess = MainTabView;

            InitializeGlobalVars();

            var newTab = new Microsoft.UI.Xaml.Controls.TabViewItem();
            //newTab.IconSource = new SymbolIconSource() { Symbol = Symbol.Document };
            newTab.Header = "Latest Posts";

            // The Content of a TabViewItem is often a frame which hosts a page.
            Frame frame = new Frame();
            newTab.Content = frame;

            frame.Navigate(typeof(PostsViewPage), this);

            MainTabView.TabItems.Add(newTab);

            MainTabView.SelectedItem = newTab;
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


        private void RatingSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TabView_AddTabButtonClick(object sender, object args)
        {
            var newTab = new Microsoft.UI.Xaml.Controls.TabViewItem();
            //newTab.IconSource = new SymbolIconSource() { Symbol = Symbol.Document };
            newTab.Header = "Latest Posts";

            // The Content of a TabViewItem is often a frame which hosts a page.
            Frame frame = new Frame();
            newTab.Content = frame;

            GlobalVars.searchText = "";

            frame.Navigate(typeof(PostsViewPage));

            MainTabView.TabItems.Add(newTab);

            MainTabView.SelectedItem = newTab;
        }

        private void Tabs_TabCloseRequested(object sender, TabViewTabCloseRequestedEventArgs args)
        {
            if(MainTabView.TabItems.IndexOf(args.Tab) < MainTabView.SelectedIndex)
            {
                MainTabView.TabItems.Remove(args.Tab);
                MainTabView.SelectedIndex--;
                return;
            }
            MainTabView.TabItems.Remove(args.Tab);
        }
    }
}
