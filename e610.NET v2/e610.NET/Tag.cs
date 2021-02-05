using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace e610.NET
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Tag : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int id { get; set; }
        public string name { get; set; }
        public int post_count { get; set; }
        public int old_post_count { get; set; }
        public int total_new { get; set; }
        private Visibility show_count;
        public Visibility Show_Count
        {
            get
            {
                return show_count;
            }
            set
            {
                if (value != this.show_count)
                {
                    show_count = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private Visibility show_progress;
        public Visibility Show_Progress
        {
            get
            {
                return show_progress;
            }
            set
            {
                if (value != this.show_progress)
                {
                    show_progress = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string related_tags { get; set; }
        public int category { get; set; }
        public bool is_locked { get; set; }
    }

    public class TagList
    {
        public Tag[] tags { get; set; }
    }

    public class FollowsViewModel
    {
        private ObservableCollection<Tag> tags = new ObservableCollection<Tag>();
        public ObservableCollection<Tag> Tags { get { return this.tags; } }
        public FollowsViewModel() { }

        public void AddTag(Tag t)
        {
            tags.Add(t);
        }
        public void ClearTags()
        {
            tags.Clear();
        }
    }

}
