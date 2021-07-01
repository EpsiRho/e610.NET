using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace e610.NET.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SauceNao : Page
    {
        public Windows.Storage.StorageFile file;

        public SauceNao()
        {
            this.InitializeComponent();
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            try
            {
                APITextBox.Text = (string)localSettings.Values["SauceNaoAPIKey"];
            }
            catch (Exception)
            {

            }
        }

        private void BackPage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Function Vars
                var client = new RestClient(); // Client to handle Requests
                var request = new RestRequest(RestSharp.Method.POST); // REST request

                // Show Progress Bar + Get post limit from slider
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ResultLoadProgress.Visibility = Visibility.Visible;
                    ResultsView.Children.Clear();
                });

                client.BaseUrl = new Uri("https://saucenao.com/search.php?");

                // Set the useragent for e621
                client.UserAgent = "e610.NET/1.7(by EpsilonRho)";

                request.AddQueryParameter("output_type", "2");
                request.AddQueryParameter("api_key", APITextBox.Text);
                request.AddQueryParameter("db", "999");

                request.AddHeader("Content-Type", "multipart/form-data");

                if (file != null)
                {
                    var stream = await file.OpenStreamForReadAsync();
                    var bytes = new byte[(int)stream.Length];
                    stream.Read(bytes, 0, (int)stream.Length);
                    request.AddFile("file", bytes, file.Name);
                }

                // Send the request
                var response = client.Execute(request);

                // Deserialize the response
                SauceNaoJSON DeserializedJson;
                try
                {
                    DeserializedJson = JsonConvert.DeserializeObject<SauceNaoJSON>(response.Content);
                }
                catch (Exception err)
                {
                    return;
                }

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ResultLoadProgress.Visibility = Visibility.Collapsed;
                    ShortApiLimit.Text = $"Short Api Limit: {DeserializedJson.header.short_remaining}/{DeserializedJson.header.short_limit}";
                    LongApiLimit.Text = $"Long Api Limit: {DeserializedJson.header.long_remaining}/{DeserializedJson.header.long_limit}";
                });

                try
                {
                    foreach (Result r in DeserializedJson.results)
                    {
                        var hStack = new StackPanel();
                        hStack.Orientation = Orientation.Horizontal;
                        hStack.Margin = new Thickness(0, 10, 10, 10);

                        var thumbnail = new ImageEx();
                        var image = new BitmapImage(new Uri(r.header.thumbnail, UriKind.Absolute));
                        thumbnail.Source = image;
                        thumbnail.Width = double.NaN;
                        thumbnail.Height = double.NaN;
                        thumbnail.Margin = new Thickness(0, 0, 10, 0);

                        hStack.Children.Add(thumbnail);

                        var vStack = new StackPanel();

                        var CreatorText = new TextBlock();
                        CreatorText.Text = $"Creator: {r.data.author_name}";

                        vStack.Children.Add(CreatorText);

                        var SimilarityText = new TextBlock();
                        SimilarityText.Text = $"Similarity: {r.header.similarity}";

                        vStack.Children.Add(SimilarityText);

                        var LinksText = new TextBlock();
                        LinksText.Text = $"Links:";

                        vStack.Children.Add(LinksText);
                        try
                        {
                            foreach (string url in r.data.ext_urls)
                            {
                                var LinkText = new HyperlinkButton();
                                LinkText.Content = $"{url}";
                                LinkText.Click += launchURI_Click;
                                LinkText.Padding = new Thickness(0, 5, 5, 5);
                                LinkText.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                                vStack.Children.Add(LinkText);
                            }
                        }
                        catch (Exception)
                        {

                        }

                        hStack.Children.Add(vStack);

                        ResultsView.Children.Add(hStack);
                    }
                }
                catch (Exception err)
                {
                    return;
                }
            }
            catch (Exception)
            {

            }
        }

        private async void launchURI_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton button = (HyperlinkButton)sender;
            string url = (string)button.Content;
            if (url.Contains("e621"))
            {
                string[] split = url.Split("/");
                GlobalVars.searchText = "id:" + Convert.ToInt32(split[split.Length - 1]);
                int tabIndex = MainPage.MainTabViewAccess.SelectedIndex;
                (MainPage.MainTabViewAccess.TabItems[tabIndex] as TabViewItem).Header = $"Post: {Convert.ToInt32(split[split.Length - 1])}";
                this.Frame.Navigate(typeof(PostsViewPage), null, new DrillInNavigationTransitionInfo());
            }
            else
            {
                var uri = new Uri(url);
                var success = await Windows.System.Launcher.LaunchUriAsync(uri);
            }
        }

        private async void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Define and show the file picker
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add("*");
            file = await picker.PickSingleFileAsync();

            ChosenFile.Text = file.Name;
        }

        private void APITextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["SauceNaoAPIKey"] = APITextBox.Text;
        }
    }
}
