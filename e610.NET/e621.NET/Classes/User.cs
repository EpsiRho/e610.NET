using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace e610.NET
{
    public class User
    {
        //public int wiki_page_version_count { get; set; }
        public int artist_version_count { get; set; }
        public int pool_version_count { get; set; }
        public int forum_post_count { get; set; }
        public int comment_count { get; set; }
        public int appeal_count { get; set; }
        public int flag_count { get; set; }
        public int positive_feedback_count { get; set; }
        public int neutral_feedback_count { get; set; }
        public int negative_feedback_count { get; set; }
        public int upload_limit { get; set; }
        public int id { get; set; }
        public DateTime created_at { get; set; }
        public string name { get; set; }
        public int level { get; set; }
        public int base_upload_limit { get; set; }
        public int post_upload_count { get; set; }
        public int post_update_count { get; set; }
        public int note_update_count { get; set; }
        public bool is_banned { get; set; }
        public bool can_approve_posts { get; set; }
        public bool can_upload_free { get; set; }
        public string level_string { get; set; }
        public int avatar_id { get; set; }
    }
}
