﻿using e610.NET;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            //if (GlobalVars.newSearch == true)
            //{
            //    ViewModel = new PostsViewModel();
            //    Thread LoadThread = new Thread(LoadPosts);
            //    LoadThread.Start(new LoadPostsArgs(SearchBox.Text, -1));
            //    GlobalVars.newSearch = false;
            //}
            //else if(GlobalVars.ViewModel.Posts.Count() > 0)
            //{
            //    ViewModel = new PostsViewModel();
            //    Thread pop = new Thread(threadedRepopulate);
            //    pop.Start();
            //}
            //else
            //{
            //    ViewModel = new PostsViewModel();
            //}
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

            GlobalVars.ShowComments = (bool)localSettings.Values["comments"];
            CommentSwitch.IsOn = GlobalVars.ShowComments;

            GlobalVars.MuteVolume = (bool)localSettings.Values["volume"];
            VolumeSwitch.IsOn = GlobalVars.MuteVolume;
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
                int count = 0;
                foreach (Post p in DeserializedJson.posts)
                {
                    // If the url is null the post is blacklisted
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
                }
                DeserializedJson = null;

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

        // SideBar Functions //
            // Button Functions //
        private void SearchButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.ClearPosts();
            Thread LoadThread = new Thread(LoadPosts);
            LoadThread.Start(new LoadPostsArgs(SearchBox.Text, -1));
            pageCount = 1;
            Bindings.Update();
            GlobalVars.postCount = (int)PostCountSlider.Value;
            if(PageText.Visibility == Visibility.Collapsed)
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
                Thread TagClean = new Thread(TagsCleanup);
                TagClean.Start();
                Bindings.Update();
            }
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
                if (singlePost.pools.Count > 0)
                {
                    // Added the pools to the listview
                    Thread poolThread = new Thread(new ThreadStart(poolPopulate));
                    poolThread.Start();
                    poolThread = null;
                }
                if (pick.file.url != "https://ambisure.com/wp-content/uploads/2019/03/SHOCK1-1030x724.png")
                {
                    ImageLoadProgress.Visibility = Visibility.Visible;
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
                    TopBar.Visibility = Visibility.Visible;
                    ImageGrid.Visibility = Visibility.Collapsed;
                    CloseButton.Visibility = Visibility.Visible;
                    PageText.Visibility = Visibility.Collapsed;
                    BackPage.Visibility = Visibility.Collapsed;
                    ForwardPage.Visibility = Visibility.Collapsed;
                    PageNumberText.Visibility = Visibility.Collapsed;
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

        // Touch Functions //
        private void LeftSwipeItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
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
        private void RightSwipeItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
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
            MenuFlyoutItem firstItem = new MenuFlyoutItem { Text = "Save Post" };
            firstItem.Click += new RoutedEventHandler(StartSaveAsync);
            myFlyout.Items.Add(firstItem);
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
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ImageLoadProgress.Visibility = Visibility.Visible;
                });
                HttpClient client = new HttpClient(); // Create HttpClient
                byte[] buffer = await client.GetByteArrayAsync(singlePost.file.url); // Download file
                StorageFile file = null;
                try
                {
                    file = await Windows.Storage.DownloadsFolder.CreateFileAsync(singlePost.file.md5 + "." + singlePost.file.ext);
                }
                catch (Exception)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        ImageLoadProgress.Visibility = Visibility.Collapsed;
                        InfoPopup.Title = "Saving Error";
                        InfoPopup.Message = "File Already Exists";
                        InfoPopup.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning;
                        InfoPopup.IsOpen = true;
                    });
                    Thread closePopup = new Thread(CloseInfoPopup);
                    closePopup.Start();
                    return;
                }
                using (Stream stream = await file.OpenStreamForWriteAsync())
                {
                    stream.Write(buffer, 0, buffer.Length); // Save
                }
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ImageLoadProgress.Visibility = Visibility.Collapsed;
                    InfoPopup.Title = "Post Saved";
                    InfoPopup.Message = "File Saved to downlaods";
                    InfoPopup.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success;
                    InfoPopup.IsOpen = true;
                });
                Thread ClosePopup = new Thread(CloseInfoPopup);
                ClosePopup.Start();
            }
            catch (Exception e)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ImageLoadProgress.Visibility = Visibility.Collapsed;
                    InfoPopup.Title = "Saving Error";
                    InfoPopup.Message = e.Message;
                    InfoPopup.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
                    InfoPopup.IsOpen = true;
                });
                Thread ClosePopup = new Thread(CloseInfoPopup);
                ClosePopup.Start();
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
            //try
            //{
            //    string ClickedItem = (e.OriginalSource as FrameworkElement).DataContext as string;
            //    if (SearchBox.Text.Contains(ClickedItem))
            //    {
            //        if (SearchBox.Text.Contains("-" + ClickedItem))
            //        {
            //            SearchBox.Text = SearchBox.Text.Replace("-"+ClickedItem, "");
            //        }
            //        else 
            //        {
            //            SearchBox.Text = SearchBox.Text.Replace(ClickedItem, "-" + ClickedItem);
            //        }
            //    }
            //    else
            //    {
            //        if (SearchBox.Text.Count() > 0)
            //        {
            //            if (SearchBox.Text.Count() == SearchBox.Text.LastIndexOf(' '))
            //            {
            //                SearchBox.Text.Remove(SearchBox.Text.LastIndexOf(' '));
            //            }
            //            SearchBox.Text += " -" + ClickedItem;
            //        }
            //        else
            //        {
            //            SearchBox.Text += "-" + ClickedItem;
            //        }
            //    }
            //}
            //catch (Exception)
            //{

            //}
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
                Thread LoadThread = new Thread(LoadPosts);
                pageCount++;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                {
                    ViewModel.ClearPosts();
                    LoadThread.Start(new LoadPostsArgs(SearchBox.Text, pageCount));
                });
                while (ViewModel.Posts.Count == 0)
                {

                }
                singlePost = ViewModel.Posts[0];
            }
            else
            {
                Post pick = ViewModel.Posts[singlePost.index + 1];
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
                Thread tags = new Thread(PopulateTreeView);
                tags.Start();
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
                Thread tags = new Thread(PopulateTreeView);
                tags.Start();
            }
        }
            // Exit Functions //
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
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
            Thread TagClean = new Thread(TagsCleanup);
            TagClean.Start();
            Bindings.Update();
        }

        // Info and Debug //
        private async void CloseInfoPopup()
        {
            Thread.Sleep(3000);
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                InfoPopup.IsOpen = false;
            });
        }

        // Helper Functions //
        private void bigpicture_ImageExOpened(object sender, Microsoft.Toolkit.Uwp.UI.Controls.ImageExOpenedEventArgs e)
        {
            ImageLoadProgress.Visibility = Visibility.Collapsed;
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
                        var newMenuItem = new MenuFlyoutItem();
                        newMenuItem.Text = newpool.name;
                        newMenuItem.Click += (s, e1) =>
                        {
                            GlobalVars.newPool = true;
                            this.Frame.Navigate(typeof(PoolView), newpool, new DrillInNavigationTransitionInfo());
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
                client.UserAgent = "e610.NET/1.3(by EpsilonRho)";
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

        // Settings //
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
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





        private void ImageSize_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void PoolButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(PoolButton);
        }

        private void VoteUpButton_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void VoteDownButton_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void FavoiteButton_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void ImageMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }


        private void Account_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AccountsPage), null, new DrillInNavigationTransitionInfo());
        }



    }
}
