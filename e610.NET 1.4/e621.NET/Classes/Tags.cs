using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace e610.NET
{
    public class Tag
    {
        public int id { get; set; }
        public string name { get; set; }
        public int post_count { get; set; }
        public string related_tags { get; set; }
        public DateTime related_tags_updated_at { get; set; }
        public int category { get; set; }
        public bool is_locked { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class TagsHolder
    {
        public List<Tag> tags { get; set; }
    }
}
