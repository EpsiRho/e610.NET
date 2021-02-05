using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Background_Notifier
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public sealed class Tag : INotifyPropertyChanged
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
        public string related_tags { get; set; }
        public int category { get; set; }
        public bool is_locked { get; set; }
    }

    public sealed class TagList
    {
        public Tag[] tags { get; set; }
    }

}
