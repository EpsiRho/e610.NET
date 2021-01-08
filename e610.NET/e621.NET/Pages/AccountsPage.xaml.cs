using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
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
    public sealed partial class AccountsPage : Page
    {
        // Page Vars //
        private LoggedInUser CurrentUser;
        public PostsViewModel ViewModel { get; set; }

        // Page Load Functions //
        public AccountsPage()
        {
            this.InitializeComponent();
            ViewModel = new PostsViewModel();
            GetSettings();
        }
        private void GetSettings()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            GlobalVars.Username = (string)localSettings.Values["username"];
            GlobalVars.APIKey = (string)localSettings.Values["apikey"];
            if (GlobalVars.Username != "" && GlobalVars.APIKey != "")
            { 
                LoadingBar.Visibility = Visibility.Visible;
                if (GlobalVars.LoggedInInfo != null)
                {
                    LoginPanel.Visibility = Visibility.Collapsed;
                    AccountPanel.Visibility = Visibility.Visible;
                    UsernameTitle.Text = GlobalVars.Username;
                    CurrentUser = GlobalVars.LoggedInInfo;
                    Thread infoThread = new Thread(LoadFavPosts);
                    infoThread.Start(new LoadPostsArgs(GlobalVars.Username, 10));
                }
                else
                {
                    Thread loginThread = new Thread(login);
                    loginThread.Start();
                }
            }
        }
        private async void login()
        {
            var client = new RestClient(); // Client to handle Requests
            var request = new RestRequest(RestSharp.Method.GET); // REST request
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                LoginPanel.Visibility = Visibility.Collapsed;
                AccountPanel.Visibility = Visibility.Visible;
                UsernameTitle.Text = GlobalVars.Username;
            });
            client.BaseUrl = new Uri("https://e621.net/users.json?");
            client.UserAgent = "e610.NET/1.2(by EpsilonRho)";
            request.AddQueryParameter("search[name_matches]", GlobalVars.Username);

            var response = client.Execute(request);
            string edited = response.Content.Remove(0, 1);
            edited = edited.Remove(edited.Count() - 1, 1);
            LoggedInUser DeserializedJson = JsonConvert.DeserializeObject<LoggedInUser>(edited);
            CurrentUser = DeserializedJson;
            getInfo();
            Thread infoThread = new Thread(LoadFavPosts);
            infoThread.Start(new LoadPostsArgs(GlobalVars.Username, 10));
        }
        private async void getInfo()
        {
            if (CurrentUser.can_approve_posts)
            {
                CurrentUser.perms += "Approve Posts ";
            }
            if (CurrentUser.can_upload_free)
            {
                CurrentUser.perms += "Unrestricted Uploads";
            }
            var client = new RestClient(); // Client to handle Requests
            var request = new RestRequest(RestSharp.Method.GET); // REST request
            client.BaseUrl = new Uri("https://e621.net/deleted_posts");
            client.UserAgent = "e610.NET/1.2(by EpsilonRho)";
            if (GlobalVars.Username != "" && GlobalVars.APIKey != "")
            {
                request.AddQueryParameter("login", GlobalVars.Username);
                request.AddQueryParameter("api_key", GlobalVars.APIKey);
            }
            request.AddQueryParameter("user_id", CurrentUser.id.ToString());

            // Send the request
            var response = client.Execute(request);

            int deletedCount = Regex.Matches(response.Content, "word").Count;
            CurrentUser.post_deleted_count = deletedCount;
            CurrentUser.actual_upload_limit = CurrentUser.base_upload_limit + ((CurrentUser.post_upload_count + deletedCount) / 10) - (deletedCount / 4) - 0;
            GlobalVars.LoggedInInfo = CurrentUser;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Bindings.Update();
            });
        }
        private async void LoadFavPosts(object t)
        {
            // Function Vars
            var client = new RestClient(); // Client to handle Requests
            LoadPostsArgs args = (LoadPostsArgs)t; // Convert Object to LoadPostArgs class
            var request = new RestRequest(RestSharp.Method.GET); // REST request

            // Set Endpoint
            // TODO: Switching between e621 - gelbooru - r34 - etc
            client.BaseUrl = new Uri("https://e621.net/posts.json?");

            // Set the useragent for e621
            client.UserAgent = "e610.NET/1.2(by EpsilonRho)";

            // If user is logged in set login parameters into request
            if (GlobalVars.Username != "" && GlobalVars.APIKey != "")
            {
                request.AddQueryParameter("login", GlobalVars.Username);
                request.AddQueryParameter("api_key", GlobalVars.APIKey);
            }

            // Set parameters for tags and post limit
            request.AddQueryParameter("tags", GlobalVars.Rating + " fav:" + args.tags);

            request.AddQueryParameter("limit", "10");

            // Send the request
            var response = client.Execute(request);

            // Deserialize the response
            Root DeserializedJson = JsonConvert.DeserializeObject<Root>(response.Content);

            // Get the posts from the Deserialized Class
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
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        p.preview.url = "https://ambisure.com/wp-content/uploads/2019/03/SHOCK1-1030x724.png";
                        p.file.url = "https://ambisure.com/wp-content/uploads/2019/03/SHOCK1-1030x724.png";
                        ViewModel.AddPost(p);
                    });
                }
                DeserializedJson = null;
            }
            GC.Collect();

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                LoadingBar.Visibility = Visibility.Collapsed;
            });
        }

        // Button Functions //
        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["username"] = UsernameBox.Text;
            localSettings.Values["apikey"] = APIKeyBox.Text;

            var client = new RestClient(); // Client to handle Requests
            var request = new RestRequest(RestSharp.Method.GET); // REST request
            client.BaseUrl = new Uri("https://e621.net/posts.json?");
            client.UserAgent = "e610.NET/1.2(by EpsilonRho)";
            request.AddQueryParameter("login", UsernameBox.Text);
            request.AddQueryParameter("api_key", APIKeyBox.Text);
            request.AddQueryParameter("limit", "1");

            // Send the request
            var response = client.Execute(request);

            if (response.Content.Contains("AuthenticationFailure"))
            {
                ErrorMessage.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                GlobalVars.Username = UsernameBox.Text;
                GlobalVars.APIKey = APIKeyBox.Text;
                Thread loginThread = new Thread(login);
                loginThread.Start();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorMessage.Visibility = Visibility.Collapsed;
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["username"] = "";
            localSettings.Values["apikey"] = "";
            UsernameBox.Text = "";
            APIKeyBox.Text = "";
            GlobalVars.Username = "";
            GlobalVars.APIKey = "";
            LoginPanel.Visibility = Visibility.Visible;
            AccountPanel.Visibility = Visibility.Collapsed;
        }

        private void ImageGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            Post pick = (Post)e.ClickedItem;
            GlobalVars.searchText = "fav:" + GlobalVars.Username;
            this.Frame.Navigate(typeof(SinglePostView), pick, new DrillInNavigationTransitionInfo());
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalVars.newSearch = true;
            GlobalVars.searchText = "fav:"+ GlobalVars.Username;
            this.Frame.Navigate(typeof(PostsViewPage), null, new DrillInNavigationTransitionInfo());
        }
    }
}
