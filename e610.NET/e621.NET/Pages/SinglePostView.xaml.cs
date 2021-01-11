using e610.NET.Pages;
using FFmpegInterop;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
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
    
    public sealed partial class SinglePostView : Page
    {
        // Pages Variables //
        private Post singlePost; // Holds the post shown on this page
        private Pool poolToSelect;
        public bool canGetTags;
        private ObservableCollection<Comment> CommentsSource = new ObservableCollection<Comment>(); // Tags tree binding source
        private ObservableCollection<Pool> ConnectedPools = new ObservableCollection<Pool>(); // Pools list binding source
        private ObservableCollection<Pool> MovementSource = new ObservableCollection<Pool>(); // Movement list binding source

        // Page Load Functions //
        public SinglePostView()
        {
            this.InitializeComponent();
            GC.Collect();
            
        }
        private void PageLoad()
        {
            // Update the searchbox with the searchbox from the PostsView
            SearchBox.Text = GlobalVars.searchText;
            canGetTags = true;

            Window.Current.SizeChanged += OnWindowSizeChanged;

            SidePanelShadow.Receivers.Add(PostPanel);
            TopPanelShadow.Receivers.Add(PostPanel);

            // Update the vote up / down with the post score
            VoteUpCount.Text = singlePost.score.up.ToString();
            VoteDownCount.Text = singlePost.score.down.ToString();
            Pool tagname = new Pool();
            tagname.name = "Tags: " + SearchBox.Text;
            MovementSource.Add(tagname);
            if(GlobalVars.PoolName == null)
            {
                MovementSelection.SelectedIndex = 0;
            }
            else
            {
                poolToSelect = GlobalVars.PoolName;
                GlobalVars.PoolName = null;
            }

            // If there are any pools with this post in
            if (singlePost.pools.Count > 0)
            {
                // Added the pools to the listview
                Thread poolThread = new Thread(new ThreadStart(poolPopulate));
                poolThread.Start();
                poolThread = null;
            }
            if (singlePost.comment_count > 0)
            {
                Thread commentsThread = new Thread(new ThreadStart(CommentsPopulate));
                commentsThread.Start();
                commentsThread = null;
            }

            // Check if the post needs to use the mediaplayer or just an image
            if (singlePost.file.ext == "webm")
            {
                bigvideo.Source = new Uri(singlePost.file.url);
                bigvideo.Visibility = Visibility.Visible;
                ImageLoadProgress.Visibility = Visibility.Collapsed;
            }
            else if(singlePost.file.ext == "swf")
            {
                smallpicture.Visibility = Visibility.Visible;
                ImageLoadProgress.Visibility = Visibility.Collapsed;
            }
            else
            {
                bigpicture.Visibility = Visibility.Visible;
            }
            Thread TagsThread = new Thread(new ThreadStart(PopulateTreeView));
            TagsThread.Start();
            TagsThread = null;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.UnloadObject(this);
            bigpicture.Source = null;
            bigpicture = null;
            bigvideo.Source = null;
            bigvideo = null;
            GC.Collect();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            singlePost = (Post)e.Parameter;
            PageLoad();
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
                        MovementSource.Add(newpool);
                        if(MovementSelection.SelectedIndex != 0)
                        {
                            if(poolToSelect.name == newpool.name)
                            {
                                MovementSelection.SelectedIndex =MovementSelection.Items.Count() - 1;
                            }
                        }
                    });
                }
            }
        }
        private async void CommentsPopulate()
        {
            try
            {
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
        private List<Comment> GetComments()
        {
            try
            {
                var client = new RestClient();
                client.BaseUrl = new Uri("https://e621.net/comments.json?");
                client.UserAgent = "e610.NET/1.1(by EpsilonRho)";
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
        private void PopulateTreeView()
        {
            Thread.Sleep(200);
            if(singlePost.tags.artist.Count() > 0)
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
        private async void ClearTagsAndComments()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
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
        private void OnWindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Size window = GetCurrentDisplaySize();
            try
            {
                if (singlePost.file.ext == "webm")
                {
                    bigvideo.Source = new Uri(singlePost.file.url);
                    bigvideo.Visibility = Visibility.Visible;
                    ImageLoadProgress.Visibility = Visibility.Collapsed;
                    bigvideo.Width = double.NaN;
                    bigvideo.Height = window.Height - 105;
                }
                else
                {
                    if (GlobalVars.Binding == "Page Height")
                    {
                        bigpicture.Width = double.NaN;
                        bigpicture.Height = window.Height - 105;
                        CommandBar.HorizontalAlignment = HorizontalAlignment.Center;
                        ImageScrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                        GlobalVars.Binding = "Page Height";
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
                SearchTagAutoComplete.Items.Clear();
                GlobalVars.newSearch = true;
                GlobalVars.searchText = SearchBox.Text;
                this.Frame.Navigate(typeof(PostsViewPage), null, new DrillInNavigationTransitionInfo());
            }
        }
        private void PoolBar_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(PoolBar);
        }
        private void VoteUpButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var client = new RestClient();
            client.BaseUrl = new Uri("https://e621.net/posts/"+ singlePost.id + "/votes.json");
            client.UserAgent = "e610.NET/1.3(by EpsilonRho)";
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
                InfoPopup.Title = "Upvoted";
                InfoPopup.Message = "Post Updated";
                InfoPopup.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Informational;
                InfoPopup.IsOpen = true;
            }
            else
            {
                singlePost.score.up--;
                VoteUpCount.Text = singlePost.score.up.ToString();
                InfoPopup.Title = "Un-Upvoted";
                InfoPopup.Message = "Post Updated";
                InfoPopup.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Informational;
                InfoPopup.IsOpen = true;
            }
            Thread ClosePopup = new Thread(CloseInfoPopup);
            ClosePopup.Start();
        }
        private void VoteDownButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var client = new RestClient();
            client.BaseUrl = new Uri("https://e621.net/posts/" + singlePost.id + "/votes.json");
            client.UserAgent = "e610.NET/1.3(by EpsilonRho)";
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
                InfoPopup.Title = "Downvoted";
                InfoPopup.Message = "Post Updated";
                InfoPopup.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Informational;
                InfoPopup.IsOpen = true;
            }
            else
            {
                singlePost.score.down++;
                VoteDownCount.Text = singlePost.score.down.ToString();
                InfoPopup.Title = "Un-Downvoted";
                InfoPopup.Message = "Post Updated";
                InfoPopup.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Informational;
                InfoPopup.IsOpen = true;
            }
            Thread ClosePopup = new Thread(CloseInfoPopup);
            ClosePopup.Start();
        }
        private void FavoiteButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var client = new RestClient();
            client.BaseUrl = new Uri("https://e621.net/favorites.json");
            client.UserAgent = "e610.NET/1.3(by EpsilonRho)";
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
                InfoPopup.Title = "Favorited";
                InfoPopup.Message = "Post Updated";
                InfoPopup.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Informational;
                InfoPopup.IsOpen = true;
            }
            else
            {
                client.BaseUrl = new Uri("https://e621.net/favorites/" + singlePost.id.ToString() + ".json");
                client.UserAgent = "e610.NET/1.3(by EpsilonRho)";
                request = new RestRequest(RestSharp.Method.DELETE);
                if (GlobalVars.Username != "" && GlobalVars.APIKey != "")
                {
                    request.AddQueryParameter("login", GlobalVars.Username);
                    request.AddQueryParameter("api_key", GlobalVars.APIKey);
                }
                response = client.Execute(request);
                InfoPopup.Title = "Un-Favorited";
                InfoPopup.Message = "Post Updated";
                InfoPopup.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Informational;
                InfoPopup.IsOpen = true;
            }
            Thread ClosePopup = new Thread(CloseInfoPopup);
            ClosePopup.Start();
        }

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
                    DownloadProgress.Visibility = Visibility.Visible;
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
                        DownloadProgress.Visibility = Visibility.Collapsed;
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
                    DownloadProgress.Visibility = Visibility.Collapsed;
                    InfoPopup.Title = "Post Saved";
                    InfoPopup.Message = "File Saved to downlaods";
                    InfoPopup.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Success;
                    InfoPopup.IsOpen = true;
                });
                Thread ClosePopup = new Thread(CloseInfoPopup);
                ClosePopup.Start();
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
                Thread ClosePopup = new Thread(CloseInfoPopup);
                ClosePopup.Start();
            }
        }

        private void bigpicture_ImageExOpened(object sender, Microsoft.Toolkit.Uwp.UI.Controls.ImageExOpenedEventArgs e)
        {
            ImageLoadProgress.Visibility = Visibility.Collapsed;
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

        private void Tags_ItemClick(object sender, ItemClickEventArgs e)
        {
            string ClickedItem = (string)e.ClickedItem;
            GlobalVars.newSearch = true;
            GlobalVars.searchText = ClickedItem;
            this.Frame.Navigate(typeof(PostsViewPage), null, new DrillInNavigationTransitionInfo());
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

        private void CommentsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView != null)
            {
                listView.SelectedIndex = -1;
            }
        }

        private void ImageSize_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(ImageSize);
        }

        private async void CloseInfoPopup()
        {
            Thread.Sleep(3000);
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                InfoPopup.IsOpen = false;
            });
        }

        private void BackPage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                Thread LoadThread = new Thread(LoadPost);
                Pool selected = (Pool)MovementSelection.SelectedItem;
                if (SearchBox.Text.Contains("order:id"))
                {
                    if (selected.name.Contains("Tags:"))
                    {
                        LoadThread.Start(new LoadPostsArgs(SearchBox.Text, singlePost.id, 'b'));
                    }
                    else
                    {
                        LoadThread.Start(new LoadPostsArgs("pool:" + selected.id, singlePost.id, 'b'));
                    }
                }
                else if (!selected.name.Contains("Tags:"))
                {
                    LoadThread.Start(new LoadPostsArgs("pool:" + selected.id, singlePost.id, 'b'));
                }
                else
                {
                    LoadThread.Start(new LoadPostsArgs(SearchBox.Text, singlePost.id, 'a'));
                }
                ImageScrollView.ChangeView(null, 0, null, false);
            }
            catch (Exception)
            {

            }
        }

        private void ForwardPage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                Thread LoadThread = new Thread(LoadPost);
                Pool selected = (Pool)MovementSelection.SelectedItem;
                if (SearchBox.Text.Contains("order:id"))
                {
                    if (selected.name.Contains("Tags:"))
                    {
                        LoadThread.Start(new LoadPostsArgs(SearchBox.Text, singlePost.id, 'a'));
                    }
                    else
                    {
                        LoadThread.Start(new LoadPostsArgs("pool:" + selected.id, singlePost.id, 'a'));
                    }
                }
                else if (!selected.name.Contains("Tags:"))
                {
                    LoadThread.Start(new LoadPostsArgs("pool:" + selected.id, singlePost.id, 'a'));
                }
                else
                {
                    LoadThread.Start(new LoadPostsArgs(SearchBox.Text, singlePost.id, 'b'));
                }
                ImageScrollView.ChangeView(null, 0, null, false);
            }
            catch (Exception)
            {

            }
        }

        private async void LoadPost(object t)
        {
            // Function Vars
            var client = new RestClient(); // Client to handle Requests
            LoadPostsArgs args = (LoadPostsArgs)t; // Convert Object to LoadPostArgs class
            var request = new RestRequest(RestSharp.Method.GET); // REST request

            // Show Progress Bar + Get post limit from slider
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                LoadingBar.IsIndeterminate = true;
                LoadingBar.Visibility = Visibility.Visible;
                ImageLoadProgress.Visibility = Visibility.Visible;
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

            request.AddQueryParameter("tags", GlobalVars.Rating + " " + args.tags);

            request.AddQueryParameter("limit", "1");

            // If the lastid is not -1, use the last post id to move forward or back a page
            if (args.page != -1)
            {
                request.AddQueryParameter("page", args.paginationChar + args.page.ToString());
            }

            // Send the request
            var response = client.Execute(request);

            // Deserialize the response
            Root DeserializedJson = JsonConvert.DeserializeObject<Root>(response.Content);

            if(DeserializedJson.posts.Count > 0)
            {
                if(DeserializedJson.posts[0].file.url == null)
                {
                    args.page = DeserializedJson.posts[0].id;
                    LoadPost(args);
                    return;
                }
                singlePost = DeserializedJson.posts[0];
            }
            else
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
                {
                    LoadingBar.Visibility = Visibility.Collapsed;
                    ImageLoadProgress.Visibility = Visibility.Collapsed;
                });
                return;
            }

            // Loading done, hide the progress bar
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
            {
                LoadingBar.Visibility = Visibility.Collapsed;
                Bindings.Update();
            });

            Pool selected = null;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => // Call Shit needed from UI Thread
            {
                selected = (Pool)MovementSelection.SelectedItem;
            });
            if (selected.name.Contains("Tags:"))
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    MovementSource.Clear();
                    Pool tagname = new Pool();
                    tagname.name = "Tags: " + SearchBox.Text;
                    MovementSource.Add(tagname);
                    MovementSelection.SelectedIndex = 0;
                });
            }
            else
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    poolToSelect = selected;
                    MovementSource.Clear();
                    Pool tagname = new Pool();
                    tagname.name = "Tags: " + SearchBox.Text;
                    MovementSource.Add(tagname);
                });
            }
            ClearTagsAndComments();
            
            if (singlePost.pools.Count > 0)
            {
                // Added the pools to the listview
                Thread poolThread = new Thread(new ThreadStart(poolPopulate));
                poolThread.Start();
                poolThread = null;
            }
            if (singlePost.comment_count > 0)
            {
                Thread commentsThread = new Thread(new ThreadStart(CommentsPopulate));
                commentsThread.Start();
                commentsThread = null;
            }
            if (singlePost.file.ext == "webm")
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    bigvideo.Source = new Uri(singlePost.file.url);
                    bigvideo.Visibility = Visibility.Visible;
                    ImageLoadProgress.Visibility = Visibility.Collapsed;
                    bigpicture.Visibility = Visibility.Collapsed;
                    smallpicture.Visibility = Visibility.Collapsed;
                    bigvideo.Width = double.NaN;
                    bigvideo.Height = page.ActualHeight - 55;
                });
            }
            else if (singlePost.file.ext == "swf")
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    smallpicture.Visibility = Visibility.Visible;
                    ImageLoadProgress.Visibility = Visibility.Collapsed;
                    bigpicture.Visibility = Visibility.Collapsed;
                    bigvideo.Visibility = Visibility.Collapsed;
                });
            }
            else
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    bigpicture.Visibility = Visibility.Visible;
                    smallpicture.Visibility = Visibility.Collapsed;
                    bigvideo.Visibility = Visibility.Collapsed;
                });
            }
            Thread TagsThread = new Thread(new ThreadStart(PopulateTreeView));
            TagsThread.Start();
            TagsThread = null;
            GC.Collect();
        }

        private async void bigpicture_ImageExOpened_1(object sender, Microsoft.Toolkit.Uwp.UI.Controls.ImageExOpenedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ImageLoadProgress.Visibility = Visibility.Collapsed;
            });
            try
            {
                if (GlobalVars.Binding == "Sample Height")
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        bigpicture.Width = singlePost.sample.width;
                        bigpicture.Height = singlePost.sample.height;
                        CommandBar.HorizontalAlignment = HorizontalAlignment.Center;
                        ImageScrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                        GlobalVars.Binding = "Sample Height";
                        Bindings.Update();
                    });
                }
                else if (GlobalVars.Binding == "Page Height")
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        bigpicture.Width = double.NaN;
                        bigpicture.Height = page.ActualHeight - 65;
                        CommandBar.HorizontalAlignment = HorizontalAlignment.Center;
                        ImageScrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                        GlobalVars.Binding = "Page Height";
                        Bindings.Update();
                    });
                }
                else
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        bigpicture.Width = singlePost.file.width;
                        bigpicture.Height = singlePost.file.height;
                        CommandBar.HorizontalAlignment = HorizontalAlignment.Left;
                        ImageScrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                        GlobalVars.Binding = "Full Height";
                        Bindings.Update();
                    });
                }
            }
            catch (Exception)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    bigpicture.Width = singlePost.sample.width;
                    bigpicture.Height = singlePost.sample.height;
                });
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
                    CommandBar.HorizontalAlignment = HorizontalAlignment.Center;
                    ImageScrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    GlobalVars.Binding = "Sample Height";
                }
                else if (clickItem.Text == "Page Height")
                {
                    bigpicture.Width = double.NaN;
                    bigpicture.Height = page.ActualHeight - 65;
                    CommandBar.HorizontalAlignment = HorizontalAlignment.Center;
                    ImageScrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    GlobalVars.Binding = "Page Height";
                }
                else
                {
                    bigpicture.Width = singlePost.file.width;
                    bigpicture.Height = singlePost.file.height;
                    CommandBar.HorizontalAlignment = HorizontalAlignment.Left;
                    ImageScrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    GlobalVars.Binding = "Full Height";
                }
            }
            catch (Exception)
            {
                bigpicture.Width = singlePost.sample.width;
                bigpicture.Height = singlePost.sample.height;
            }
        }

        private async void getTags(object t)
        {
            // Function Vars
            var client = new RestClient(); // Client to handle Requests
            string args = (string)t; // Convert Object to LoadPostArgs class
            var request = new RestRequest(RestSharp.Method.GET); // REST request

            if (args[0] == '-')
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

        private void page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (SearchBox.FocusState != FocusState.Keyboard && SearchBox.FocusState != FocusState.Programmatic && SearchBox.FocusState != FocusState.Pointer)
            {
                if (e.Key == Windows.System.VirtualKey.Left)
                {
                    try
                    {
                        Thread LoadThread = new Thread(LoadPost);
                        Pool selected = (Pool)MovementSelection.SelectedItem;
                        if (SearchBox.Text.Contains("order:id"))
                        {
                            if (selected.name.Contains("Tags:"))
                            {
                                LoadThread.Start(new LoadPostsArgs(SearchBox.Text, singlePost.id, 'b'));
                            }
                            else
                            {
                                LoadThread.Start(new LoadPostsArgs("pool:" + selected.id, singlePost.id, 'b'));
                            }
                        }
                        else if (!selected.name.Contains("Tags:"))
                        {
                            LoadThread.Start(new LoadPostsArgs("pool:" + selected.id, singlePost.id, 'b'));
                        }
                        else
                        {
                            LoadThread.Start(new LoadPostsArgs(SearchBox.Text, singlePost.id, 'a'));
                        }
                        ImageScrollView.ChangeView(null, 0, null, false);
                    }
                    catch (Exception)
                    {

                    }
                }
                else if (e.Key == Windows.System.VirtualKey.Right)
                {
                    try
                    {
                        Thread LoadThread = new Thread(LoadPost);
                        Pool selected = (Pool)MovementSelection.SelectedItem;
                        if (SearchBox.Text.Contains("order:id"))
                        {
                            if (selected.name.Contains("Tags:"))
                            {
                                LoadThread.Start(new LoadPostsArgs(SearchBox.Text, singlePost.id, 'a'));
                            }
                            else
                            {
                                LoadThread.Start(new LoadPostsArgs("pool:" + selected.id, singlePost.id, 'a'));
                            }
                        }
                        else if (!selected.name.Contains("Tags:"))
                        {
                            LoadThread.Start(new LoadPostsArgs("pool:" + selected.id, singlePost.id, 'a'));
                        }
                        else
                        {
                            LoadThread.Start(new LoadPostsArgs(SearchBox.Text, singlePost.id, 'b'));
                        }
                        ImageScrollView.ChangeView(null, 0, null, false);
                    }
                    catch (Exception)
                    {

                    }
                }
                else if(e.Key == Windows.System.VirtualKey.Escape)
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

        private void LeftSwipeItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            try
            {
                Thread LoadThread = new Thread(LoadPost);
                Pool selected = (Pool)MovementSelection.SelectedItem;
                if (SearchBox.Text.Contains("order:id"))
                {
                    if (selected.name.Contains("Tags:"))
                    {
                        LoadThread.Start(new LoadPostsArgs(SearchBox.Text, singlePost.id, 'b'));
                    }
                    else
                    {
                        LoadThread.Start(new LoadPostsArgs("pool:" + selected.id, singlePost.id, 'b'));
                    }
                }
                else if (!selected.name.Contains("Tags:"))
                {
                    LoadThread.Start(new LoadPostsArgs("pool:" + selected.id, singlePost.id, 'b'));
                }
                else
                {
                    LoadThread.Start(new LoadPostsArgs(SearchBox.Text, singlePost.id, 'a'));
                }
                ImageScrollView.ChangeView(null, 0, null, false);
            }
            catch (Exception)
            {

            }
        }

        private void RightSwipeItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            try
            {
                Thread LoadThread = new Thread(LoadPost);
                Pool selected = (Pool)MovementSelection.SelectedItem;
                if (SearchBox.Text.Contains("order:id"))
                {
                    if (selected.name.Contains("Tags:"))
                    {
                        LoadThread.Start(new LoadPostsArgs(SearchBox.Text, singlePost.id, 'a'));
                    }
                    else
                    {
                        LoadThread.Start(new LoadPostsArgs("pool:" + selected.id, singlePost.id, 'a'));
                    }
                }
                else if (!selected.name.Contains("Tags:"))
                {
                    LoadThread.Start(new LoadPostsArgs("pool:" + selected.id, singlePost.id, 'a'));
                }
                else
                {
                    LoadThread.Start(new LoadPostsArgs(SearchBox.Text, singlePost.id, 'b'));
                }
                ImageScrollView.ChangeView(null, 0, null, false);
            }
            catch (Exception)
            {

            }
        }

    }
}
