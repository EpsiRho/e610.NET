using e610.NET;
using e610.NET.Pages;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
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
        private ObservableCollection<Comment> CommentsSource = new ObservableCollection<Comment>(); // Tags tree binding source
        private ObservableCollection<Pool> ConnectedPools = new ObservableCollection<Pool>(); // Pools list binding source
        private ObservableCollection<Pool> MovementSource = new ObservableCollection<Pool>(); // Movement list binding source
        public int pageCount;
        public bool canGetTags;
        private Post singlePost;
        private Pool singlePool;
        private bool _isPostSwiped;
        private bool _isGridSwiped;
        private string ImageSizeString;
        public bool stopDownload;
        private bool IsPopupOpen;
        public bool loadingPosts;
        private string ClickedTag;

        public PostsViewPage()
        {
            this.InitializeComponent();
        }
        private void PageLoad()
        {
            ImageSizeString = "Page Height";
            SearchBox.Text = GlobalVars.searchText;
            PostCountSlider.Value = GlobalVars.postCount;
            pageCount = GlobalVars.pageCount;
            canGetTags = true;

            SidePanelShadow.Receivers.Add(MainPanel);

            Window.Current.SizeChanged += OnWindowSizeChanged;

            if(singlePost != null)
            {
                Thread LoadThread = new Thread(LoadPosts);
                LoadThread.Start(new LoadPostsArgs(SearchBox.Text, singlePost.pageNumber));
            }
            else
            {
                Thread LoadThread = new Thread(LoadPosts);
                LoadThread.Start(new LoadPostsArgs(SearchBox.Text, -1));

            }


            ViewModel = new PostsViewModel();
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (GlobalVars.Rating == "")
            {
                RatingSelection.SelectedItem = "rating:explicit";
            }
            else
            {
                RatingSelection.SelectedItem = GlobalVars.Rating;
            }

            try
            {
                GlobalVars.ShowComments = (bool)localSettings.Values["comments"];
                CommentSwitch.IsOn = GlobalVars.ShowComments;
            }
            catch (Exception)
            {

            }
            try 
            { 
                GlobalVars.MuteVolume = (bool)localSettings.Values["volume"];
                VolumeSwitch.IsOn = GlobalVars.MuteVolume;
            }
            catch (Exception)
            {

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
            try
            {
                Post pick = (Post)e.Parameter;
                if (pick == null)
                {
                    return;
                }
                singlePost = pick;

                DownloadPool.Visibility = Visibility.Collapsed;

                Thread poolThread = new Thread(new ThreadStart(poolPopulate));
                poolThread.Start();
                if (pick.file.url != "https://ambisure.com/wp-content/uploads/2019/03/SHOCK1-1030x724.png")
                {
                    ImageLoadProgress.Visibility = Visibility.Visible;
                    VoteUpCount.Text = singlePost.score.up.ToString();
                    VoteDownCount.Text = singlePost.score.down.ToString();
                    if (singlePost.file.ext == "webm")
                    {
                        bigvideo.Source = new Uri(pick.file.url);
                        if (GlobalVars.MuteVolume)
                        {
                            bigvideo.Volume = 0;
                        }
                        else
                        {

                        }
                        bigvideo.Visibility = Visibility.Visible;
                        ImageLoadProgress.Visibility = Visibility.Collapsed;
                    }
                    else if (singlePost.file.ext == "swf")
                    {
                        smallpicture.Source = new Uri(pick.preview.url);
                        smallpicture.Visibility = Visibility.Visible;
                        ImageLoadProgress.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        bigpicture.Source = new Uri(pick.file.url);
                        bigpicture.Visibility = Visibility.Visible;
                    }
                    Thread commentsThread = new Thread(new ThreadStart(CommentsPopulate));
                    commentsThread.Start();
                    TopBar.Visibility = Visibility.Visible;
                    ImageGrid.Visibility = Visibility.Collapsed;
                    CloseButton.Visibility = Visibility.Visible;
                    PageText.Visibility = Visibility.Collapsed;
                    BackPage.Visibility = Visibility.Collapsed;
                    ForwardPage.Visibility = Visibility.Collapsed;
                    PageNumberText.Visibility = Visibility.Collapsed;
                    DescRect.Visibility = Visibility.Visible;
                    DescText.Text = singlePost.description;
                    try
                    {
                        if (ImageSizeString == "Sample Height")
                        {
                            bigpicture.Width = singlePost.sample.width;
                            bigpicture.Height = singlePost.sample.height;
                            bigvideo.Width = singlePost.sample.width;
                            bigvideo.Height = singlePost.sample.height;
                            ImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                            GlobalVars.Binding = "Sample Height";
                        }
                        else if (ImageSizeString == "Page Height")
                        {
                            bigpicture.Width = double.NaN;
                            bigpicture.Height = PostPage.ActualHeight - 65;
                            bigvideo.Width = PostPage.ActualWidth - 242;
                            bigvideo.Height = double.NaN;
                            ImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                            GlobalVars.Binding = "Page Height";
                        }
                        else
                        {
                            bigpicture.Width = singlePost.file.width;
                            bigpicture.Height = singlePost.file.height;
                            bigvideo.Width = singlePost.file.width;
                            bigvideo.Height = singlePost.file.height;
                            ImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                            GlobalVars.Binding = "Full Height";
                        }
                    }
                    catch (Exception)
                    {
                        bigpicture.Width = singlePost.sample.width;
                        bigpicture.Height = singlePost.sample.height;
                    }
                    Thread tags = new Thread(PopulateTreeView);
                    tags.Start();
                    Bindings.Update();
                }
            }
            catch (Exception)
            {

            }
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
                client.UserAgent = "e610.NET/1.6(by EpsilonRho)";

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
                int count = 0;
                foreach (Post p in DeserializedJson.posts)
                {
                    p.index = count;
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
                            p.file.url = "https://ambisure.com/wp-content/uploads/2019/03/SHOCK1-1030x724.png";
                            ViewModel.AddPost(p);
                            LoadingBar.Value++;
                        });
                    }
                    count++;
                    loadingPosts = false;
                }
                DeserializedJson = null;

                // Loading done, hide the progress bar
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                {
                    LoadingBar.Visibility = Visibility.Collapsed;
                    SearchTagAutoComplete.Items.Clear();
                });
                GC.Collect();
                loadingPosts = false;
            }
            catch (Exception)
            {

            }
        }

        // Get Post Tags
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
            client.UserAgent = "e610.NET/1.6(by EpsilonRho)";
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

        // SideBar Functions //

        // Button Functions //
        private void SearchButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.ClearPosts();
            int tabIndex = MainPage.MainTabViewAccess.SelectedIndex;
            if (SearchBox.Text == "")
            {
                (MainPage.MainTabViewAccess.TabItems[tabIndex] as TabViewItem).Header = "Latest Posts";
            }
            else
            {
                (MainPage.MainTabViewAccess.TabItems[tabIndex] as TabViewItem).Header = SearchBox.Text;
            }
            Bindings.Update();
            Thread LoadThread = new Thread(LoadPosts);
            LoadThread.Start(new LoadPostsArgs(SearchBox.Text, -1));
            pageCount = 1;
            Bindings.Update();
            GlobalVars.postCount = (int)PostCountSlider.Value;
            SearchName.Text = SearchBox.Text.Trim();
            if (SearchBox.Text.Contains("pool:"))
            {
                DownloadPool.Visibility = Visibility.Visible;
            }
            else
            {
                DownloadPool.Visibility = Visibility.Collapsed;
            }
            if (PageText.Visibility == Visibility.Collapsed)
            {
                bigpicture.Source = "";
                TopBar.Visibility = Visibility.Collapsed;
                bigpicture.Visibility = Visibility.Collapsed;
                bigvideo.Visibility = Visibility.Collapsed;
                smallpicture.Visibility = Visibility.Collapsed;
                ImageGrid.Visibility = Visibility.Visible;
                CloseButton.Visibility = Visibility.Collapsed;
                ImageLoadProgress.Visibility = Visibility.Collapsed;
                PageText.Visibility = Visibility.Visible;
                BackPage.Visibility = Visibility.Visible;
                ForwardPage.Visibility = Visibility.Visible;
                PageNumberText.Visibility = Visibility.Visible;
                DescRect.Visibility = Visibility.Collapsed;
                DescText.Text = "";
                Thread TagClean = new Thread(TagsCleanup);
                TagClean.Start();
                Bindings.Update();
            }
        }
        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                eraseButton.Visibility = Visibility.Collapsed;
                SearchTagAutoComplete.Items.Clear();
                ViewModel.ClearPosts();
                int tabIndex = MainPage.MainTabViewAccess.SelectedIndex;
                if (SearchBox.Text == "")
                {
                    (MainPage.MainTabViewAccess.TabItems[tabIndex] as TabViewItem).Header = "Latest Posts";
                }
                else
                {
                    (MainPage.MainTabViewAccess.TabItems[tabIndex] as TabViewItem).Header = SearchBox.Text;
                }
                Bindings.Update();
                Thread LoadThread = new Thread(LoadPosts);
                LoadThread.Start(new LoadPostsArgs(SearchBox.Text, -1));
                pageCount = 1;
                Bindings.Update();
                GlobalVars.postCount = (int)PostCountSlider.Value;
                SearchName.Text = SearchBox.Text.Trim();
                if (SearchBox.Text.Contains("pool:"))
                {
                    DownloadPool.Visibility = Visibility.Visible;
                }
                else
                {
                    DownloadPool.Visibility = Visibility.Collapsed;
                }
                if (PageText.Visibility == Visibility.Collapsed)
                {
                    bigpicture.Source = "";
                    TopBar.Visibility = Visibility.Collapsed;
                    bigpicture.Visibility = Visibility.Collapsed;
                    bigvideo.Visibility = Visibility.Collapsed;
                    smallpicture.Visibility = Visibility.Collapsed;
                    ImageGrid.Visibility = Visibility.Visible;
                    CloseButton.Visibility = Visibility.Collapsed;
                    ImageLoadProgress.Visibility = Visibility.Collapsed;
                    PageText.Visibility = Visibility.Visible;
                    BackPage.Visibility = Visibility.Visible;
                    ForwardPage.Visibility = Visibility.Visible;
                    PageNumberText.Visibility = Visibility.Visible;
                    DescRect.Visibility = Visibility.Collapsed;
                    DescText.Text = "";
                    Thread TagClean = new Thread(TagsCleanup);
                    TagClean.Start();
                    Bindings.Update();
                }
            }
        }
        private void ImageGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                Post pick = (Post)e.ClickedItem;
                singlePost = pick;
                DownloadPool.Visibility = Visibility.Collapsed;
                int tabIndex = MainPage.MainTabViewAccess.SelectedIndex;
                (MainPage.MainTabViewAccess.TabItems[tabIndex] as TabViewItem).Header = $"Post: {pick.id}";

                Thread poolThread = new Thread(new ThreadStart(poolPopulate));
                poolThread.Start();
                if (pick.file.url != "https://ambisure.com/wp-content/uploads/2019/03/SHOCK1-1030x724.png")
                {
                    ImageLoadProgress.Visibility = Visibility.Visible;
                    VoteUpCount.Text = singlePost.score.up.ToString();
                    VoteDownCount.Text = singlePost.score.down.ToString();
                    if (singlePost.file.ext == "webm")
                    {
                        bigvideo.Source = new Uri(pick.file.url);
                        if (GlobalVars.MuteVolume)
                        {
                            bigvideo.Volume = 0;
                        }
                        else
                        {

                        }
                        bigvideo.Visibility = Visibility.Visible;
                        ImageLoadProgress.Visibility = Visibility.Collapsed;
                    }
                    else if (singlePost.file.ext == "swf")
                    {
                        smallpicture.Source = new Uri(pick.preview.url);
                        smallpicture.Visibility = Visibility.Visible;
                        ImageLoadProgress.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        bigpicture.Source = new Uri(pick.file.url);
                        bigpicture.Visibility = Visibility.Visible;
                    }
                    Thread commentsThread = new Thread(new ThreadStart(CommentsPopulate));
                    commentsThread.Start();
                    TopBar.Visibility = Visibility.Visible;
                    ImageGrid.Visibility = Visibility.Collapsed;
                    CloseButton.Visibility = Visibility.Visible;
                    PageText.Visibility = Visibility.Collapsed;
                    BackPage.Visibility = Visibility.Collapsed;
                    ForwardPage.Visibility = Visibility.Collapsed;
                    PageNumberText.Visibility = Visibility.Collapsed;
                    DescRect.Visibility = Visibility.Visible;
                    DescText.Text = singlePost.description;
                    try
                    {
                        if (ImageSizeString == "Sample Height")
                        {
                            bigpicture.Width = singlePost.sample.width;
                            bigpicture.Height = singlePost.sample.height;
                            bigvideo.Width = singlePost.sample.width;
                            bigvideo.Height = singlePost.sample.height;
                            ImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                            GlobalVars.Binding = "Sample Height";
                        }
                        else if (ImageSizeString == "Page Height")
                        {
                            bigpicture.Width = double.NaN;
                            bigpicture.Height = PostPage.ActualHeight - 65;
                            bigvideo.Width = PostPage.ActualWidth - 242;
                            bigvideo.Height = double.NaN;
                            ImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                            GlobalVars.Binding = "Page Height";
                        }
                        else
                        {
                            bigpicture.Width = singlePost.file.width;
                            bigpicture.Height = singlePost.file.height;
                            bigvideo.Width = singlePost.file.width;
                            bigvideo.Height = singlePost.file.height;
                            ImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                            GlobalVars.Binding = "Full Height";
                        }
                    }
                    catch (Exception)
                    {
                        bigpicture.Width = singlePost.sample.width;
                        bigpicture.Height = singlePost.sample.height;
                    }
                    Thread tags = new Thread(PopulateTreeView);
                    tags.Start();
                    Bindings.Update();
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
        private void Account_Click(object sender, RoutedEventArgs e)
        {
            int tabIndex = MainPage.MainTabViewAccess.SelectedIndex;
            (MainPage.MainTabViewAccess.TabItems[tabIndex] as TabViewItem).Header = "Account";
            Bindings.Update();
            this.Frame.Navigate(typeof(AccountsPage), null, new DrillInNavigationTransitionInfo());
        }

        // Helper Functions //
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
        private List<Comment> GetComments()
        {
            try
            {
                var client = new RestClient();
                client.BaseUrl = new Uri("https://e621.net/comments.json?");
                client.UserAgent = "e610.NET/1.6(by EpsilonRho)";
                var request = new RestRequest(RestSharp.Method.GET);
                if (GlobalVars.Username != "" && GlobalVars.APIKey != "")
                {
                    request.AddQueryParameter("login", GlobalVars.Username);
                    request.AddQueryParameter("api_key", GlobalVars.APIKey);
                }
                request.AddQueryParameter("search[post_id]", singlePost.id.ToString());
                request.AddQueryParameter("group_by", "comment");
                request.AddQueryParameter("format", "json");
                IRestResponse response = client.Execute(request);
                string edited = response.Content.Remove(0, 1);
                edited = edited.Remove(edited.Count() - 1, 1);
                List<Comment> DeserializedJson = JsonConvert.DeserializeObject<List<Comment>>(response.Content);
                response = null;
                edited = null;
                return DeserializedJson;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private async void CommentsPopulate()
        {
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    CommentsSource.Clear();
                });
                Thread.Sleep(10);
                if (GlobalVars.ShowComments)
                {
                    List<Comment> Comments = GetComments();
                    for (int i = 0; i < Comments.Count(); i++)
                    {
                        Comment c = Comments[i];
                        if (c.body.Contains("[quote]"))
                        {
                            c.quotevis = Visibility.Visible;
                            c.quote = c.body.Substring(c.body.IndexOf("["), c.body.IndexOf("[/") - (c.body.IndexOf("[")));
                            c.body = c.body.Replace(c.quote, "");
                            c.body = c.body.Replace("[/quote]", "");
                            if (c.quote.Contains("[quote]\""))
                            {
                                c.quotedName = c.quote.Substring(c.quote.IndexOf("\""), c.quote.IndexOf("\n") - c.quote.IndexOf("\""));
                                c.quote = c.quote.Replace("[quote]", "");
                                c.quote = c.quote.Replace(c.quotedName, "");
                                string[] temp = c.quotedName.Split("/");
                                for (int k = 1; k < temp.Length; k++)
                                {
                                    c.quotedName = c.quotedName.Replace(temp[k], "");
                                }
                                c.quotedName = c.quotedName.Replace("/", "");
                                c.quotedName = c.quotedName.Replace("\"", "");
                                c.quotedName = c.quotedName.Replace(":", "");
                                c.quotedName += "said:";
                                temp = temp[temp.Count() - 1].Split(" ");
                                try
                                {
                                    c.quotedID = Int32.Parse(temp[0]);
                                }
                                catch (Exception)
                                {

                                }
                            }
                            else
                            {
                                c.quote = c.quote.Replace("[quote]", "");
                                c.quotedID = 0;
                                c.quotedName = "Quote:";
                            }
                        }
                        else
                        {
                            c.quotevis = Visibility.Collapsed;
                        }
                        c.Avatar_Url = "";
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            CommentsSource.Add(c);
                        });
                    }
                }
            }
            catch (Exception )
            {

            }
        }
        private void CommentsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView != null)
            {
                listView.SelectedIndex = -1;
            }
        }
        private void DescHyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (DescText.Visibility == Visibility.Visible)
            {
                DescText.Visibility = Visibility.Collapsed;
            }
            else
            {
                DescText.Visibility = Visibility.Visible;
            }
        }

        // Keyboard Functions //
        private void page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (SearchBox.FocusState != FocusState.Keyboard && SearchBox.FocusState != FocusState.Programmatic && SearchBox.FocusState != FocusState.Pointer)
            {
                if (e.Key == Windows.System.VirtualKey.Left)
                {
                    try
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
                    catch (Exception)
                    {

                    }
                }
                else if (e.Key == Windows.System.VirtualKey.Right)
                {
                    try
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
                    catch (Exception)
                    {

                    }
                }
                else if (e.Key == Windows.System.VirtualKey.Escape)
                {
                    try
                    {
                        if (Frame.CanGoBack)
                        {
                            Frame.GoBack();
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        // Post Functions //

        // Save Functions //
        private void bigpicture_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            MenuFlyout myFlyout = new MenuFlyout();
            MenuFlyoutItem firstItem = new MenuFlyoutItem { Text = "Copy Post" };
            firstItem.Click += new RoutedEventHandler(StartCopyAsync);
            myFlyout.Items.Add(firstItem);
            MenuFlyoutItem secondItem = new MenuFlyoutItem { Text = "Save Post" };
            secondItem.Click += new RoutedEventHandler(StartSaveAsync);
            myFlyout.Items.Add(secondItem);
            MenuFlyoutSubItem subMenu = new MenuFlyoutSubItem { Text = "Share" };
            MenuFlyoutItem ThirdItem = new MenuFlyoutItem { Text = "Copy Post Link" };
            ThirdItem.Click += new RoutedEventHandler(CopyPostLink);
            subMenu.Items.Add(ThirdItem);
            MenuFlyoutItem FourthItem = new MenuFlyoutItem { Text = "Copy Content Link" };
            FourthItem.Click += new RoutedEventHandler(CopyContentLink);
            subMenu.Items.Add(FourthItem);
            myFlyout.Items.Add(subMenu);
            myFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }
        private void StartSaveAsync(object sender, RoutedEventArgs e)
        {
            Thread saveThread = new Thread(new ThreadStart(SaveImage));
            saveThread.Start();
        }
        private async void SaveImage()
        {
            try
            {
                List<string> tags = new List<string>();
                
                foreach (string tag in singlePost.tags.artist)
                {
                    tags.Add(tag);
                }
                foreach (string tag in singlePost.tags.copyright)
                {
                    tags.Add(tag);
                }
                foreach (string tag in singlePost.tags.character)
                {
                    tags.Add(tag);
                }
                foreach (string tag in singlePost.tags.species)
                {
                    tags.Add(tag);
                }
                foreach (string tag in singlePost.tags.general)
                {
                    tags.Add(tag);
                }
                foreach (string tag in singlePost.tags.lore)
                {
                    tags.Add(tag);
                }
                foreach (string tag in singlePost.tags.meta)
                {
                    tags.Add(tag);
                }
                foreach (string tag in singlePost.tags.invalid)
                {
                    tags.Add(tag);
                }

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ImageLoadProgress.Visibility = Visibility.Visible;
                });
                HttpClient client = new HttpClient(); // Create HttpClient
                byte[] ByteArray = await client.GetByteArrayAsync(singlePost.file.url); // Download file
                IBuffer buffer = ByteArray.AsBuffer();

                StorageFile file = null;
                try
                {
                    file = await Windows.Storage.DownloadsFolder.CreateFileAsync(singlePost.file.md5 + "." + singlePost.file.ext);
                }
                catch (Exception)
                {
                    ShowInfoPopup("Saving Error: File Already Exists");
                    return;
                }

                if (singlePost.file.ext == "webm")
                {
                    using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await stream.WriteAsync(buffer); // Save
                    }
                    //VideoProperties videoProperties = await file.Properties.GetVideoPropertiesAsync();
                    ////foreach (string tag in tags)
                    ////{
                    ////    videoProperties.Keywords.Add(tag);
                    ////}
                    ////}
                    //await videoProperties.SavePropertiesAsync();
                    ShowInfoPopup("Post saved to downloads (Without tags)");
                    return;
                }
                else
                {
                    StringBuilder builder = new StringBuilder();
                    foreach (string tag in tags)
                    {
                        builder.Append($"{tag};");
                    }
                    string keywords = builder.ToString();

                    SoftwareBitmap softwareBitmap;

                    using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await stream.WriteAsync(buffer); // Save
                        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                        // Get the SoftwareBitmap representation of the file
                        softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                        encoder.SetSoftwareBitmap(softwareBitmap);

                        var propertySet = new Windows.Graphics.Imaging.BitmapPropertySet();
                        var kwds = new BitmapTypedValue(keywords, Windows.Foundation.PropertyType.String);
                        propertySet.Add(new KeyValuePair<string, BitmapTypedValue>("System.Keywords", kwds));

                        await encoder.BitmapProperties.SetPropertiesAsync(propertySet);

                        await encoder.FlushAsync();
                    }
                }

                ShowInfoPopup("Post saved to downloads");
            }
            catch (Exception e)
            {
                ShowInfoPopup("Saving Error: " + e.Message);
            }
        }
        private void StartCopyAsync(object sender, RoutedEventArgs e)
        {
            Thread saveThread = new Thread(new ThreadStart(CopyImage));
            saveThread.Start();
        }
        private async void CopyImage()
        {
            try
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ImageLoadProgress.Visibility = Visibility.Visible;
                });
                HttpClient client = new HttpClient(); // Create HttpClient
                //byte[] buffer = await client.GetByteArrayAsync(singlePost.file.url);
                var dataPackage = new DataPackage();
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromUri(new Uri(singlePost.file.url)));
                    Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                    ImageLoadProgress.Visibility = Visibility.Collapsed;
                });
                ShowInfoPopup("Copied!");
            }
            catch (Exception e)
            {
                ShowInfoPopup("Copying Error: " + e.Message);
            }
        }
        private async void CopyContentLink(object sender, RoutedEventArgs e)
        {
            try
            {
                var dataPackage = new DataPackage();
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    dataPackage.SetText(singlePost.file.url);
                    Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                    ImageLoadProgress.Visibility = Visibility.Collapsed;
                });
                ShowInfoPopup("Copied!");
            }
            catch (Exception err)
            {
                ShowInfoPopup("Copy Error: " + err.Message);
            }
        }
        private async void CopyPostLink(object sender, RoutedEventArgs e)
        {
            try
            {
                var dataPackage = new DataPackage();
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    dataPackage.SetText("https://e621.net/posts/" + singlePost.id);
                    Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                    ImageLoadProgress.Visibility = Visibility.Collapsed;
                });
                ShowInfoPopup("Copied!");
            }
            catch (Exception err)
            {
                ShowInfoPopup("Copy Error: " + err.Message);
            }
        }

        // Tag Functions //
        private void PopulateTreeView()
        {
            Thread.Sleep(200);
            if (singlePost.tags.artist.Count() > 0)
            {
                _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ArtistsTitle.Visibility = Visibility.Visible;
                });
                foreach (string str in singlePost.tags.artist)
                {
                    _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        ArtistsTags.Items.Add(str);
                    });
                }
            }

            if (singlePost.tags.copyright.Count() > 0)
            {
                _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    CopyrightsTitle.Visibility = Visibility.Visible;
                });
                foreach (string str in singlePost.tags.copyright)
                {
                    _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        CopyrightsTags.Items.Add(str);
                    });
                }
            }

            if (singlePost.tags.character.Count() > 0)
            {
                _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    CharactersTitle.Visibility = Visibility.Visible;
                });
                foreach (string str in singlePost.tags.character)
                {
                    _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        CharactersTags.Items.Add(str);
                    });
                }
            }

            if (singlePost.tags.species.Count() > 0)
            {
                _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    SpeciesTitle.Visibility = Visibility.Visible;
                });
                foreach (string str in singlePost.tags.species)
                {
                    _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        SpeciesTags.Items.Add(str);
                    });
                }
            }

            if (singlePost.tags.general.Count() > 0)
            {
                _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    GeneralTitle.Visibility = Visibility.Visible;
                });
                foreach (string str in singlePost.tags.general)
                {
                    _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        GeneralTags.Items.Add(str);
                    });
                }
            }

            if (singlePost.tags.meta.Count() > 0)
            {
                _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    MetaTitle.Visibility = Visibility.Visible;
                });
                foreach (string str in singlePost.tags.meta)
                {
                    _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        MetaTags.Items.Add(str);
                    });
                }
            }

            if (singlePost.tags.lore.Count() > 0)
            {
                _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    LoreTitle.Visibility = Visibility.Visible;
                });
                foreach (string str in singlePost.tags.lore)
                {
                    _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        LoreTags.Items.Add(str);
                    });
                }
            }

            if (singlePost.tags.invalid.Count() > 0)
            {
                _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    InvalidTitle.Visibility = Visibility.Visible;
                });
                foreach (string str in singlePost.tags.invalid)
                {
                    _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        InvalidTags.Items.Add(str);
                    });
                }
            }
        }
        private void Tags_ItemClick(object sender, ItemClickEventArgs e)
        {
            string ClickedItem = (string)e.ClickedItem;
            SearchBox.Text = ClickedItem; 
            ViewModel.ClearPosts();
            Thread LoadThread = new Thread(LoadPosts);
            LoadThread.Start(new LoadPostsArgs(ClickedItem, -1));
            bigpicture.Source = "";
            TopBar.Visibility = Visibility.Collapsed;
            bigpicture.Visibility = Visibility.Collapsed;
            bigvideo.Visibility = Visibility.Collapsed;
            smallpicture.Visibility = Visibility.Collapsed;
            ImageGrid.Visibility = Visibility.Visible;
            CloseButton.Visibility = Visibility.Collapsed;
            ImageLoadProgress.Visibility = Visibility.Collapsed;
            PageText.Visibility = Visibility.Visible;
            BackPage.Visibility = Visibility.Visible;
            ForwardPage.Visibility = Visibility.Visible;
            PageNumberText.Visibility = Visibility.Visible;
            DescRect.Visibility = Visibility.Collapsed;
            DescText.Text = "";
            pageCount = 1;
            if (SearchBox.Text.Contains("pool:"))
            {
                DownloadPool.Visibility = Visibility.Visible;
            }
            else
            {
                DownloadPool.Visibility = Visibility.Collapsed;
            }
            Thread TagClean = new Thread(TagsCleanup);
            TagClean.Start();
            Bindings.Update();
            //GlobalVars.newSearch = true;
            //GlobalVars.searchText = ClickedItem;
            //this.Frame.Navigate(typeof(PostsViewPage), null, new DrillInNavigationTransitionInfo());
            //if (SearchBox.Text.Contains("-" + ClickedItem))
            //{
            //    SearchBox.Text = SearchBox.Text.Replace("-" + ClickedItem, ClickedItem);
            //}
            //else if (SearchBox.Text.Contains(ClickedItem))
            //{
            //    SearchBox.Text = SearchBox.Text.Replace(ClickedItem, "");
            //}
            //else 
            //{ 
            //    if (SearchBox.Text.Count() > 0)
            //    {
            //        if (SearchBox.Text.Count() == SearchBox.Text.LastIndexOf(' '))
            //        {
            //            SearchBox.Text.Remove(SearchBox.Text.LastIndexOf(' '));
            //        }
            //        SearchBox.Text += " " + ClickedItem;
            //    }
            //    else
            //    {
            //        SearchBox.Text += ClickedItem;
            //    }
            //}

        }
        private void Tags_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            try
            {
                ClickedTag = (e.OriginalSource as FrameworkElement).DataContext as string;
                MenuFlyout myFlyout = new MenuFlyout();
                MenuFlyoutItem firstItem = new MenuFlyoutItem { Text = "Open in new tab" };
                firstItem.Click += new RoutedEventHandler(OpenInNewTab);
                myFlyout.Items.Add(firstItem);
                myFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
            }
            catch (Exception)
            {

            }
        }
        private void OpenInNewTab(object sender, RoutedEventArgs e)
        {
            var newTab = new Microsoft.UI.Xaml.Controls.TabViewItem();
            //newTab.IconSource = new SymbolIconSource() { Symbol = Symbol.Document };
            newTab.Header = ClickedTag;

            // The Content of a TabViewItem is often a frame which hosts a page.
            Frame frame = new Frame();
            newTab.Content = frame;

            GlobalVars.newSearch = true;
            GlobalVars.searchText = ClickedTag;

            frame.Navigate(typeof(PostsViewPage));

            MainPage.MainTabViewAccess.TabItems.Add(newTab);

            MainPage.MainTabViewAccess.SelectedItem = newTab;
        }

        private void TagsCleanup()
        {
            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ArtistsTitle.Visibility = Visibility.Collapsed;
                CopyrightsTitle.Visibility = Visibility.Collapsed;
                CharactersTitle.Visibility = Visibility.Collapsed;
                SpeciesTitle.Visibility = Visibility.Collapsed;
                GeneralTitle.Visibility = Visibility.Collapsed;
                MetaTitle.Visibility = Visibility.Collapsed;
                LoreTitle.Visibility = Visibility.Collapsed;
                InvalidTitle.Visibility = Visibility.Collapsed;
                ArtistsTags.Items.Clear();
                CopyrightsTags.Items.Clear();
                CharactersTags.Items.Clear();
                SpeciesTags.Items.Clear();
                GeneralTags.Items.Clear();
                MetaTags.Items.Clear();
                LoreTags.Items.Clear();
                InvalidTags.Items.Clear();
                CommentsSource.Clear();
                PoolsMenu.Items.Clear();
            });
        }
        private void Title_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton ClickedItem = e.OriginalSource as HyperlinkButton;
            switch (ClickedItem.Content)
            {
                case "Artists:":
                    if (ArtistsTags.Visibility == Visibility.Collapsed)
                    {
                        ArtistsTags.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ArtistsTags.Visibility = Visibility.Collapsed;
                    }
                    break;
                case "Copyrights:":
                    if (CopyrightsTags.Visibility == Visibility.Collapsed)
                    {
                        CopyrightsTags.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        CopyrightsTags.Visibility = Visibility.Collapsed;
                    }
                    break;
                case "Characters:":
                    if (CharactersTags.Visibility == Visibility.Collapsed)
                    {
                        CharactersTags.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        CharactersTags.Visibility = Visibility.Collapsed;
                    }
                    break;
                case "Species:":
                    if (SpeciesTags.Visibility == Visibility.Collapsed)
                    {
                        SpeciesTags.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        SpeciesTags.Visibility = Visibility.Collapsed;
                    }
                    break;
                case "General:":
                    if (GeneralTags.Visibility == Visibility.Collapsed)
                    {
                        GeneralTags.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        GeneralTags.Visibility = Visibility.Collapsed;
                    }
                    break;
                case "Meta:":
                    if (MetaTags.Visibility == Visibility.Collapsed)
                    {
                        MetaTags.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MetaTags.Visibility = Visibility.Collapsed;
                    }
                    break;
                case "Lore:":
                    if (LoreTags.Visibility == Visibility.Collapsed)
                    {
                        LoreTags.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        LoreTags.Visibility = Visibility.Collapsed;
                    }
                    break;
                case "Invalid:":
                    if (InvalidTags.Visibility == Visibility.Collapsed)
                    {
                        InvalidTags.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        InvalidTags.Visibility = Visibility.Collapsed;
                    }
                    break;
            }
        }

        // Post Movement //
        private void ForwardPost_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Thread LoadThread = new Thread(ForwardPostLoad);
            LoadThread.Start();
        }
        private void BackPost_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Thread LoadThread = new Thread(BackPostLoad);
            LoadThread.Start();
        }
        private async void ForwardPostLoad()
        {
            if (singlePost.index == ViewModel.Posts.Count - 1)
            {
                loadingPosts = true;
                Thread LoadThread = new Thread(LoadPosts);
                pageCount++;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                {
                    ViewModel.ClearPosts();
                    LoadThread.Start(new LoadPostsArgs(SearchBox.Text, pageCount));
                });
                while (loadingPosts)
                {

                }
                try
                {
                    singlePost = ViewModel.Posts[0];
                }
                catch (Exception)
                {

                }
            }
            else
            {
                try
                {
                    Post pick = ViewModel.Posts[singlePost.index + 1];
                    singlePost = pick;
                }
                catch (Exception)
                {
                    Thread LoadThread = new Thread(LoadPosts);
                    pageCount--;
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                    {
                        ViewModel.ClearPosts();
                        LoadThread.Start(new LoadPostsArgs(SearchBox.Text, pageCount));
                    });
                    return;
                }
            }
            if(ViewModel.Posts.Count == 0)
            {
                Thread LoadThread = new Thread(LoadPosts);
                pageCount--;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                {
                    ViewModel.ClearPosts();
                    LoadThread.Start(new LoadPostsArgs(SearchBox.Text, pageCount));
                });
                return;
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
            {
                bigvideo.Visibility = Visibility.Collapsed;
                bigpicture.Visibility = Visibility.Collapsed;
                smallpicture.Visibility = Visibility.Collapsed;
                TagsCleanup();
            });
            if (singlePost.file.url != "https://ambisure.com/wp-content/uploads/2019/03/SHOCK1-1030x724.png")
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                {
                    ImageLoadProgress.Visibility = Visibility.Visible;
                    VoteUpCount.Text = singlePost.score.up.ToString();
                    VoteDownCount.Text = singlePost.score.down.ToString();
                });
                if (singlePost.file.ext == "webm")
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                    {
                        bigvideo.Source = new Uri(singlePost.file.url);
                    });
                    if (GlobalVars.MuteVolume)
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                        {
                            bigvideo.Volume = 0;
                        });
                    }
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                    {
                        bigvideo.Visibility = Visibility.Visible;
                        ImageLoadProgress.Visibility = Visibility.Collapsed;
                    });
                }
                else if (singlePost.file.ext == "swf")
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                    {
                        smallpicture.Source = new Uri(singlePost.preview.url);
                        smallpicture.Visibility = Visibility.Visible;
                        ImageLoadProgress.Visibility = Visibility.Collapsed;
                    });
                }
                else
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                    {
                        bigpicture.Source = new Uri(singlePost.file.url);
                        bigpicture.Visibility = Visibility.Visible;
                    });
                }
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                {
                    DescText.Text = singlePost.description;
                });
                Thread tags = new Thread(PopulateTreeView);
                tags.Start();
                Thread commentsThread = new Thread(new ThreadStart(CommentsPopulate));
                commentsThread.Start();

                Thread poolThread = new Thread(new ThreadStart(poolPopulate));
                poolThread.Start();
            }
            else
            {
                ForwardPostLoad();
                return;
            }
        }
        private async void BackPostLoad()
        {
            if (singlePost.index == 0 && pageCount != 1)
            {
                Thread LoadThread = new Thread(LoadPosts);
                pageCount--;
                int sliderValue = 0;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                {
                    ViewModel.ClearPosts();
                    LoadThread.Start(new LoadPostsArgs(SearchBox.Text, pageCount));
                    sliderValue = (int)PostCountSlider.Value;
                });
                while (ViewModel.Posts.Count != sliderValue)
                {

                }
                singlePost = ViewModel.Posts[ViewModel.Posts.Count - 1];
            }
            else if(singlePost.index == 0)
            {
                return;
            }
            else
            {
                Post pick = ViewModel.Posts[singlePost.index - 1];
                singlePost = pick;
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
            {
                bigvideo.Visibility = Visibility.Collapsed;
                bigpicture.Visibility = Visibility.Collapsed;
                smallpicture.Visibility = Visibility.Collapsed;
                TagsCleanup();
            });
            if (singlePost.file.url != "https://ambisure.com/wp-content/uploads/2019/03/SHOCK1-1030x724.png")
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                {
                    ImageLoadProgress.Visibility = Visibility.Visible;
                    VoteUpCount.Text = singlePost.score.up.ToString();
                    VoteDownCount.Text = singlePost.score.down.ToString();
                });
                if (singlePost.file.ext == "webm")
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                    {
                        bigvideo.Source = new Uri(singlePost.file.url);
                    });
                    if (GlobalVars.MuteVolume)
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                        {
                            bigvideo.Volume = 0;
                        });
                    }
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                    {
                        bigvideo.Visibility = Visibility.Visible;
                        ImageLoadProgress.Visibility = Visibility.Collapsed;
                    });
                }
                else if (singlePost.file.ext == "swf")
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                    {
                        smallpicture.Source = new Uri(singlePost.preview.url);
                        smallpicture.Visibility = Visibility.Visible;
                        ImageLoadProgress.Visibility = Visibility.Collapsed;
                    });
                }
                else
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                    {
                        bigpicture.Source = new Uri(singlePost.file.url);
                        bigpicture.Visibility = Visibility.Visible;
                    });
                }
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                {
                    DescText.Text = singlePost.description;
                });
                Thread tags = new Thread(PopulateTreeView);
                tags.Start();
                Thread commentsThread = new Thread(new ThreadStart(CommentsPopulate));
                commentsThread.Start();

                Thread poolThread = new Thread(new ThreadStart(poolPopulate));
                poolThread.Start();
            }
            else
            {
                BackPostLoad();
                return;
            }
        }

        // Exit Functions //
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            int tabIndex = MainPage.MainTabViewAccess.SelectedIndex;
            if (SearchBox.Text == "")
            {
                (MainPage.MainTabViewAccess.TabItems[tabIndex] as TabViewItem).Header = "Latest Posts";
            }
            else
            {
                (MainPage.MainTabViewAccess.TabItems[tabIndex] as TabViewItem).Header = SearchBox.Text;
            }
            bigpicture.Source = null;
            bigvideo.Source = null;
            smallpicture.Source = null;
            TopBar.Visibility = Visibility.Collapsed;
            bigpicture.Visibility = Visibility.Collapsed;
            bigvideo.Visibility = Visibility.Collapsed;
            smallpicture.Visibility = Visibility.Collapsed;
            ImageGrid.Visibility = Visibility.Visible;
            CloseButton.Visibility = Visibility.Collapsed;
            ImageLoadProgress.Visibility = Visibility.Collapsed;
            PageText.Visibility = Visibility.Visible;
            BackPage.Visibility = Visibility.Visible;
            ForwardPage.Visibility = Visibility.Visible;
            PageNumberText.Visibility = Visibility.Visible;
            DescRect.Visibility = Visibility.Collapsed;
            DescText.Text = "";
            if (SearchBox.Text.Contains("pool:"))
            {
                DownloadPool.Visibility = Visibility.Visible;
            }
            else
            {
                DownloadPool.Visibility = Visibility.Collapsed;
            }
            Thread TagClean = new Thread(TagsCleanup);
            TagClean.Start();
            Bindings.Update();
        }

        // Helper Functions //
        private void bigpicture_ImageExOpened(object sender, Microsoft.Toolkit.Uwp.UI.Controls.ImageExOpenedEventArgs e)
        {
            ImageLoadProgress.Visibility = Visibility.Collapsed;
        }
        private async void poolPopulate()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                PoolsMenu.Items.Clear();
            });
            List<int> pools = singlePost.pools;
            foreach (int p in pools)
            {
                Pool newpool = getPoolInfo(p);
                if (newpool != null)
                {
                    newpool.name = newpool.name.Replace("_", " ");
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        var newMenuItem = new MenuFlyoutItem();
                        newMenuItem.Text = newpool.name;
                        newMenuItem.Click += (s, e1) =>
                        {
                            singlePool = newpool;
                            SearchBox.Text = "order:id pool:" + newpool.id;
                            ViewModel.ClearPosts();
                            Thread LoadThread = new Thread(LoadPosts);
                            LoadThread.Start(new LoadPostsArgs(SearchBox.Text, -1));
                            bigpicture.Source = "";
                            TopBar.Visibility = Visibility.Collapsed;
                            bigpicture.Visibility = Visibility.Collapsed;
                            bigvideo.Visibility = Visibility.Collapsed;
                            smallpicture.Visibility = Visibility.Collapsed;
                            ImageGrid.Visibility = Visibility.Visible;
                            CloseButton.Visibility = Visibility.Collapsed;
                            ImageLoadProgress.Visibility = Visibility.Collapsed;
                            PageText.Visibility = Visibility.Visible;
                            BackPage.Visibility = Visibility.Visible;
                            ForwardPage.Visibility = Visibility.Visible;
                            PageNumberText.Visibility = Visibility.Visible;
                            DescRect.Visibility = Visibility.Collapsed;
                            DescText.Text = "";
                            DownloadPool.Visibility = Visibility.Visible;
                            Thread TagClean = new Thread(TagsCleanup);
                            TagClean.Start();
                            Bindings.Update();
                        };
                        PoolsMenu.Items.Add(newMenuItem);
                        //MovementSource.Add(newpool);
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
                client.UserAgent = "e610.NET/1.6(by EpsilonRho)";
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
        private void OnWindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Size window = GetCurrentDisplaySize();
            try
            {
                if (singlePost.file.ext == "webm")
                {
                    bigvideo.Width = window.Width - 230;
                    bigvideo.Height = window.Height - 105;
                }
                else
                {
                    if (ImageSizeString == "Page Height")
                    {
                        bigpicture.Width = double.NaN;
                        bigpicture.Height = window.Height - 65;
                        ImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                        ImageSizeString = "Page Height";
                    }
                }
            }
            catch (Exception)
            {

            }
        }
        public static Size GetCurrentDisplaySize()
        {
            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            var size = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);
            return size;
        }
        private async void ShowInfoPopup(string info)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                InfoPopupText.Text = info;
                InfoPopup.Opacity = 100;
                InfoPopupText.Opacity = 100;
                InfoPopup.Visibility = Visibility.Visible;
                ImageLoadProgress.Visibility = Visibility.Collapsed;
            });
            IsPopupOpen = true;
            Thread t = new Thread(InfoPopupTimer);
            t.Start();
        }
        private async void InfoPopupTimer()
        {
            Thread.Sleep(2000);
            IsPopupOpen = false;
            for (double i = 1.0; i > 0; i = i - 0.01)
            {
                if (IsPopupOpen)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        InfoPopup.Opacity = 100;
                        InfoPopupText.Opacity = 100;
                    });
                    return;
                }
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    InfoPopup.Opacity = i;
                    InfoPopupText.Opacity = i;
                });
                Thread.Sleep(16);
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                InfoPopup.Visibility = Visibility.Collapsed;
            });
        }

        // Settings //
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var taskName = "Notification";
            NotificationStatus.IsChecked = false;
            NotificationStatus.Content = "Notifications Off";
            foreach (var t in BackgroundTaskRegistration.AllTasks)
            {
                if (t.Value.Name == taskName)
                {
                    NotificationStatus.IsChecked = true;
                    NotificationStatus.Content = "Notifications On";
                }
            }
            SettingsDialog.ShowAsync();
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
            GlobalVars.MuteVolume = VolumeSwitch.IsOn;
            localSettings.Values["volume"] = GlobalVars.MuteVolume;
        }

        // Top Bar Functions //
        private void ImageSize_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(ImageSize);
        }
        private void PoolButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(PoolButton);
        }
        private void VoteUpButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var client = new RestClient();
            client.BaseUrl = new Uri("https://e621.net/posts/" + singlePost.id + "/votes.json");
            client.UserAgent = "e610.NET/1.6(by EpsilonRho)";
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
            }
            else
            {
                singlePost.score.up--;
                VoteUpCount.Text = singlePost.score.up.ToString();
            }
            ShowInfoPopup("Post Updated");
        }
        private void VoteDownButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var client = new RestClient();
            client.BaseUrl = new Uri("https://e621.net/posts/" + singlePost.id + "/votes.json");
            client.UserAgent = "e610.NET/1.6(by EpsilonRho)";
            var request = new RestRequest(RestSharp.Method.POST);
            if (GlobalVars.Username != "" && GlobalVars.APIKey != "")
            {
                request.AddQueryParameter("login", GlobalVars.Username);
                request.AddQueryParameter("api_key", GlobalVars.APIKey);
            }
            request.AddQueryParameter("score", "-1");
            var response = client.Execute(request);
            VoteResponse DeserializedJson = JsonConvert.DeserializeObject<VoteResponse>(response.Content);
            if (DeserializedJson.our_score == -1)
            {
                singlePost.score.down--;
                VoteDownCount.Text = singlePost.score.down.ToString();
            }
            else
            {
                singlePost.score.down++;
                VoteDownCount.Text = singlePost.score.down.ToString();
            }
            ShowInfoPopup("Post Updated");
        }
        private void FavoiteButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var client = new RestClient();
            client.BaseUrl = new Uri("https://e621.net/favorites.json");
            client.UserAgent = "e610.NET/1.6(by EpsilonRho)";
            var request = new RestRequest(RestSharp.Method.POST);
            if (GlobalVars.Username != "" && GlobalVars.APIKey != "")
            {
                request.AddQueryParameter("login", GlobalVars.Username);
                request.AddQueryParameter("api_key", GlobalVars.APIKey);
            }
            request.AddQueryParameter("post_id", singlePost.id.ToString());
            var response = client.Execute(request);
            if (!response.Content.Contains("You have already favorited this post"))
            {
                ShowInfoPopup("Post Favorited");
            }
            else
            {
                client.BaseUrl = new Uri("https://e621.net/favorites/" + singlePost.id.ToString() + ".json");
                client.UserAgent = "e610.NET/1.6(by EpsilonRho)";
                request = new RestRequest(RestSharp.Method.DELETE);
                if (GlobalVars.Username != "" && GlobalVars.APIKey != "")
                {
                    request.AddQueryParameter("login", GlobalVars.Username);
                    request.AddQueryParameter("api_key", GlobalVars.APIKey);
                }
                response = client.Execute(request);
                ShowInfoPopup("Post UnFavorited");
            }
        }
        private void ImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem clickItem = (MenuFlyoutItem)e.OriginalSource;
            try
            {
                if (clickItem.Text == "Sample Height")
                {
                    bigpicture.Width = singlePost.sample.width;
                    bigpicture.Height = singlePost.sample.height;
                    bigvideo.Width = singlePost.sample.width;
                    bigvideo.Height = singlePost.sample.height;
                    ImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    ImageSizeString = "Sample Height";
                }
                else if (clickItem.Text == "Page Height")
                {
                    bigpicture.Width = double.NaN;
                    bigpicture.Height = PostPage.ActualHeight - 65;
                    bigvideo.Width = PostPage.ActualWidth - 230;
                    bigvideo.Height = double.NaN;
                    ImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    ImageSizeString = "Page Height";
                }
                else
                {
                    bigpicture.Width = singlePost.file.width;
                    bigpicture.Height = singlePost.file.height;
                    bigvideo.Width = singlePost.file.width;
                    bigvideo.Height = singlePost.file.height;
                    ImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    ImageSizeString = "Full Height";
                }
            }
            catch (Exception)
            {
                bigpicture.Width = singlePost.sample.width;
                bigpicture.Height = singlePost.sample.height;
            }
        }

        // Touch Gestures //
        private void bigpicture_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.IsInertial && !_isPostSwiped)
            {
                var swipedDistance = e.Cumulative.Translation.X;

                if (Math.Abs(swipedDistance) <= 200) return;

                if (swipedDistance > 0)
                {
                    Thread LoadThread = new Thread(BackPostLoad);
                    LoadThread.Start();
                }
                else
                {
                    Thread LoadThread = new Thread(ForwardPostLoad);
                    LoadThread.Start();
                }
                _isPostSwiped = true;
            }
        }
        private void bigpicture_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _isPostSwiped = false;
        }
        private void ImageGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.IsInertial && !_isGridSwiped)
            {
                var swipedDistance = e.Cumulative.Translation.X;

                if (Math.Abs(swipedDistance) <= 200) return;

                if (swipedDistance < 0)
                {
                    try
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
                    catch (Exception)
                    {

                    }
                }
                else
                {
                    try
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
                    catch (Exception)
                    {

                    }
                }
                _isGridSwiped = true;
            }
        }
        private void ImageGrid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _isGridSwiped = false;
        }

        // Background Notifications //
        private async void RegisterNotification()
        {
            try
            {
                uint time = 30;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    time = Convert.ToUInt32(NotificationTime.Text);
                });
                TimeTrigger timeTrigger = new TimeTrigger(time, false);
                var requestStatus = await BackgroundExecutionManager.RequestAccessAsync();
                if (requestStatus != BackgroundAccessStatus.AlwaysAllowed)
                {

                }

                var taskName = "Notification";

                foreach (var t in BackgroundTaskRegistration.AllTasks)
                {
                    if (t.Value.Name == taskName)
                    {
                        t.Value.Unregister(true);
                    }
                }

                var builder = new BackgroundTaskBuilder();

                builder.Name = taskName;
                builder.TaskEntryPoint = "BackgroundNotificationHandler.Notification";
                builder.SetTrigger(timeTrigger);
                builder.AddCondition(new SystemCondition(SystemConditionType.UserNotPresent));
                BackgroundTaskRegistration task = builder.Register();
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    NotificationStatus.IsChecked = true;
                    NotificationStatus.Content = "Notifications On";
                });
            }
            catch (Exception e)
            {
                ShowInfoPopup("Couldn't Register Background Task" + e.Message);
            }
        }
        private async void DeregisterNotification()
        {
            try
            {
                var taskName = "Notifier";

                foreach (var t in BackgroundTaskRegistration.AllTasks)
                {
                    if (t.Value.Name == taskName)
                    {
                        t.Value.Unregister(true);
                    }
                }

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    NotificationStatus.IsChecked = false;
                    NotificationStatus.Content = "Notifications Off";
                });
            }
            catch (Exception)
            {
                ShowInfoPopup("Couldn't Deregister Background Task");
            }
        }

        private void DeregisterButton_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(DeregisterNotification);
            t.Start();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uint x = Convert.ToUInt32(NotificationTime.Text);
                if (x < 15)
                {
                    ShowInfoPopup("Time must be higher than 15 minutes");
                    return;
                }
                Thread t = new Thread(RegisterNotification);
                t.Start();
            }
            catch (Exception)
            {
                ShowInfoPopup("Error: invalid time");
            }
        }

        private void NotificationTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            NotificationTime.Text = new string(NotificationTime.Text.Where(char.IsDigit).ToArray());
            NotificationTime.SelectionStart = NotificationTime.Text.Length;
            NotificationTime.SelectionLength = 0;
        }
        private void AppHyperlinkClick(object sender, RoutedEventArgs e)
        {
            if (AppSettingsPanel.Visibility == Visibility.Visible)
            {
                AppSettingsPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                AppSettingsPanel.Visibility = Visibility.Visible;
            }
        }
        private void NotificationHyperlinkClick(object sender, RoutedEventArgs e)
        {
            if (NotificationsSettingsPanel.Visibility == Visibility.Visible)
            {
                NotificationsSettingsPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                NotificationsSettingsPanel.Visibility = Visibility.Visible;
            }
        }

        private void Following_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(FollowingPage));
        }

        public Root DownloadPostHolder;
        private void LoadPoolPostsForDownload(int page, string tags)
        {
            // Function Vars
            var client = new RestClient(); // Client to handle Requests
            double limit = 75; // Post Limit
            var request = new RestRequest(RestSharp.Method.GET); // REST request

            // Set Endpoint
            // TODO: Switching between e621 - gelbooru - r34 - etc
            client.BaseUrl = new Uri("https://e621.net/posts.json?");

            // Set the useragent for e621
            client.UserAgent = "e610.NET/1.6(by EpsilonRho)";

            // If user is logged in set login parameters into request
            if (GlobalVars.Username != "" && GlobalVars.APIKey != "")
            {
                request.AddQueryParameter("login", GlobalVars.Username);
                request.AddQueryParameter("api_key", GlobalVars.APIKey);
            }

            // Set parameters for tags and post limit
            request.AddQueryParameter("tags", GlobalVars.Rating + " "+ tags);
            request.AddQueryParameter("limit", limit.ToString());

            request.AddQueryParameter("page", page.ToString());

            // Send the request
            var response = client.Execute(request);

            // Deserialize the response
            DownloadPostHolder = JsonConvert.DeserializeObject<Root>(response.Content);
        }
        private void DownloadPool_Click(object sender, RoutedEventArgs e)
        {
            if (DownloadProgress.Visibility == Visibility.Visible)
            {
                stopDownload = true;
            }
            else
            {
                StartSaveAsync();
            }
        }
        private void StartSaveAsync()
        {
            Thread saveThread = new Thread(SaveTags);
            if (!saveThread.IsAlive)
            {
                try
                {
                    saveThread.Start();
                }
                catch (Exception)
                {

                }
            }
        }
        private async void SaveTags()
        {
            int postCount = 0;
            int viewPosts = 0;
            int downloadpage = 1;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                DownloadProgress.IsIndeterminate = false;
                DownloadProgress.Value = 0;
                DownloadProgress.Maximum = 10;
                DownloadProgress.Visibility = Visibility.Visible;
                NormalText.Visibility = Visibility.Collapsed;
            });
            StorageFolder folder = null;
            try
            {
                string foldername = singlePool.name.Replace("_", " ");
                foldername = foldername.Replace(":", "");
                foldername = foldername.Replace("<", "");
                foldername = foldername.Replace(">", "");
                foldername = foldername.Replace("\"", "");
                foldername = foldername.Replace("\\", "");
                foldername = foldername.Replace("/", "");
                foldername = foldername.Replace("|", "");
                foldername = foldername.Replace("?", "");
                foldername = foldername.Replace("*", "");
                folder = await Windows.Storage.DownloadsFolder.CreateFolderAsync(foldername);
            }
            catch (Exception)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    DownloadProgress.Visibility = Visibility.Collapsed;
                    DownloadPool.Visibility = Visibility.Collapsed;
                });
                ShowInfoPopup("Saving Error: Folder already Exists");
                return;
            }
            string tags = "";
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                tags = SearchBox.Text;
            });
            LoadPoolPostsForDownload(downloadpage, tags);
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                DownloadProgress.IsIndeterminate = false;
                DownloadProgress.Maximum = singlePool.post_count;
                DownloadProgress.Value = 0;
                FilesText.Text = postCount.ToString() + "/" + singlePool.post_count.ToString();
            });

            while (postCount < singlePool.post_count)
            {
                try
                {
                    HttpClient client = new HttpClient(); // Create HttpClient
                    byte[] buffer = await client.GetByteArrayAsync(DownloadPostHolder.posts[viewPosts].file.url); // Download file
                    StorageFile file = await folder.CreateFileAsync(postCount.ToString() + "." + DownloadPostHolder.posts[viewPosts].file.ext);

                    using (Stream stream = await file.OpenStreamForWriteAsync())
                    {
                        stream.Write(buffer, 0, buffer.Length); // Save
                    }
                    postCount++;
                    viewPosts++;
                    _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        DownloadProgress.Value++;
                        FilesText.Text = postCount.ToString() + "/" + singlePool.post_count.ToString();
                    });
                }
                catch (Exception e)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        DownloadProgress.Visibility = Visibility.Collapsed;
                        DownloadPool.Visibility = Visibility.Collapsed;
                    });
                    ShowInfoPopup("Saving Error: " + e.Message);
                }
                if (viewPosts == DownloadPostHolder.posts.Count)
                {
                    downloadpage++;
                    LoadPoolPostsForDownload(downloadpage, tags);
                    viewPosts = 0;
                }
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                DownloadProgress.Visibility = Visibility.Collapsed;
                NormalText.Visibility = Visibility.Visible;
                FilesText.Text = "";
                DownloadPool.Visibility = Visibility.Collapsed;
            });
            ShowInfoPopup("Posts saved to downloads");
        }

        private void erase_Tapped(object sender, TappedRoutedEventArgs e)
        {
            eraseButton.Visibility = Visibility.Collapsed;
            SearchBox.Text = "";
        }

        private void SearchBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            eraseButton.Visibility = Visibility.Visible;
            if (SearchBox.Text == "")
            {
                eraseButton.Visibility = Visibility.Collapsed;
            }
        }

        private void SauceNao_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SauceNao), null, new DrillInNavigationTransitionInfo());
        }

        private void Tags_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                var pointer = e.GetCurrentPoint(sender as UIElement);
                if (pointer.Properties.PointerUpdateKind == Windows.UI.Input.PointerUpdateKind.MiddleButtonReleased)
                {
                    ClickedTag = (e.OriginalSource as FrameworkElement).DataContext as string;
                    var newTab = new Microsoft.UI.Xaml.Controls.TabViewItem();

                    newTab.Header = ClickedTag;

                    // The Content of a TabViewItem is often a frame which hosts a page.
                    Frame frame = new Frame();
                    newTab.Content = frame;

                    GlobalVars.newSearch = true;
                    GlobalVars.searchText = ClickedTag;

                    frame.Navigate(typeof(PostsViewPage));

                    MainPage.MainTabViewAccess.TabItems.Add(newTab);

                    //MainPage.MainTabViewAccess.SelectedItem = newTab;
                }
            }
            catch (Exception)
            {

            }
        }

        private void ImageGrid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                var pointer = e.GetCurrentPoint(sender as UIElement);
                if (pointer.Properties.PointerUpdateKind == Windows.UI.Input.PointerUpdateKind.MiddleButtonReleased)
                {
                    Post pick = (e.OriginalSource as FrameworkElement).DataContext as Post;
                    pick.pageNumber = pageCount;
                    var newTab = new Microsoft.UI.Xaml.Controls.TabViewItem();

                    newTab.Header = $"Post: {pick.id}";

                    // The Content of a TabViewItem is often a frame which hosts a page.
                    Frame frame = new Frame();
                    newTab.Content = frame;

                    GlobalVars.searchText = SearchBox.Text;

                    frame.Navigate(typeof(PostsViewPage), pick);

                    MainPage.MainTabViewAccess.TabItems.Add(newTab);

                    //MainPage.MainTabViewAccess.SelectedItem = newTab;
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
