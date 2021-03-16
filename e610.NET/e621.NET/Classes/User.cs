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
        public object avatar_id { get; set; }
    }
    public class LoggedInUser
    {
        public string perms;
        public int id { get; set; }
        public DateTime created_at { get; set; }
        public string name { get; set; }
        public int level { get; set; }
        public int base_upload_limit { get; set; }
        public int actual_upload_limit { get; set; }
        public int post_deleted_count { get; set; }
        public int post_upload_count { get; set; }
        public int post_update_count { get; set; }
        public int note_update_count { get; set; }
        public bool is_banned { get; set; }
        public bool can_approve_posts { get; set; }
        public bool can_upload_free { get; set; }
        public string level_string { get; set; }
        public object avatar_id { get; set; }
        public bool show_avatars { get; set; }
        public bool blacklist_avatars { get; set; }
        public bool blacklist_users { get; set; }
        public bool description_collapsed_initially { get; set; }
        public bool hide_comments { get; set; }
        public bool show_hidden_comments { get; set; }
        public bool show_post_statistics { get; set; }
        public bool has_mail { get; set; }
        public bool receive_email_notifications { get; set; }
        public bool enable_keyboard_navigation { get; set; }
        public bool enable_privacy_mode { get; set; }
        public bool style_usernames { get; set; }
        public bool enable_auto_complete { get; set; }
        public bool has_saved_searches { get; set; }
        public bool disable_cropped_thumbnails { get; set; }
        public bool disable_mobile_gestures { get; set; }
        public bool enable_safe_mode { get; set; }
        public bool disable_responsive_mode { get; set; }
        public bool disable_post_tooltips { get; set; }
        public bool no_flagging { get; set; }
        public bool no_feedback { get; set; }
        public bool disable_user_dmails { get; set; }
        public bool enable_compact_uploader { get; set; }
        public DateTime updated_at { get; set; }
        public string email { get; set; }
        public DateTime last_logged_in_at { get; set; }
        public DateTime last_forum_read_at { get; set; }
        public string recent_tags { get; set; }
        public int comment_threshold { get; set; }
        public string default_image_size { get; set; }
        public string favorite_tags { get; set; }
        public string blacklisted_tags { get; set; }
        public string time_zone { get; set; }
        public int per_page { get; set; }
        public string custom_style { get; set; }
        public int favorite_count { get; set; }
        public int api_regen_multiplier { get; set; }
        public int api_burst_limit { get; set; }
        public int remaining_api_limit { get; set; }
        public int statement_timeout { get; set; }
        public int favorite_limit { get; set; }
        public int tag_query_limit { get; set; }
    }
}
