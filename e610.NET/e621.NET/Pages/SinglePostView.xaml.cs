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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Streams;
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
        private ObservableCollection<Comment> CommentsSource = new ObservableCollection<Comment>(); // Tags tree binding source
        private ObservableCollection<Pool> ConnectedPools = new ObservableCollection<Pool>(); // Pools list binding source

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
            }
            else if(singlePost.file.ext == "swf")
            {
                smallpicture.Visibility = Visibility.Visible;
            }
            else
            {
                bigpicture.Visibility = Visibility.Visible;
            }
            ImageLoadProgress.Visibility = Visibility.Collapsed;
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
                    });
                }
            }
        }
        private async void CommentsPopulate()
        {
            Thread.Sleep(10);
            List<Comment> Comments = GetComments();
            for(int i = 0; i < Comments.Count(); i++)
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
                        for(int k = 1; k < temp.Length; k++)
                        {
                            c.quotedName = c.quotedName.Replace(temp[k], "");
                        }
                        c.quotedName = c.quotedName.Replace("/", "");
                        c.quotedName = c.quotedName.Replace("\"", "");
                        c.quotedName = c.quotedName.Replace(":", "");
                        c.quotedName += "said:";
                        temp = temp[temp.Count() - 1].Split(" ");
                        c.quotedID = Int32.Parse(temp[0]);
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
        private Pool getPoolInfo(int poolID)
        {
            try
            {
                var client = new RestClient();
                client.BaseUrl = new Uri("https://e621.net/pools.json?");
                client.UserAgent = "e610.NET/1.1(by EpsilonRho)";
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
            FlyoutBase.ShowAttachedFlyout(PoolBar);
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
                        DownloadErrorPopup.IsOpen = true;
                    });
                }
                using (Stream stream = await file.OpenStreamForWriteAsync())
                {
                    stream.Write(buffer, 0, buffer.Length); // Save
                }
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    DownloadProgress.Visibility = Visibility.Collapsed;
                });
            }
            catch
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    DownloadErrorPopup.IsOpen = true;
                });
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
            if (SearchBox.Text.Contains("-" + ClickedItem))
            {
                SearchBox.Text = SearchBox.Text.Replace("-" + ClickedItem, ClickedItem);
            }
            else if (SearchBox.Text.Contains(ClickedItem))
            {
                SearchBox.Text = SearchBox.Text.Replace(ClickedItem, "");
            }
            else 
            { 
                if (SearchBox.Text.Count() > 0)
                {
                    if (SearchBox.Text.Count() == SearchBox.Text.LastIndexOf(' '))
                    {
                        SearchBox.Text.Remove(SearchBox.Text.LastIndexOf(' '));
                    }
                    SearchBox.Text += " " + ClickedItem;
                }
                else
                {
                    SearchBox.Text += ClickedItem;
                }
            }
        }

        private void Tags_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            try
            {
                string ClickedItem = (e.OriginalSource as FrameworkElement).DataContext as string;
                if (SearchBox.Text.Contains(ClickedItem))
                {
                    if (SearchBox.Text.Contains("-" + ClickedItem))
                    {
                        SearchBox.Text = SearchBox.Text.Replace("-"+ClickedItem, "");
                    }
                    else 
                    {
                        SearchBox.Text = SearchBox.Text.Replace(ClickedItem, "-" + ClickedItem);
                    }
                }
                else
                {
                    if (SearchBox.Text.Count() > 0)
                    {
                        if (SearchBox.Text.Count() == SearchBox.Text.LastIndexOf(' '))
                        {
                            SearchBox.Text.Remove(SearchBox.Text.LastIndexOf(' '));
                        }
                        SearchBox.Text += " -" + ClickedItem;
                    }
                    else
                    {
                        SearchBox.Text += "-" + ClickedItem;
                    }
                }
            }
            catch (Exception)
            {

            }
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
    }
}
