using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using RestSharp;
using Newtonsoft.Json;
using System.Threading;
using Windows.UI.Xaml.Media.Imaging;
using System.Net.Http;
using Windows.UI.ViewManagement;
using Windows.Graphics.Display;
using System.Text.RegularExpressions;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace e610.NET
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Global Vars //
        PostsViewModel ViewModel;
        FollowsViewModel followsViewModel;
        bool KeepQueueAlive;
        List<Tag> tagQueue;
        Post singlePost;
        bool IsPopupOpen;
        List<string> follows;
        Tag RightTappedItem;
        string clickedTag;
        string Username;
        string ApiKey;
        // Page Load //
        public MainPage()
        {
            this.InitializeComponent();
            ViewModel = new PostsViewModel();
            followsViewModel = new FollowsViewModel();
            Window.Current.SizeChanged += OnWindowSizeChanged;
            GetSettings();
            Thread t = new Thread(LoadSaveFile);
            t.Start();
        }
        private void GetSettings()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Username = (string)localSettings.Values["username"];
            if (Username == null)
            {
                Username = "";
                localSettings.Values["username"] = "";
            }
            UsernameBox.Text = Username;
            ApiKey = (string)localSettings.Values["apikey"];
            if (ApiKey == null)
            {
                ApiKey = "";
                localSettings.Values["apikey"] = "";
            }
            ApiKeyBox.Text = ApiKey;
            // TO-DO: Fix stupid Settings bug
        }
        private async void LoadSaveFile()
        {
            try
            {
                StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile saveFile = await storageFolder.CreateFileAsync("SaveFile.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
                string text = await Windows.Storage.FileIO.ReadTextAsync(saveFile);
                follows = new List<string>(text.Split('\n'));
                List<string> tags = new List<string>();
                foreach (string s in follows)
                {
                    if (s != "")
                    {
                        string[] split = s.Split(":");
                        string arg = "~" + split[0];
                        tags.Add(arg);
                    }
                }
                Thread loadRecent = new Thread(LoadRecentPosts);
                loadRecent.Start(tags);
                KeepQueueAlive = true;
                tagQueue = new List<Tag>();
                Thread TagQueue = new Thread(getTagInfo);
                TagQueue.Start();
                foreach (string name in follows)
                {
                    if (name != "")
                    {
                        string[] split = name.Split(':');
                        Tag tag = new Tag();
                        tag.name = split[0];
                        tag.old_post_count = Convert.ToInt32(split[1]);
                        tag.Show_Count = Visibility.Collapsed;
                        tag.Show_Progress = Visibility.Visible;

                        tagQueue.Add(tag);

                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            followsViewModel.AddTag(tag);
                        });
                    }
                }
                KeepQueueAlive = false;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    FollowingProgressRing.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception e)
            {

            }
        }
        private async void getTagInfo()
        {
            while (KeepQueueAlive || tagQueue.Count() > 0)
            {
                try
                {
                    Tag tag = tagQueue[0];
                    var client = new RestClient(); // Client to handle Requests
                    var request = new RestRequest(RestSharp.Method.GET); // REST request
                    client.BaseUrl = new Uri("https://e621.net//tags.json?");

                    // Set the useragent for e621
                    client.UserAgent = "e621 Follower/1.0(by EpsilonRho)";
                    request.AddQueryParameter("search[name_matches]", tagQueue[0].name);
                    request.AddQueryParameter("search[order]", "count");
                    //request.AddQueryParameter("search[hide_empty]", "true");
                    request.AddQueryParameter("limit", "1");
                    // Send the request
                    var response = client.Execute(request);
                    // Deserialize the response
                    TagList DeserializedJson = JsonConvert.DeserializeObject<TagList>("{tags:" + response.Content + "}");

                    tagQueue[0].total_new = DeserializedJson.tags[0].post_count - tagQueue[0].old_post_count;
                    tagQueue[0].post_count = DeserializedJson.tags[0].post_count;

                    if (tagQueue[0].total_new > 0)
                    {
                        try
                        {
                            tagQueue[0].Show_Count = Visibility.Visible;
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                    else
                    {
                        tagQueue[0].Show_Count = Visibility.Collapsed;
                    }
                    tagQueue[0].Show_Progress = Visibility.Collapsed;

                    int index = followsViewModel.Tags.IndexOf(tag);
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        followsViewModel.Tags[index] = tagQueue[0];
                    });
                    tagQueue.RemoveAt(0);
                }
                catch (Exception)
                {

                }
            }
        }

        // Click Functions //
        private void PostsView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Post p = (Post)e.ClickedItem;
            singlePost = p;
            RecentProgressRing.Visibility = Visibility.Visible;
            FullImage.Width = double.NaN;
            FullImage.Height = page.ActualHeight - 68;
            FullVideo.Width = double.NaN;
            FullVideo.Height = page.ActualHeight - 68;
            ImageGrid.Width = double.NaN;
            ImageGrid.Height = page.ActualHeight - 68;
            if (p.file.ext == "webm")
            {
                FullVideo.Source = new Uri(p.file.url, UriKind.Absolute);
                FullVideo.Visibility = Visibility.Visible;
                ImageBG.Visibility = Visibility.Visible;
                RecentProgressRing.Visibility = Visibility.Collapsed;
            }
            else if (p.file.ext == "swf")
            {
                ShowInfoPopup(".swf File type unsupported");
                return;
            }
            else
            {
                FullImage.Source = new BitmapImage(new Uri(p.file.url, UriKind.Absolute));
                FullImage.Visibility = Visibility.Visible;
                ImageBG.Visibility = Visibility.Visible;
            }
            CloseButton.Visibility = Visibility.Visible;
            Thread tags = new Thread(LoadTags);
            tags.Start();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseButton.Visibility = Visibility.Collapsed;
            ImageBG.Visibility = Visibility.Collapsed;
            RecentProgressRing.Visibility = Visibility.Collapsed;
            FullImage.Source = null;
            FullVideo.Source = null;
            FullImage.Visibility = Visibility.Collapsed;
            FullVideo.Visibility = Visibility.Collapsed;
            ArtistsTags.Items.Clear();
            CopyrightsTags.Items.Clear();
            CharactersTags.Items.Clear();
            SpeciesTags.Items.Clear();
            GeneralTags.Items.Clear();
            MetaTags.Items.Clear();
            LoreTags.Items.Clear();
            InvalidTags.Items.Clear();
        }
        private void FollowList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (RecentProgressRing.Visibility == Visibility.Collapsed)
            {
                ViewModel.ClearPosts();
                RecentBackButton.Visibility = Visibility.Visible;
                Tag clickedTag = (Tag)e.ClickedItem;
                PostsTitle.Text = "Posts: " + clickedTag.name;
                Thread t = new Thread(LoadNewPosts);
                t.Start(clickedTag);
                if (clickedTag.total_new > 0)
                {
                    int index = followsViewModel.Tags.IndexOf(clickedTag);
                    clickedTag.total_new = 0;
                    clickedTag.Show_Count = Visibility.Collapsed;
                    followsViewModel.Tags[index] = clickedTag;
                    Thread t2 = new Thread(WriteFile);
                    t2.Start();
                    Bindings.Update();
                }
            }
        }
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            //ShowInfoPopup("Test InfoPopup, Settings coming soon");
            //RegisterButton_Click();
            SettingsDialog.ShowAsync();
        }
        private void FullImage_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            MenuFlyout myFlyout = new MenuFlyout();
            MenuFlyoutItem firstItem = new MenuFlyoutItem { Text = "Save Post" };
            firstItem.Click += new RoutedEventHandler(StartSaveAsync);
            myFlyout.Items.Add(firstItem);
            myFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }
        private void FullVideo_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            MenuFlyout myFlyout = new MenuFlyout();
            MenuFlyoutItem firstItem = new MenuFlyoutItem { Text = "Save Post" };
            firstItem.Click += new RoutedEventHandler(StartSaveAsync);
            myFlyout.Items.Add(firstItem);
            myFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }
        private void RecentBackButton_Click(object sender, RoutedEventArgs e)
        {
            if (RecentProgressRing.Visibility == Visibility.Collapsed)
            {
                PostsTitle.Text = "Recent Posts";
                ViewModel.ClearPosts();
                RecentBackButton.Visibility = Visibility.Collapsed;
                AddTagButton.Visibility = Visibility.Collapsed;
                List<string> tags = new List<string>();
                foreach (string s in follows)
                {
                    if (s != "")
                    {
                        string[] split = s.Split(":");
                        string arg = "~" + split[0];
                        tags.Add(arg);
                    }
                }
                Thread loadRecent = new Thread(LoadRecentPosts);
                loadRecent.Start(tags);
            };
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Visibility == Visibility.Visible && SearchBox.Text == "")
            {
                SearchBox.Visibility = Visibility.Collapsed;
            }
            else if(SearchBox.Visibility == Visibility.Visible && SearchBox.Text != "")
            {
                ViewModel.ClearPosts();
                RecentBackButton.Visibility = Visibility.Visible;
                AddTagButton.Visibility = Visibility.Collapsed;
                CloseButton.Visibility = Visibility.Collapsed;
                ImageBG.Visibility = Visibility.Collapsed;
                RecentProgressRing.Visibility = Visibility.Collapsed;
                FullImage.Source = null;
                FullVideo.Source = null;
                FullImage.Visibility = Visibility.Collapsed;
                FullVideo.Visibility = Visibility.Collapsed;
                ArtistsTags.Items.Clear();
                CopyrightsTags.Items.Clear();
                CharactersTags.Items.Clear();
                SpeciesTags.Items.Clear();
                GeneralTags.Items.Clear();
                MetaTags.Items.Clear();
                LoreTags.Items.Clear();
                InvalidTags.Items.Clear();
                PostsTitle.Text = "Posts: " + SearchBox.Text;
                Thread followButton = new Thread(ShowFollowButton);
                followButton.Start();
                List<string> tags = new List<string>(SearchBox.Text.Split(" "));
                Thread t = new Thread(LoadRecentPosts);
                t.Start(tags);
                SearchBox.Text = "";
                SearchBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                SearchBox.Visibility = Visibility.Visible;
            }
        }
        private void FollowList_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            RightTappedItem = (e.OriginalSource as FrameworkElement).DataContext as Tag;
            if (RightTappedItem != null)
            {
                MenuFlyout myFlyout = new MenuFlyout();
                MenuFlyoutItem firstItem = new MenuFlyoutItem { Text = "Unfollow" };
                firstItem.Click += new RoutedEventHandler(Unfollowtag);
                myFlyout.Items.Add(firstItem);
                myFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
            }
        }
        private void AddTagButton_Click(object sender, RoutedEventArgs e)
        {
            Thread Add = new Thread(AddNewTag);
            Add.Start(PostsTitle.Text);
            AddTagButton.Visibility = Visibility.Collapsed;
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uint x = Convert.ToUInt32(NotificationTime.Text);
                if(x < 15)
                {
                    ShowInfoPopup("Time must be higher than 15 minutes");
                    return;
                }
                Thread t = new Thread(RegisterNotification);
                t.Start();
            }
            catch(Exception)
            {
                ShowInfoPopup("Error: invalid time");
            }
        }
        private void DeregisterButton_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(DeregisterNotification);
            t.Start();
        }
        private void Tags_ItemClick(object sender, ItemClickEventArgs e)
        {
            clickedTag = (string)e.ClickedItem;
            MenuFlyout myFlyout = new MenuFlyout();
            bool found = false;
            foreach (Tag t in followsViewModel.Tags)
            {
                if (t.name.ToLower() == clickedTag.ToLower())
                {
                    found = true;
                }
            }
            if (!found)
            {
                MenuFlyoutItem firstItem = new MenuFlyoutItem { Text = "Follow Tag" };
                firstItem.Click += new RoutedEventHandler(FollowTagClick);
                myFlyout.Items.Add(firstItem);
            }

            MenuFlyoutItem secondItem = new MenuFlyoutItem { Text = "Search Tag" };
            secondItem.Click += new RoutedEventHandler(SearchTagClick);
            myFlyout.Items.Add(secondItem);
            var point = CoreWindow.GetForCurrentThread().PointerPosition;
            var x = point.X - Window.Current.Bounds.X;
            var y = point.Y - Window.Current.Bounds.Y;
            myFlyout.ShowAt(mainGrid, new Point(x,y));
            ArtistsTags.SelectedItem = null;
            CopyrightsTags.SelectedItem = null;
            CharactersTags.SelectedItem = null;
            SpeciesTags.SelectedItem = null;
            GeneralTags.SelectedItem = null;
            MetaTags.SelectedItem = null;
            LoreTags.SelectedItem = null;
            InvalidTags.SelectedItem = null;
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

        // Load Posts //
        private async void LoadRecentPosts(object n)
        {
            List<string> names = (List<string>)n;
            string arg = "";
            foreach (string name in names)
            {
                if (name != "")
                {
                    arg += name + " ";
                }
            }

            var client = new RestClient(); // Client to handle Requests
            var request = new RestRequest(RestSharp.Method.GET); // REST request

            // TODO: Switching between e621 - gelbooru - r34 - etc
            client.BaseUrl = new Uri("https://e621.net/posts.json?");

            // Set the useragent for e621
            client.UserAgent = "e621 Follower/1.0(by EpsilonRho)";

            // Get Login Code in Settings Later
            //if (GlobalVars.Username != "" && GlobalVars.APIKey != "")
            //{
            //    request.AddQueryParameter("login", "EpsilonRho");
            //    request.AddQueryParameter("api_key", "o4AwL5uw1QbuMvCQvfykQzsr");
            //}
            request.AddQueryParameter("tags", arg);
            request.AddQueryParameter("limit", "75");

            // Send the request
            var response = client.Execute(request);

            // Deserialize the response
            Root DeserializedJson = JsonConvert.DeserializeObject<Root>(response.Content);

            try
            {
                if (DeserializedJson.posts.Count() == 0)
                {
                    ShowInfoPopup("No Posts Found");
                }

                foreach (Post p in DeserializedJson.posts)
                {
                    // If the url is null the post is blacklisted
                    if (p.preview.url != null)
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            ViewModel.AddPost(p);
                        });
                    }
                    else
                    {
                        // this post was blacklsited so show funny picture
                        
                    }
                }
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    RecentProgressRing.Visibility = Visibility.Collapsed;
                });
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
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

                var taskName = "Notifier";

                foreach (var t in BackgroundTaskRegistration.AllTasks)
                {
                    if (t.Value.Name == taskName)
                    {
                        t.Value.Unregister(true);
                    }
                }

                var builder = new BackgroundTaskBuilder();

                builder.Name = taskName;
                builder.TaskEntryPoint = "Background_Notifier.Notifier";
                builder.SetTrigger(timeTrigger);
                builder.AddCondition(new SystemCondition(SystemConditionType.UserNotPresent));
                BackgroundTaskRegistration task = builder.Register();
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    NotificationStatus.IsChecked = true;
                    NotificationStatus.Content = "Notifications On";
                });
            }
            catch(Exception)
            {
                ShowInfoPopup("Couldn't Register Background Task");
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

        // Helper Functions //
        public static Size GetCurrentDisplaySize()
        {
            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            var size = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);
            return size;
        }
        private void OnWindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Size window = GetCurrentDisplaySize();
            try
            {
                if (FullImage.Visibility == Visibility.Visible || FullVideo.Visibility == Visibility.Visible)
                {
                    FullImage.Width = double.NaN;
                    FullImage.Height = window.Height - 68;
                    FullVideo.Width = double.NaN;
                    FullVideo.Height = window.Height - 68;
                    ImageGrid.Width = double.NaN;
                    ImageGrid.Height = window.Height - 68;
                }
            }
            catch (Exception)
            {

            }
        }
        private void OnCompleted(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            
        }
        private void FullImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            RecentProgressRing.Visibility = Visibility.Collapsed;
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

        // Background Functions //
        private async void LoadNewPosts(object t)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                RecentProgressRing.Visibility = Visibility.Visible;
            });
            Tag clickedTag = (Tag)t;
            List<string> tags = new List<string>();
            tags.Add(clickedTag.name);
            LoadRecentPosts(tags);
        }
        private async void ShowInfoPopup(string info)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                InfoPopupText.Text = info;
                InfoPopup.Opacity = 100;
                InfoPopupText.Opacity = 100;
                InfoPopup.Visibility = Visibility.Visible;
            });
            IsPopupOpen = true;
            Thread t = new Thread(InfoPopupTimer);
            t.Start();
        }
        private void Unfollowtag(object sender, RoutedEventArgs e)
        {
            follows.RemoveAt(followsViewModel.Tags.IndexOf(RightTappedItem));
            followsViewModel.Tags.RemoveAt(followsViewModel.Tags.IndexOf(RightTappedItem)); 
            WriteFile();
            PostsTitle.Text = "Posts: Followed";
            ViewModel.ClearPosts();
            RecentBackButton.Visibility = Visibility.Collapsed;
            AddTagButton.Visibility = Visibility.Collapsed;
            Thread loadRecent = new Thread(LoadRecentPosts);
            loadRecent.Start(follows);
        }
        private async void ShowFollowButton()
        {
            string tagName = "";
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                tagName = PostsTitle.Text.Replace("Posts: ", "");
            });
            bool found = false;
            foreach(Tag t in followsViewModel.Tags)
            {
                if(t.name.ToLower() == tagName.ToLower())
                {
                    found = true;
                }
            }
            if (!found)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    AddTagButton.Visibility = Visibility.Visible;
                });
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
                TagList DeserializedJson = JsonConvert.DeserializeObject<TagList>("{tags:" + response.Content + "}");
                DeserializedJson.tags[0].Show_Count = Visibility.Collapsed;
                DeserializedJson.tags[0].old_post_count = DeserializedJson.tags[0].post_count;
                DeserializedJson.tags[0].total_new = 0;
                DeserializedJson.tags[0].Show_Progress = Visibility.Collapsed;

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    followsViewModel.AddTag(DeserializedJson.tags[0]);

                });
                WriteFile();
                follows.Add(tagName);
            }
            catch (Exception)
            {
                ShowInfoPopup("Cannot add tag");
            }
        }
        private void LoadTags()
        {
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
        private void FollowTagClick(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(AddNewTag);
            t.Start(clickedTag);
        }
        private void SearchTagClick(object sender, RoutedEventArgs e)
        {
            ViewModel.ClearPosts();
            RecentBackButton.Visibility = Visibility.Visible;
            AddTagButton.Visibility = Visibility.Collapsed;
            CloseButton.Visibility = Visibility.Collapsed;
            ImageBG.Visibility = Visibility.Collapsed;
            RecentProgressRing.Visibility = Visibility.Collapsed;
            FullImage.Source = null;
            FullVideo.Source = null;
            FullImage.Visibility = Visibility.Collapsed;
            FullVideo.Visibility = Visibility.Collapsed;
            ArtistsTags.Items.Clear();
            CopyrightsTags.Items.Clear();
            CharactersTags.Items.Clear();
            SpeciesTags.Items.Clear();
            GeneralTags.Items.Clear();
            MetaTags.Items.Clear();
            LoreTags.Items.Clear();
            InvalidTags.Items.Clear();
            PostsTitle.Text = "Posts: " + clickedTag;
            Thread followButton = new Thread(ShowFollowButton);
            followButton.Start();
            List<string> tags = new List<string>();
            tags.Add(clickedTag);
            Thread t = new Thread(LoadRecentPosts);
            t.Start(tags);
            SearchBox.Text = "";
            SearchBox.Visibility = Visibility.Collapsed;
        }

        // File System Functions //
        private async void WriteFile()
        {
            StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile saveFile = await storageFolder.CreateFileAsync("SaveFile.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
            string text = "";
            foreach(Tag tag in followsViewModel.Tags)
            {
                text += tag.name + ":" + tag.post_count.ToString() + "\n";
            }

            await Windows.Storage.FileIO.WriteTextAsync(saveFile, text);
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
                    RecentProgressRing.Visibility = Visibility.Visible;
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
                    ShowInfoPopup("Saving error: File already exists");
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        RecentProgressRing.Visibility = Visibility.Collapsed;
                    });
                    return;
                }
                using (Stream stream = await file.OpenStreamForWriteAsync())
                {
                    stream.Write(buffer, 0, buffer.Length); // Save
                }
                ShowInfoPopup("Post Saved");
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    RecentProgressRing.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception e)
            {
                ShowInfoPopup("Saving error: " + e.Message);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    RecentProgressRing.Visibility = Visibility.Collapsed;
                });
            }
        }

        // Keyboard Functions //
        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Enter)
            {
                if(SearchBox.Text != "")
                {
                    ViewModel.ClearPosts();
                    RecentBackButton.Visibility = Visibility.Visible;
                    AddTagButton.Visibility = Visibility.Collapsed;
                    PostsTitle.Text = "Posts: " + SearchBox.Text;
                    Thread followButton = new Thread(ShowFollowButton);
                    followButton.Start();
                    List<string> tags = new List<string>(SearchBox.Text.Split(" "));
                    Thread t = new Thread(LoadRecentPosts);
                    t.Start(tags);
                    SearchBox.Text = "";
                    SearchBox.Visibility = Visibility.Collapsed;
                }
            }
            else if(e.Key == Windows.System.VirtualKey.Escape)
            {
                SearchBox.Text = "";
                SearchBox.Visibility = Visibility.Collapsed;
            }
        }

        // Settings Functions
        private void SettingsDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["username"] = UsernameBox.Text;
            localSettings.Values["apikey"] = ApiKeyBox.Text;
            localSettings.Values["notifTime"] = NotificationTime.Text;
        }
        private void ListSide_Toggled(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (ListSide.IsOn)
            {
                localSettings.Values["notifTime"] = "SETTORIGHT";
                Grid.SetColumn(PostsScrollView, 0);
                Grid.SetColumn(FollowingScrollView, 1);
                Grid.SetColumn(ImageBG, 0);
                Grid.SetColumn(CloseButton, 0);
                Grid.SetColumn(RecentProgressRing, 0);
                Grid.SetColumn(FollowingProgressRing, 0);
                Grid.SetColumn(PostHolder, 1);
                Grid.SetColumn(TagScroller, 0);
                LeftColumn.Width = new GridLength(50,GridUnitType.Star);
                RightColumn.Width = new GridLength(0, GridUnitType.Auto);
                LeftPostColumn.Width = new GridLength(0, GridUnitType.Auto);
                RightPostColumn.Width = new GridLength(90, GridUnitType.Star);
            }
            else
            {
                localSettings.Values["notifTime"] = "SETTOLEFT";
                Grid.SetColumn(PostsScrollView, 1);
                Grid.SetColumn(FollowingScrollView, 0);
                Grid.SetColumn(ImageBG, 1);
                Grid.SetColumn(CloseButton, 1);
                Grid.SetColumn(RecentProgressRing, 1);
                Grid.SetColumn(FollowingProgressRing, 1);
                Grid.SetColumn(PostHolder, 0);
                Grid.SetColumn(TagScroller, 1);
                LeftColumn.Width = new GridLength(0, GridUnitType.Auto);
                RightColumn.Width = new GridLength(50, GridUnitType.Star);
                LeftPostColumn.Width = new GridLength(90, GridUnitType.Star);
                RightPostColumn.Width = new GridLength(0, GridUnitType.Auto);
            }
        }
        private void LoginHyperlinkClick(object sender, RoutedEventArgs e)
        {
            if(LoginSettingsPanel.Visibility == Visibility.Visible)
            {
                LoginSettingsPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                LoginSettingsPanel.Visibility = Visibility.Visible;
            }
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
                var taskName = "Notifier";
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
                NotificationsSettingsPanel.Visibility = Visibility.Visible;
            }
        }
        private void NotificationTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            NotificationTime.Text = new string(NotificationTime.Text.Where(char.IsDigit).ToArray());
            NotificationTime.SelectionStart = NotificationTime.Text.Length;
            NotificationTime.SelectionLength = 0;
        }
    }
}
