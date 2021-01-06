using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace e610.NET
{    
    
    public sealed partial class SinglePostView : Page
    {
        // Pages Variables //
        private Post singlePost; // Holds the post shown on this page
        private ObservableCollection<TreeItem> DataSource = new ObservableCollection<TreeItem>(); // Tags tree binding source
        private ObservableCollection<Pool> ConnectedPools = new ObservableCollection<Pool>(); // Pools list binding source

        // Page Load Functions //
        public SinglePostView()
        {
            this.InitializeComponent();
            GC.Collect();
            PageLoad();
        }
        private void PageLoad()
        {
            // Grab the post clicked on in the PostsView
            singlePost = GlobalVars.nvPost;

            // Update the searchbox with the searchbox from the PostsView
            SearchBox.Text = GlobalVars.searchText; 

            // Update the vote up / down with the post score
            VoteUpCount.Text = singlePost.score.up.ToString();
            VoteDownCount.Text = singlePost.score.down.ToString();

            // If there are any pools with this post in
            if (singlePost.pools.Count > 0)
            {
                // Make the button visible
                PoolBar.Visibility = Visibility.Visible;

                // Added the pools to the listview
                Thread poolThread = new Thread(new ThreadStart(poolPopulate));
                poolThread.Start();
                poolThread = null;
            }

            // Check if the post needs to use the mediaplayer or just an image
            if (singlePost.file.ext == "webm")
            {
                Uri pathUri = new Uri(singlePost.file.url);
                bigvideo.Source = MediaSource.CreateFromUri(pathUri);
                bigvideo.Visibility = Visibility.Visible;
                // Start a thread to load and populate the tags list
                if (DataSource.Count() == 0)
                {
                    Thread TagsThread = new Thread(new ThreadStart(PopulateTreeView));
                    TagsThread.Start();
                    TagsThread = null;
                }
            }
            else
            {
                bigpicture.Visibility = Visibility.Visible;
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.UnloadObject(this);
            DataSource.Clear();
            DataSource = null;
            TagsView.ItemsSource = null;
            TagsView = null;
            bigpicture.Source = null;
            bigpicture = null;
            bigvideo.Source = null;
            bigvideo = null;
            GlobalVars.nvPost = null;
            GC.Collect();
        }
        private async void poolPopulate()
        {
            List<int> pools = singlePost.pools;
            foreach (int p in pools)
            {
                Pool newpool = getPoolInfo(p);
                if (newpool != null)
                {
                    newpool.name = newpool.name.Replace("_", " ");
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        ConnectedPools.Add(newpool);
                    });
                }
            }
        }
        private Pool getPoolInfo(int poolID)
        {
            try
            {
                var client = new RestClient();
                client.BaseUrl = new Uri("https://e621.net/pools.json?");
                client.UserAgent = "e610.NET/0.1(by EpsilonRho)";
                var request = new RestRequest(RestSharp.Method.GET);
                if (GlobalVars.Username != "" && GlobalVars.APIKey != "")
                {
                    request.AddQueryParameter("login", GlobalVars.Username);
                    request.AddQueryParameter("api_key", GlobalVars.APIKey);
                }
                request.AddQueryParameter("search[id]", poolID.ToString());
                IRestResponse response = client.Execute(request);
                string edited = response.Content.Remove(0, 1);
                edited = edited.Remove(edited.Count() - 1, 1);
                Pool DeserializedJson = JsonConvert.DeserializeObject<Pool>(edited);
                response = null;
                edited = null;
                return DeserializedJson;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private async void PopulateTreeView()
        {
            Thread.Sleep(500);
            TreeItem Artists = new TreeItem("Artists");
            foreach (string str in singlePost.tags.artist)
            {
                Artists.Children.Add(new TreeItem(str));
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                DataSource.Add(Artists);
            });
            Artists = null;
            TreeItem Copyright = new TreeItem("Copyright");
            foreach (string str in singlePost.tags.copyright)
            {
                Copyright.Children.Add(new TreeItem(str));
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                DataSource.Add(Copyright);
            });
            Copyright = null;
            TreeItem Character = new TreeItem("Character");
            foreach (string str in singlePost.tags.character)
            {
                Character.Children.Add(new TreeItem(str));
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                DataSource.Add(Character);
            });
            Character = null;
            TreeItem Species = new TreeItem("Species");
            foreach (string str in singlePost.tags.species)
            {
                Species.Children.Add(new TreeItem(str));
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                DataSource.Add(Species);
            });
            Species = null;
            TreeItem General = new TreeItem("General");
            foreach (string str in singlePost.tags.general)
            {
                General.Children.Add(new TreeItem(str));
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                DataSource.Add(General);
            });
            General = null;
            TreeItem Meta = new TreeItem("Meta");
            foreach (string str in singlePost.tags.meta)
            {
                Meta.Children.Add(new TreeItem(str));
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                DataSource.Add(Meta);
            });
            Meta = null;
            TreeItem Lore = new TreeItem("Lore");
            foreach (string str in singlePost.tags.lore)
            {
                Lore.Children.Add(new TreeItem(str));
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                DataSource.Add(Lore);
            });
            Lore = null;
            TreeItem Invalid = new TreeItem("Invalid");
            foreach (string str in singlePost.tags.invalid)
            {
                Invalid.Children.Add(new TreeItem(str));
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                DataSource.Add(Invalid);
            });
            Invalid = null;
            GC.Collect();
        }

        // Button Functions //
        private void SearchButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GlobalVars.newSearch = true;
            GlobalVars.searchText = SearchBox.Text;
            this.Frame.Navigate(typeof(PostsViewPage), null, new DrillInNavigationTransitionInfo());
        }
        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                GlobalVars.newSearch = true;
                GlobalVars.searchText = SearchBox.Text;
                this.Frame.Navigate(typeof(PostsViewPage), null, new DrillInNavigationTransitionInfo());
            }
        }
        private void PoolBar_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (PoolsPopout.IsOpen)
            {
                PoolsPopout.IsOpen = false;
            }
            else
            {
                PoolsPopout.IsOpen = true;
            }
        }
        private void TagsView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            TreeItem clicked = (TreeItem)args.InvokedItem;
            if(clicked.Children.Count == 0)
            {
                if (SearchBox.Text.Count() == SearchBox.Text.LastIndexOf(' '))
                {
                    SearchBox.Text.Remove(SearchBox.Text.LastIndexOf(' '));
                }

                SearchBox.Text += " " + clicked.Name;
            }

        }
        private void TagsView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            try
            {
                TextBlock ClickedItem = (e.OriginalSource as TextBlock);
                //TagsView.SelectedNodes.Add(ClickedItem);
                SearchBox.Text = SearchBox.Text.Replace(ClickedItem.Text, "");
            }
            catch(Exception){

            }
        }
        private void VoteUpButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var client = new RestClient();
            client.BaseUrl = new Uri("https://e621.net/posts/"+ singlePost.id + "/votes.json");
            client.UserAgent = "e610.NET/0.1(by EpsilonRho)";
            var request = new RestRequest(RestSharp.Method.POST);
            if (GlobalVars.Username != "" && GlobalVars.APIKey != "")
            {
                request.AddQueryParameter("login", GlobalVars.Username);
                request.AddQueryParameter("api_key", GlobalVars.APIKey);
            }
            request.AddQueryParameter("score", "1");
            var response = client.Execute(request);
            VoteResponse DeserializedJson = JsonConvert.DeserializeObject<VoteResponse>(response.Content);
            if (DeserializedJson.our_score == 1)
            {
                singlePost.score.up++;
                VoteUpCount.Text = singlePost.score.up.ToString();
                VoteUpButton.Background = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
            }
            else
            {
                singlePost.score.up++;
                VoteUpCount.Text = singlePost.score.up.ToString();
                VoteUpButton.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            }
        }
        private void VoteDownButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var client = new RestClient();
            client.BaseUrl = new Uri("https://e621.net/posts/" + singlePost.id + "/votes.json?");
            client.UserAgent = "e610.NET/0.1(by EpsilonRho)";
            var request = new RestRequest(RestSharp.Method.GET);
            if (GlobalVars.Username != "" && GlobalVars.APIKey != "")
            {
                request.AddQueryParameter("login", GlobalVars.Username);
                request.AddQueryParameter("api_key", GlobalVars.APIKey);
            }
            request.AddQueryParameter("score", "-1");
            var response = client.Execute(request);
            VoteResponse DeserializedJson = JsonConvert.DeserializeObject<VoteResponse>(response.Content);
            if (DeserializedJson.our_score == 1)
            {
                singlePost.score.down++;
                VoteDownCount.Text = singlePost.score.down.ToString();
                VoteDownButton.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            }
            else
            {
                singlePost.score.down++;
                VoteDownCount.Text = singlePost.score.down.ToString();
                VoteDownButton.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            }
        }
        private void FavoiteButton_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }


        private void bigpicture_ImageOpened(object sender, RoutedEventArgs e)
        {
            // Start a thread to load and populate the tags list
            if (DataSource.Count() == 0)
            {
                Thread TagsThread = new Thread(new ThreadStart(PopulateTreeView));
                TagsThread.Start();
                TagsThread = null;
            }
        }
    }
}
