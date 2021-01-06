using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace e610.NET
{
    public class File
    {
        public int width { get; set; }
        public int height { get; set; }
        public string ext { get; set; }
        public int size { get; set; }
        public string md5 { get; set; }
        public string url { get; set; }
    }

    public class Preview
    {
        public int width { get; set; }
        public int height { get; set; }
        public string url { get; set; }
    }

    public class Alternates
    {
    }

    public class Sample
    {
        public bool has { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public string url { get; set; }
        public Alternates alternates { get; set; }
    }

    public class Score
    {
        public int up { get; set; }
        public int down { get; set; }
        public int total { get; set; }
    }

    public class Tags
    {
        public List<string> general { get; set; }
        public List<string> species { get; set; }
        public List<string> character { get; set; }
        public List<string> copyright { get; set; }
        public List<string> artist { get; set; }
        public List<object> invalid { get; set; }
        public List<object> lore { get; set; }
        public List<string> meta { get; set; }
    }

    public class Flags
    {
        public bool pending { get; set; }
        public bool flagged { get; set; }
        public bool note_locked { get; set; }
        public bool status_locked { get; set; }
        public bool rating_locked { get; set; }
        public bool deleted { get; set; }
    }

    public class Relationships
    {
        public int? parent_id { get; set; }
        public bool has_children { get; set; }
        public bool has_active_children { get; set; }
        public List<int> children { get; set; }
    }

    public class Post : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        private string thumbURL;
        public string ThumbURL
        {
            get
            {
                return thumbURL;
            }
            set
            {
                if(value != thumbURL)
                {
                    thumbURL = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int id { get; set; }
        public DateTime created_at { get; set; }
        public File file { get; set; }
        public Preview preview { get; set; }
        public Sample sample { get; set; }
        public Score score { get; set; }
        public Tags tags { get; set; }
        public List<object> locked_tags { get; set; }
        public int change_seq { get; set; }
        public Flags flags { get; set; }
        public string rating { get; set; }
        public int fav_count { get; set; }
        public List<string> sources { get; set; }
        public List<int> pools { get; set; }
        public Relationships relationships { get; set; }
        public int? approver_id { get; set; }
        public int uploader_id { get; set; }
        public string description { get; set; }
        public int comment_count { get; set; }
        public bool is_favorited { get; set; }
        public bool has_notes { get; set; }
        public object duration { get; set; }
    }

    public class Root
    {
        public List<Post> posts { get; set; }
    }

    public class PostsViewModel
    {
        private ObservableCollection<Post> posts = new ObservableCollection<Post>();
        public ObservableCollection<Post> Posts { get { return this.posts; } }
        public PostsViewModel() { }

        public void AddPost(Post p)
        {
            posts.Add(p);
        }
        public void ClearPosts()
        {
            posts.Clear();
        }
        public void changeThumbnail(bool opt)
        {
            if(opt)
            {
                foreach(Post p in posts)
                {
                    p.ThumbURL = p.sample.url;
                }
            }
            else
            {
                foreach (Post p in posts)
                {
                    p.ThumbURL = p.preview.url;
                }
            }
        }
    }
}
