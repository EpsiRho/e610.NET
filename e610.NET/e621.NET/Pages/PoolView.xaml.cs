using e610.NET;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace e610.NET.Pages
{
    public sealed partial class PoolView : Page
    {
        // Page Vars //
        public PostsViewModel ViewModel { get; set; }
        public Root DownloadPostHolder;
        private Pool CurrentPool;
        public int pageCount;
        public bool stopDownload;
        Thread saveThread;

        // Page Loading //
        public PoolView()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            CurrentPool = (Pool)e.Parameter;
            ViewModel = new PostsViewModel();
            PostCountSlider.Value = GlobalVars.postCount;
            SearchBox.Text = GlobalVars.searchText;
            PoolTitle.Text = CurrentPool.name.Replace("_", " ") + " | " + CurrentPool.post_count + " Posts |";
            PoolDesc.Text = CurrentPool.description;
            if(PoolDesc.Text != "")
            {
                DescToggle.Visibility = Visibility.Visible;
            }
            if (GlobalVars.newPool == true)
            {
                ViewModel = new PostsViewModel();
                Thread LoadThread = new Thread(LoadPoolPosts);
                LoadThread.Start(new LoadPostsArgs(CurrentPool.id.ToString(), -1));
                GlobalVars.newPool = false;
            }
            else if (GlobalVars.PoolViewModel.Posts.Count() > 0)
            {
                ViewModel = new PostsViewModel();
                Thread pop = new Thread(threadedRepopulate);
                pop.Start();
            }
            pageCount = 1;
            stopDownload = false;
            saveThread = new Thread(new ThreadStart(SaveImage));
        }

        // Load Posts //
        private void LoadPoolPostsForDownload(int page)
        {
            // Function Vars
            var client = new RestClient(); // Client to handle Requests
            double limit = 75; // Post Limit
            var request = new RestRequest(RestSharp.Method.GET); // REST request

            // Set Endpoint
            // TODO: Switching between e621 - gelbooru - r34 - etc
            client.BaseUrl = new Uri("https://e621.net/posts.json?");

            // Set the useragent for e621
            client.UserAgent = "e610.NET/1.3(by EpsilonRho)";

            // If user is logged in set login parameters into request
            if (GlobalVars.Username != "" && GlobalVars.APIKey != "")
            {
                request.AddQueryParameter("login", GlobalVars.Username);
                request.AddQueryParameter("api_key", GlobalVars.APIKey);
            }

            // Set parameters for tags and post limit
            request.AddQueryParameter("tags", GlobalVars.Rating + " order:id pool:" + CurrentPool.id);
            request.AddQueryParameter("tags", "order:id pool:" + CurrentPool.id);
            request.AddQueryParameter("limit", limit.ToString());

            request.AddQueryParameter("page", page.ToString());

            // Send the request
            var response = client.Execute(request);

            // Deserialize the response
            DownloadPostHolder = JsonConvert.DeserializeObject<Root>(response.Content);
        }
        private async void LoadPoolPosts(object t)
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
                limit = PostCountSlider.Value;
            });

            // Set Endpoint
            // TODO: Switching between e621 - gelbooru - r34 - etc
            client.BaseUrl = new Uri("https://e621.net/posts.json?");

            // Set the useragent for e621
            client.UserAgent = "e610.NET/1.3(by EpsilonRho)";

            // If user is logged in set login parameters into request
            if (GlobalVars.Username != "" && GlobalVars.APIKey != "")
            {
                request.AddQueryParameter("login", GlobalVars.Username);
                request.AddQueryParameter("api_key", GlobalVars.APIKey);
            }

            // Set parameters for tags and post limit
            request.AddQueryParameter("tags", GlobalVars.Rating + " order:id pool:" + args.tags);
            
            request.AddQueryParameter("limit", limit.ToString());

            // If the lastid is not -1, use the last post id to move forward or back a page
            if (args.page != -1)
            {
                request.AddQueryParameter("page", args.page.ToString());
            }

            // Send the request
            var response = client.Execute(request);

            // Deserialize the response
            Root DeserializedJson = JsonConvert.DeserializeObject<Root>(response.Content);
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
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
                        p.preview.url = "https://ambisure.com/wp-content/uploads/2019/03/SHOCK1-1030x724.png";
                        p.file.url = "https://ambisure.com/wp-content/uploads/2019/03/SHOCK1-1030x724.png";
                        ViewModel.AddPost(p);
                        //LoadingBar.Value++;
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
        public async void threadedRepopulate()
        {
            Thread.Sleep(100);
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                LoadingBar.IsIndeterminate = false;
                LoadingBar.Maximum = GlobalVars.PoolViewModel.Posts.Count;
                LoadingBar.Value = 0;
                LoadingBar.Visibility = Visibility.Visible;
            });

            foreach (Post p in GlobalVars.PoolViewModel.Posts)
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
                GlobalVars.PoolViewModel.Posts.Clear();
            });
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
        private void ForwardPage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ViewModel.Posts.Count > 0)
            {
                Thread LoadThread = new Thread(LoadPoolPosts);
                //Post p = ViewModel.Posts.Last();
                ViewModel.ClearPosts();
                pageCount++;
                LoadThread.Start(new LoadPostsArgs(CurrentPool.id.ToString(), pageCount));
                Bindings.Update();
            }
        }
        private void BackPage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (pageCount != 1)
            {
                Thread LoadThread = new Thread(LoadPoolPosts);
                //Post p = ViewModel.Posts.First();
                ViewModel.ClearPosts();
                pageCount--;
                LoadThread.Start(new LoadPostsArgs(CurrentPool.id.ToString(), pageCount));
                Bindings.Update();
            }
        }

        private void ImageGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                Post pick = (Post)e.ClickedItem;
                if (pick.file.url != "https://ambisure.com/wp-content/uploads/2019/03/SHOCK1-1030x724.png")
                {
                    GlobalVars.PoolViewModel = ViewModel;
                    GlobalVars.pageCount = pageCount;
                    GlobalVars.searchText = SearchBox.Text;
                    GlobalVars.postCount = (int)PostCountSlider.Value;
                    GlobalVars.PoolName = CurrentPool;
                    this.Frame.Navigate(typeof(SinglePostView), pick, new DrillInNavigationTransitionInfo());
                }
            }
            catch (Exception)
            {

            }
        }

        private void StartSaveAsync()
        {
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

        private async void SaveImage()
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
                string foldername = CurrentPool.name.Replace("_", " ");
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
                    InfoPopup.Title = "Saving Error";
                    InfoPopup.Message = "File Already Exists";
                    InfoPopup.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning;
                    InfoPopup.IsOpen = true;
                });
                return;
            }
            LoadPoolPostsForDownload(downloadpage);
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                DownloadProgress.IsIndeterminate = false;
                DownloadProgress.Maximum = CurrentPool.post_count;
                DownloadProgress.Value = 0;
                FilesText.Text = postCount.ToString() + "/" + CurrentPool.post_count.ToString();
            });

            while(postCount < CurrentPool.post_count) 
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
                          FilesText.Text = postCount.ToString() + "/" + CurrentPool.post_count.ToString();
                      });
                }
                catch(Exception e)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        DownloadProgress.Visibility = Visibility.Collapsed;
                        InfoPopup.Title = "Saving Error";
                        InfoPopup.Message = e.Message;
                        InfoPopup.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
                        InfoPopup.IsOpen = true;
                    });
                }
                if(viewPosts == DownloadPostHolder.posts.Count)
                {
                    downloadpage++;
                    LoadPoolPostsForDownload(downloadpage);
                    viewPosts = 0;
                }
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                DownloadProgress.Visibility = Visibility.Collapsed;
                NormalText.Visibility = Visibility.Visible;
                FilesText.Text = "";
                InfoPopup.Title = "Post Saved";
                InfoPopup.Message = "File Saved to downlaods";
                InfoPopup.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success;
                InfoPopup.IsOpen = true;
            });
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

        private void DescToggle_Click(object sender, RoutedEventArgs e)
        {
            if(PoolDesc.Visibility == Visibility.Collapsed)
            {
                DescToggle.Content = "Hide Description";
                PoolDesc.Visibility = Visibility.Visible;
            }
            else
            {
                DescToggle.Content = "Show Description";
                PoolDesc.Visibility = Visibility.Collapsed;
            }
        }
    }
}
