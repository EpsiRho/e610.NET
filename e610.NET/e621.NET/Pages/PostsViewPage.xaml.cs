using Newtonsoft.Json;
using RestSharp;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace e610.NET
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PostsViewPage : Page
    {
        // Pages Vars //
        public PostsViewModel ViewModel { get; set; }
        public int pageCount;
        private class LoadPostsArgs
        {
            public string tags;
            public int lastid;
            public char BorA;

            public LoadPostsArgs(string t, int l, char ba)
            {
                tags = t;
                lastid = l;
                BorA = ba;
            }
        }

        public PostsViewPage()
        {
            this.InitializeComponent();
            GC.Collect();
            PageLoad();
        }
        private void PageLoad()
        {
            SearchBox.Text = GlobalVars.searchText;
            PostCountSlider.Value = GlobalVars.postCount;
            pageCount = GlobalVars.pageCount;
            if (GlobalVars.newSearch == true)
            {
                ViewModel = new PostsViewModel();
                Thread LoadThread = new Thread(LoadPosts);
                LoadThread.Start(new LoadPostsArgs(SearchBox.Text, -1, ' '));
                GlobalVars.newSearch = false;
            }
            else if(GlobalVars.ViewModel.Posts.Count() > 0)
            {
                ViewModel = new PostsViewModel();
                Thread pop = new Thread(threadedRepopulate);
                pop.Start();
            }
            else
            {
                ViewModel = new PostsViewModel();
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //ViewModel.Posts.Clear();
            ViewModel = null;
            ImageGrid = null;
            this.UnloadObject(this);
            GC.Collect();
        }

        // Repopulate Posts with saved ViewModel in GlobalVars //
        public async void threadedRepopulate()
        {
            Thread.Sleep(100);
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                LoadingBar.IsIndeterminate = false;
                LoadingBar.Maximum = GlobalVars.ViewModel.Posts.Count;
                LoadingBar.Value = 0;
                LoadingBar.Visibility = Visibility.Visible;
            });

            foreach (Post p in GlobalVars.ViewModel.Posts)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ViewModel.AddPost(p);
                    LoadingBar.Value++;
                });
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                LoadingBar.Visibility = Visibility.Collapsed;
                GlobalVars.ViewModel.Posts.Clear();
            });
            GC.Collect();
        }

        // Load New Posts //
        public async void LoadPosts(object t)
        {
            try
            {
                // Function Vars
                var client = new RestClient(); // Client to handle Requests
                double limit = 75; // Post Limit
                LoadPostsArgs args = (LoadPostsArgs)t; // Convert Object to LoadPostArgs class
                var request = new RestRequest(RestSharp.Method.GET); // REST request

                // Show Progress Bar + Get post limit from slider
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => 
                {
                    LoadingBar.IsIndeterminate = true;
                    LoadingBar.Visibility = Visibility.Visible;
                    limit = PostCountSlider.Value;
                });

                // Set Endpoint
                // TODO: Switching between e621 - gelbooru - r34 - etc
                client.BaseUrl = new Uri("https://e621.net/posts.json?");

                // Set the useragent for e621
                client.UserAgent = "e610.NET/1.0(by EpsilonRho)";

                // If user is logged in set login parameters into request
                if(GlobalVars.Username != "" && GlobalVars.APIKey != "")
                {
                    request.AddQueryParameter("login", GlobalVars.Username);
                    request.AddQueryParameter("api_key", GlobalVars.APIKey);
                }

                // Set parameters for tags and post limit
                request.AddQueryParameter("tags", args.tags);
                request.AddQueryParameter("limit", limit.ToString());

                // If the lastid is not -1, use the last post id to move forward or back a page
                if (args.lastid != -1)
                {
                    request.AddQueryParameter("page", args.BorA + args.lastid.ToString());
                }

                // Send the request
                var response = client.Execute(request);

                // Deserialize the response
                Root DeserializedJson = JsonConvert.DeserializeObject<Root>(response.Content);

                // Set the progress bar to determinate to show % of posts loaded into the PostsView
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    LoadingBar.IsIndeterminate = false;
                    LoadingBar.Maximum = DeserializedJson.posts.Count;
                    LoadingBar.Value = 0;
                });

                // Get the posts from the Deserialized Class
                foreach (Post p in DeserializedJson.posts)
                {
                    // If the url is null the post is blacklisted
                    if (p.preview.url != null)
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            // Choose which thumbnail based on UI toggle
                            // main difference is Sample gifs are animated and pictures are slightly higher quality(i think)
                            if (thumbnailSource.IsOn)
                            {
                                p.ThumbURL = p.sample.url;
                            }
                            else
                            {
                                p.ThumbURL = p.preview.url;
                            }
                            ViewModel.AddPost(p);
                            LoadingBar.Value++;
                        });
                    }
                    else
                    {
                        // this post was blacklsited so show funny picture
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => 
                        {
                            p.ThumbURL = "https://ambisure.com/wp-content/uploads/2019/03/SHOCK1-1030x724.png";
                            p.file.url = "https://ambisure.com/wp-content/uploads/2019/03/SHOCK1-1030x724.png";
                            ViewModel.AddPost(p);
                            LoadingBar.Value++;
                        });
                    }
                    DeserializedJson = null;
                }

                // Loading done, hide the progress bar
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                {
                    LoadingBar.Visibility = Visibility.Collapsed;
                });
                GC.Collect();
            }
            catch (Exception)
            {

            }
        }

        // Button Functions //
        private void SearchButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.ClearPosts();
            Thread LoadThread = new Thread(LoadPosts);
            LoadThread.Start(new LoadPostsArgs(SearchBox.Text, -1, 'b'));
            pageCount = 0;
            GlobalVars.postCount = (int)PostCountSlider.Value;
        }
        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ViewModel.ClearPosts();
                Thread LoadThread = new Thread(LoadPosts);
                LoadThread.Start(new LoadPostsArgs(SearchBox.Text, -1, 'b'));
                pageCount = 0;
                GlobalVars.postCount = (int)PostCountSlider.Value;
            }
        }
        private void thumbnailSource_Toggled(object sender, RoutedEventArgs e)
        {
            ViewModel.changeThumbnail(thumbnailSource.IsOn);
            Bindings.Update();
        }
        private void ImageGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                Post pick = (Post)e.ClickedItem;
                if (pick.file.url != "https://ambisure.com/wp-content/uploads/2019/03/SHOCK1-1030x724.png")
                {
                    GlobalVars.ViewModel = ViewModel;
                    GlobalVars.pageCount = pageCount;
                    GlobalVars.searchText = SearchBox.Text;
                    GlobalVars.postCount = (int)PostCountSlider.Value;
                    GlobalVars.nvPost = null;
                    GlobalVars.nvPost = pick;
                    //GlobalVars.requestNavigate = true;
                    this.Frame.Navigate(typeof(SinglePostView), null, new DrillInNavigationTransitionInfo());
                }
            }
            catch (Exception)
            {

            }
        }
        private void ForwardPage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ViewModel.Posts.Count > 0)
            {
                Thread LoadThread = new Thread(LoadPosts);
                Post p = ViewModel.Posts.Last();
                ViewModel.ClearPosts();
                LoadThread.Start(new LoadPostsArgs(SearchBox.Text, p.id, 'b'));
                pageCount++;
                Bindings.Update();
            }
        }
        private void BackPage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (pageCount != 0)
            {
                Thread LoadThread = new Thread(LoadPosts);
                Post p = ViewModel.Posts.First();
                ViewModel.ClearPosts();
                LoadThread.Start(new LoadPostsArgs(SearchBox.Text, p.id, 'a'));
                pageCount--;
                Bindings.Update();
            }
        }
        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {

        }

    }
}
