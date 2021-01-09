using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace e610.NET
{
    public class Comment : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private string avatar_url;
        public string Avatar_Url
        {
            get
            {
                return this.avatar_url;
            }
            set
            {
                if (value != this.avatar_url)
                {
                    avatar_url = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string quote { get; set; }
        public Visibility quotevis { get; set; }
        public string quotedName { get; set; }
        public int quotedID { get; set; }
        public int id { get; set; }
        public DateTime created_at { get; set; }
        public int post_id { get; set; }
        public int creator_id { get; set; }
        public string body { get; set; }
        public int score { get; set; }
        public DateTime updated_at { get; set; }
        public int updater_id { get; set; }
        public bool do_not_bump_post { get; set; }
        public bool is_hidden { get; set; }
        public bool is_sticky { get; set; }
        public string creator_name { get; set; }
        public string updater_name { get; set; }
    }

    public class CommentsHolder
    {
        public List<Comment> comments { get; set; }
    }
}
