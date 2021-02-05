using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Xaml;

namespace Background_Notifier
{
    public sealed class Notifier : IBackgroundTask
    {
        string[] follows;
        List<Tag> Updated;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile saveFile = await storageFolder.CreateFileAsync("SaveFile.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
                string text = await Windows.Storage.FileIO.ReadTextAsync(saveFile);
                follows = text.Split('\n');
                Updated = new List<Tag>();
                foreach (string name in follows)
                {
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
                        client.UserAgent = "e621 Follower/1.0(by EpsilonRho)";
                        request.AddQueryParameter("search[name_matches]", tag.name);
                        request.AddQueryParameter("search[order]", "count");
                        //request.AddQueryParameter("search[hide_empty]", "true");
                        request.AddQueryParameter("limit", "1");
                        // Send the request
                        var response = client.Execute(request);
                        // Deserialize the response
                        TagList DeserializedJson = JsonConvert.DeserializeObject<TagList>("{tags:" + response.Content + "}");

                        tag.total_new = DeserializedJson.tags[0].post_count - tag.old_post_count;

                        if (tag.total_new > 0)
                        {
                            Updated.Add(tag);
                        }

                    }
                }
                if (Updated.Count() > 0)
                {
                    string tags = "";
                    if (Updated.Count() <= 3)
                    {
                        for (int i = 0; i < Updated.Count(); i++)
                        {
                            tags += Updated[i].name;
                            if(i < Updated.Count() - 1)
                            {
                                tags += ", ";
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            tags += Updated[i].name;
                            if (i < 2)
                            {
                                tags += ", ";
                            }
                        }
                    }

                    if (Updated.Count() > 3)
                    {
                        tags += ", and more have new posts";
                    }
                    else
                    {
                        tags += " have new posts";
                    }
                    var toastContent = new ToastContent()
                    {
                        Visual = new ToastVisual()
                        {
                            BindingGeneric = new ToastBindingGeneric()
                            {
                                Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = "New posts on tags you follow!"
                                },
                                new AdaptiveText()
                                {
                                    Text = tags
                                }
                            }
                            }
                        }
                    };

                    // Create the toast notification
                    var toastNotif = new ToastNotification(toastContent.GetXml());

                    // And send the notification
                    ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
                }
            }
            catch (Exception e)
            {
                StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile errorLog = await storageFolder.CreateFileAsync("ErrorLog.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
                string text = await Windows.Storage.FileIO.ReadTextAsync(errorLog);
                text += e.Message + "\n";
                await Windows.Storage.FileIO.WriteTextAsync(errorLog, text);
            }
        }

        private async void PopToast()
        {
            try
            {
                if (Updated.Count() > 0)
                {
                    string tags = "";
                    for (int i = 0; i < 3; i++)
                    {
                        tags += Updated[i].name + ",";
                    }
                    if (Updated.Count() > 3)
                    {
                        tags += "and more!";
                    }
                    var toastContent = new ToastContent()
                    {
                        Visual = new ToastVisual()
                        {
                            BindingGeneric = new ToastBindingGeneric()
                            {
                                Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = "New posts on tags you follow!"
                                },
                                new AdaptiveText()
                                {
                                    Text = tags
                                }
                            }
                            }
                        }
                    };

                    // Create the toast notification
                    var toastNotif = new ToastNotification(toastContent.GetXml());

                    // And send the notification
                    ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
                }
            }
            catch (Exception e)
            {
                StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile errorLog = await storageFolder.CreateFileAsync("ErrorLog.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
                string text = await Windows.Storage.FileIO.ReadTextAsync(errorLog);
                text += e.Message + "\n";
                await Windows.Storage.FileIO.WriteTextAsync(errorLog, text);
            }
        }
    }
}
