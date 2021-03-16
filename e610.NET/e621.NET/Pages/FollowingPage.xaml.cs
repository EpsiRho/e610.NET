using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace e610.NET.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FollowingPage : Page
    {
        List<string> follows;
        List<string> Updated;
        public PostsViewModel FullViewModel { get; set; }
        public PostsViewModel SortedViewModel { get; set; }
        private ObservableCollection<Comment> CommentsSource = new ObservableCollection<Comment>(); // Tags tree binding source
        private ObservableCollection<Pool> ConnectedPools = new ObservableCollection<Pool>(); // Pools list binding source
        private ObservableCollection<Pool> MovementSource = new ObservableCollection<Pool>(); // Movement list binding source
        public int pageCount;
        public bool canGetTags;
        private Post singlePost;
        private bool _isPostSwiped;
        private string ImageSizeString;
        public bool stopDownload;
        private bool IsPopupOpen;
        public bool loadingPosts;
        private string ClickedTag;
        public FollowingPage()
        {
            this.InitializeComponent();
            FullViewModel = new PostsViewModel();
            ImageSizeString = "Page Height";
            SortedViewModel = new PostsViewModel();
            Window.Current.SizeChanged += OnWindowSizeChanged;
            LoadPage();
        }

        private async void LoadPage()
        {
            FollowedTags.Items.Add($"All Tags");
            FollowedTags.SelectedIndex = 0;
            await Task.Run(async () =>
            {
                StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile saveFile = await storageFolder.CreateFileAsync("SaveFile.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
                string text = await Windows.Storage.FileIO.ReadTextAsync(saveFile);
                follows = text.Split('\n').ToList();
                foreach (string name in follows)
                {
                    if (name != "")
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            FollowedTags.Items.Add($"{name.Split(':')[0]}");
                        });
                    }

                }
            });
            Thread update = new Thread(UpdatePostCount);
            update.Start();
            LoadingBar.IsIndeterminate = false;
            LoadingBar.Maximum = follows.Count();
            LoadingBar.Value = 0;
            LoadingBar.Visibility = Visibility.Visible;
            int total = follows.Count();
            int count = 0;
            Stopwatch watch = new Stopwatch();
            while (count < total)
            {
                StringBuilder tags = new StringBuilder();
                for (int i = 0; i < 10; i++)
                {
                    LoadingBar.Value = count;
                    if ((count) < follows.Count())
                    {
                        if (follows[count].Split(':')[0] != "")
                        {
                            tags.Append($"~{follows[count].Split(':')[0]} ");
                            count++;
                        }
                        else
                        {
                            count++;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if (watch.IsRunning)
                {
                    watch.Stop();
                    if ((500 - watch.ElapsedMilliseconds) > 0)
                    {
                        int wait = (int)(500 - watch.ElapsedMilliseconds);
                        Thread.Sleep(wait);
                    }
                }
                watch.Start();
                if (tags.ToString() == "")
                {
                    break;
                }
                await Task.Run(async () =>
                {
                    LoadPosts(new LoadPostsArgs(tags.ToString(), -1));
                });
            }
            LoadingBar.Value = count;
            LoadingBar.Visibility = Visibility.Collapsed;
        }
        public async void LoadPosts(object t)
        {
            try
            {
                // Function Vars
                var client = new RestClient(); // Client to handle Requests
                double limit = 100; // Post Limit
                LoadPostsArgs args = (LoadPostsArgs)t; // Convert Object to LoadPostArgs class
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

                request.AddQueryParameter("tags", GlobalVars.Rating + " " + args.tags);

                // Set parameters for tags and post limit
                request.AddQueryParameter("limit", limit.ToString());

                // Send the request
                var response = client.Execute(request);

                // Deserialize the response
                Root DeserializedJson = JsonConvert.DeserializeObject<Root>(response.Content);

                // Get the posts from the Deserialized Class
                int count = 0;
                foreach (Post p in DeserializedJson.posts)
                {
                    p.index = count;
                    p.ItemVis = Visibility.Visible;
                    if (p.preview.url != null)
                    {
                        if (SortedViewModel.Posts.Count() != 0)
                        {
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                if (args.paginationChar == 'b')
                                {
                                    SortedViewModel.AddPostAtIndex(p);
                                }
                                FullViewModel.AddPostAtIndex(p);
                            });
                        }
                        else
                        {
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                if (args.paginationChar == 'b')
                                {
                                    SortedViewModel.AddPost(p);
                                }
                                FullViewModel.AddPost(p);
                            });
                        }
                    }
                    else
                    {

                    }
                    count++;
                }
                DeserializedJson = null;
                GC.Collect();
            }
            catch (Exception)
            {

            }
        }

        private async void UpdatePostCount()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                SecondLoadingBar.Visibility = Visibility.Visible;
                SecondLoadingBar.Maximum = follows.Count();
                SecondLoadingBar.IsIndeterminate = false;
                SecondLoadingBar.Value = 0;
            });
            for (int i = 0; i < follows.Count; i++)
            {
                string name = follows[i];
                if (name != "")
                {
                    string[] split = name.Split(':');
                    Tag tag = new Tag();
                    tag.name = split[0];
                    tag.old_post_count = Convert.ToInt32(split[1]);

                    var client = new RestClient(); // Client to handle Requests
                    var request = new RestRequest(RestSharp.Method.GET); // REST request
                    client.BaseUrl = new Uri("https://e621.net//tags.json?");

                    // Set the useragent for e621
                    client.UserAgent = "e610.NET/1.6(by EpsilonRho)";
                    request.AddQueryParameter("search[name_matches]", tag.name);
                    request.AddQueryParameter("search[order]", "count");
                    //request.AddQueryParameter("search[hide_empty]", "true");
                    request.AddQueryParameter("limit", "1");
                    // Send the request
                    var response = client.Execute(request);
                    // Deserialize the response
                    TagsHolder DeserializedJson = JsonConvert.DeserializeObject<TagsHolder>("{tags:" + response.Content + "}");

                    tag = DeserializedJson.tags[0];
                    tag.total_new = DeserializedJson.tags[0].post_count - tag.old_post_count;

                    if (tag.total_new > 0)
                    {
                        follows[follows.IndexOf(name)] = $"{tag.name}:{tag.post_count}";
                    }
                }
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    SecondLoadingBar.Value = i;
                });
                i++;
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                SecondLoadingBar.IsIndeterminate = true;
            });
            while (true)
            {
                try
                {
                    WriteFile();
                    break;
                }
                catch (Exception)
                {

                }
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                SecondLoadingBar.Visibility = Visibility.Collapsed;
            });
        }

        private void BackPage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            int tabIndex = MainPage.MainTabViewAccess.SelectedIndex;
            (MainPage.MainTabViewAccess.TabItems[tabIndex] as TabViewItem).Header = "Latest Posts";
            Bindings.Update();
            this.Frame.Navigate(typeof(PostsViewPage));
        }

        private void ImageGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                Post pick = (Post)e.ClickedItem;
                singlePost = pick;
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
                            bigpicture.Height = FollowsPage.ActualHeight - 65;
                            bigvideo.Width = FollowsPage.ActualWidth - 242;
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
                    //Thread tags = new Thread(PopulateTreeView);
                    //tags.Start();
                    Bindings.Update();
                }
            }
            catch (Exception)
            {

            }
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
                        //newMenuItem.Click += (s, e1) =>
                        //{
                        //    singlePool = newpool;
                        //    SearchBox.Text = "order:id pool:" + newpool.id;
                        //    ViewModel.ClearPosts();
                        //    Thread LoadThread = new Thread(LoadPosts);
                        //    LoadThread.Start(new LoadPostsArgs(SearchBox.Text, -1));
                        //    bigpicture.Source = "";
                        //    TopBar.Visibility = Visibility.Collapsed;
                        //    bigpicture.Visibility = Visibility.Collapsed;
                        //    bigvideo.Visibility = Visibility.Collapsed;
                        //    smallpicture.Visibility = Visibility.Collapsed;
                        //    ImageGrid.Visibility = Visibility.Visible;
                        //    CloseButton.Visibility = Visibility.Collapsed;
                        //    ImageLoadProgress.Visibility = Visibility.Collapsed;
                        //    PageText.Visibility = Visibility.Visible;
                        //    BackPage.Visibility = Visibility.Visible;
                        //    ForwardPage.Visibility = Visibility.Visible;
                        //    PageNumberText.Visibility = Visibility.Visible;
                        //    DescRect.Visibility = Visibility.Collapsed;
                        //    DescText.Text = "";
                        //    DownloadPool.Visibility = Visibility.Visible;
                        //    Thread TagClean = new Thread(TagsCleanup);
                        //    TagClean.Start();
                        //    Bindings.Update();
                        //};
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
            catch (Exception)
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

        private void FollowedTags_ItemClick(object sender, ItemClickEventArgs e)
        {
            string tag = (string)e.ClickedItem;
            if(tag == "All Tags")
            {
                SortedViewModel.ClearPosts();
                foreach (Post p in FullViewModel.Posts)
                {
                    SortedViewModel.AddPost(p);
                }
                return;
            }

            SortedViewModel.ClearPosts();
            foreach (Post p in FullViewModel.Posts)
            {
                if (p.tags.artist.Contains(tag) || p.tags.character.Contains(tag) || p.tags.copyright.Contains(tag) || p.tags.general.Contains(tag) || p.tags.invalid.Contains(tag) || p.tags.lore.Contains(tag) || p.tags.meta.Contains(tag) || p.tags.species.Contains(tag))
                {
                    SortedViewModel.AddPost(p);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            int tabIndex = MainPage.MainTabViewAccess.SelectedIndex;
            (MainPage.MainTabViewAccess.TabItems[tabIndex] as TabViewItem).Header = "Followed Tags";
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
            DescRect.Visibility = Visibility.Collapsed;
            DescText.Text = "";
            Bindings.Update();
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
            if (singlePost.index == SortedViewModel.Posts.Count - 1)
            {
                
            }
            else
            {
                try
                {
                    int index = SortedViewModel.Posts.IndexOf(singlePost);
                    Post pick = SortedViewModel.Posts[index + 1];
                    singlePost = pick;
                }
                catch (Exception)
                {

                }
            }
            if (SortedViewModel.Posts.Count == 0)
            {
                return;
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
            {
                bigvideo.Visibility = Visibility.Collapsed;
                bigpicture.Visibility = Visibility.Collapsed;
                smallpicture.Visibility = Visibility.Collapsed;
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
            if (SortedViewModel.Posts.IndexOf(singlePost) == 0 && pageCount != 1)
            {
                return;
            }
            else if (SortedViewModel.Posts.IndexOf(singlePost) == 0)
            {
                return;
            }
            else
            {
                int index = SortedViewModel.Posts.IndexOf(singlePost);
                Post pick = SortedViewModel.Posts[index - 1];
                singlePost = pick;
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
            {
                bigvideo.Visibility = Visibility.Collapsed;
                bigpicture.Visibility = Visibility.Collapsed;
                smallpicture.Visibility = Visibility.Collapsed;
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

        private void bigpicture_ImageExOpened(object sender, Microsoft.Toolkit.Uwp.UI.Controls.ImageExOpenedEventArgs e)
        {
            ImageLoadProgress.Visibility = Visibility.Collapsed;
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
                    bigpicture.Height = FollowsPage.ActualHeight - 65;
                    bigvideo.Width = FollowsPage.ActualWidth - 230;
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

        private async void AddButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (AddTagToolTip.IsOpen)
            {
                AddTagToolTip.IsOpen = false;
            }
            else
            {
                AddTagToolTip.IsOpen = true;
            }
        }

        private async void AddNewTag(object t)
        {
            try
            {
                string text = (string)t;
                string tagName = text.Replace("Posts: ", "");
                var client = new RestClient(); // Client to handle Requests
                var request = new RestRequest(RestSharp.Method.GET); // REST request
                client.BaseUrl = new Uri("https://e621.net//tags.json?");

                // Set the useragent for e621
                client.UserAgent = "e621 Follower/1.0(by EpsilonRho)";
                request.AddQueryParameter("search[name_matches]", tagName);
                request.AddQueryParameter("search[order]", "count");
                //request.AddQueryParameter("search[hide_empty]", "true");
                request.AddQueryParameter("limit", "1");
                // Send the request
                var response = client.Execute(request);
                // Deserialize the response
                TagsHolder DeserializedJson = JsonConvert.DeserializeObject<TagsHolder>("{tags:" + response.Content + "}");
                if (DeserializedJson.tags.Count == 0)
                {
                    ShowInfoPopup("Invalid tag");
                    return;
                }

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    FollowedTags.Items.Add(DeserializedJson.tags[0].name);
                    follows.Add($"{DeserializedJson.tags[0].name}:{DeserializedJson.tags[0].post_count}");
                });
                WriteFile();
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    LoadingBar.IsIndeterminate = true;
                    LoadingBar.Visibility = Visibility.Visible;
                });
                await Task.Run(async () =>
                {
                    var args = new LoadPostsArgs(tagName, -1);
                    args.limit = 30;
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        if (FollowedTags.SelectedItem != "All Tags")
                        {
                            args.paginationChar = 'a';
                        }
                    });
                    LoadPosts(args);
                });
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    LoadingBar.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception)
            {
                ShowInfoPopup("Cannot add tag");
            }
        }

        public async void WriteFile()
        {
            StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile saveFile = await storageFolder.CreateFileAsync("SaveFile.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
            string text = "";
            foreach (string tag in follows)
            {
                if (tag != "")
                {
                    text += $"{tag}\n";
                }
            }

            await Windows.Storage.FileIO.WriteTextAsync(saveFile, text);
        }

        private void AdNewbutton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var found = from str in follows
                        where str.Split(':')[0] == AddTagTextBox.Text
                        select str;
            if (AddTagTextBox.Text != "" && found.Count() == 0)
            {
                Thread thread = new Thread(AddNewTag);
                thread.Start(AddTagTextBox.Text);
            }
            AddTagTextBox.Text = "";
            AddTagToolTip.IsOpen = false;
        }

        private void FollowedTags_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            try
            {
                ClickedTag = (e.OriginalSource as FrameworkElement).DataContext as string;
                MenuFlyout myFlyout = new MenuFlyout();
                MenuFlyoutItem firstItem = new MenuFlyoutItem { Text = "Open in new tab" };
                firstItem.Click += new RoutedEventHandler(OpenInNewTab);
                myFlyout.Items.Add(firstItem);
                MenuFlyoutItem secondItem = new MenuFlyoutItem { Text = "Remove tag" };
                secondItem.Click += new RoutedEventHandler(RemoveTag);
                myFlyout.Items.Add(secondItem);
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

        private void RemoveTag(object sender, RoutedEventArgs e)
        {
            try
            {
                var found = from str in follows
                            where str.Split(':')[0] == ClickedTag
                            select str;

                var list = found.ToList();
                var tag = list[0].Split(':')[0];
                FollowedTags.Items.Remove(tag);

                follows.Remove(list[0]);

                Task.Run(async () =>
                {
                    for (int i = 0; i < FullViewModel.Posts.Count(); i++)
                    {
                        Post p = FullViewModel.Posts[i];
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            if (p.tags.artist.Contains(tag) || p.tags.character.Contains(tag) || p.tags.copyright.Contains(tag) || p.tags.general.Contains(tag) || p.tags.invalid.Contains(tag) || p.tags.lore.Contains(tag) || p.tags.meta.Contains(tag) || p.tags.species.Contains(tag))
                            {
                                FullViewModel.Posts.Remove(p);
                                try
                                {
                                    SortedViewModel.Posts.Remove(p);
                                }
                                catch (Exception)
                                {

                                }
                                i--;
                            }
                        });
                    }
                });

                WriteFile();
            }
            catch (Exception)
            {

            }
        }
    }
}
