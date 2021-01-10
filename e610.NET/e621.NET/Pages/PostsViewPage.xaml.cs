using e610.NET;
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
        public bool canGetTags;

        public PostsViewPage()
        {
            this.InitializeComponent();
            GC.Collect();
        }
        private void PageLoad()
        {
            SearchBox.Text = GlobalVars.searchText;
            PostCountSlider.Value = GlobalVars.postCount;
            pageCount = GlobalVars.pageCount;
            canGetTags = true;

            SidePanelShadow.Receivers.Add(MainPanel);

            if (GlobalVars.newSearch == true)
            {
                ViewModel = new PostsViewModel();
                Thread LoadThread = new Thread(LoadPosts);
                LoadThread.Start(new LoadPostsArgs(SearchBox.Text, -1));
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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PageLoad();
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
                try
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        ViewModel.AddPost(p);
                        LoadingBar.Value++;
                    });
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                }
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
                client.UserAgent = "e610.NET/1.3(by EpsilonRho)";

                // If user is logged in set login parameters into request
                if(GlobalVars.Username != "" && GlobalVars.APIKey != "")
                {
                    request.AddQueryParameter("login", GlobalVars.Username);
                    request.AddQueryParameter("api_key", GlobalVars.APIKey);
                }

                request.AddQueryParameter("tags", GlobalVars.Rating + " " + args.tags);

                // Set parameters for tags and post limit
                request.AddQueryParameter("limit", limit.ToString());


                if (args.page != -1)
                {
                    request.AddQueryParameter("page", args.page.ToString());
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

        private async void getTags(object t)
        {
            // Function Vars
            var client = new RestClient(); // Client to handle Requests
            string args = (string)t; // Convert Object to LoadPostArgs class
            var request = new RestRequest(RestSharp.Method.GET); // REST request

            if(args[0] == '-')
            {
                args.Remove(0, 1);
            }

            // Set Endpoint
            // TODO: Switching between e621 - gelbooru - r34 - etc
            client.BaseUrl = new Uri("https://e621.net//tags.json?");

            // Set the useragent for e621
            client.UserAgent = "e610.NET/1.3(by EpsilonRho)";
            request.AddQueryParameter("search[name_matches]", args + "*");
            request.AddQueryParameter("search[order]", "count");
            //request.AddQueryParameter("search[hide_empty]", "true");

            // Set parameters for tags and post limit
            request.AddQueryParameter("limit", "8");

            // Send the request
            var response = client.Execute(request);
            // Deserialize the response
            TagsHolder DeserializedJson = null;
            try
            {
                DeserializedJson = JsonConvert.DeserializeObject<TagsHolder>("{tags:" + response.Content + "}");
            }
            catch (Exception)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    SearchTagAutoComplete.Items.Clear();
                });
                canGetTags = true;
                return;
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                SearchTagAutoComplete.Items.Clear();
            });

            foreach (Tag tag in DeserializedJson.tags)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    SearchTagAutoComplete.Items.Add(tag.name);
                });
            }

            canGetTags = true;
        }

        // Button Functions //
        private void SearchButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.ClearPosts();
            Thread LoadThread = new Thread(LoadPosts);
            LoadThread.Start(new LoadPostsArgs(SearchBox.Text, -1));
            pageCount = 1;
            Bindings.Update();
            GlobalVars.postCount = (int)PostCountSlider.Value;
        }
        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SearchTagAutoComplete.Items.Clear();
                ViewModel.ClearPosts();
                Thread LoadThread = new Thread(LoadPosts);
                LoadThread.Start(new LoadPostsArgs(SearchBox.Text, -1));
                pageCount = 1;
                Bindings.Update();
                GlobalVars.postCount = (int)PostCountSlider.Value;
            }
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
                     this.Frame.Navigate(typeof(SinglePostView), pick, new DrillInNavigationTransitionInfo());
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
                //Post p = ViewModel.Posts.Last();
                ViewModel.ClearPosts();
                pageCount++;
                LoadThread.Start(new LoadPostsArgs(SearchBox.Text, pageCount));
                Bindings.Update();
            }
        }
        private void BackPage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (pageCount != 1)
            {
                Thread LoadThread = new Thread(LoadPosts);
                //Post p = ViewModel.Posts.First();
                ViewModel.ClearPosts();
                pageCount--;
                LoadThread.Start(new LoadPostsArgs(SearchBox.Text, pageCount));
                Bindings.Update();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string[] tags = SearchBox.Text.Split(" ");
            int index = 0;
            int count = 0;
            int pos = SearchBox.SelectionStart;
            for (int i = 0; i < tags.Count(); i++)
            {
                count += tags[i].Count();
                if (pos == count)
                {
                    index = i;
                    break;
                }
                count++;
            }
            if (tags[index].Count() >= 3)
            {
                if (canGetTags)
                {
                    Thread TagsThread = new Thread(getTags);
                    TagsThread.Start(tags[index]);
                    canGetTags = false;
                }
            }
            else
            {
                SearchTagAutoComplete.Items.Clear();
            }
        }

        private void SearchTagAutoComplete_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                canGetTags = false;
                string clickedTag = (string)e.ClickedItem;
                string[] tags = SearchBox.Text.Split(" ");
                int pos = SearchBox.SelectionStart;
                SearchBox.Text = "";
                int count = 0;
                for (int i = 0; i < tags.Count(); i++)
                {
                    count += tags[i].Count();
                    if (pos == count)
                    {
                        pos = (pos - tags[i].Count()) + clickedTag.Count() + 1;
                        if (tags[i][0] == '-')
                        {
                            tags[i] = "-" + clickedTag;
                        }
                        else
                        {
                            tags[i] = clickedTag;
                        }
                        SearchBox.Focus(FocusState.Programmatic);
                    }
                    SearchBox.Text += tags[i] + " ";
                    count++;
                }
                SearchTagAutoComplete.Items.Clear();
                SearchBox.SelectionStart = pos;
                canGetTags = true;
            }
            catch (Exception)
            {

            }
        }
    }
}
