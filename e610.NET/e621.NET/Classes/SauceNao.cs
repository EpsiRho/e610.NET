using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace e610.NET.Classes
{
    public class Header
    {
        public string user_id { get; set; }
        public string account_type { get; set; }
        public string short_limit { get; set; }
        public string long_limit { get; set; }
        public int long_remaining { get; set; }
        public int short_remaining { get; set; }
        public int status { get; set; }
        public int results_requested { get; set; }
        public string search_depth { get; set; }
        public double minimum_similarity { get; set; }
        public string query_image_display { get; set; }
        public string query_image { get; set; }
        public int results_returned { get; set; }
        public string similarity { get; set; }
        public string thumbnail { get; set; }
        public int index_id { get; set; }
        public string index_name { get; set; }
        public int dupes { get; set; }
    }

    public class Data
    {
        public List<string> ext_urls { get; set; }
        public string title { get; set; }
        public int fa_id { get; set; }
        public string author_name { get; set; }
        public string author_url { get; set; }
        public DateTime? created_at { get; set; }
        public string tweet_id { get; set; }
        public string twitter_user_id { get; set; }
        public string twitter_user_handle { get; set; }
        public int? danbooru_id { get; set; }
        public int? gelbooru_id { get; set; }
        public int? e621_id { get; set; }
        public object creator { get; set; }
        public string material { get; set; }
        public string characters { get; set; }
        public string source { get; set; }
        public string eng_name { get; set; }
        public string jp_name { get; set; }
        public int? pixiv_id { get; set; }
        public string member_name { get; set; }
        public int? member_id { get; set; }
        public string company { get; set; }
        public string getchu_id { get; set; }
        public int? seiga_id { get; set; }
        public int? anidb_aid { get; set; }
        public string part { get; set; }
        public string year { get; set; }
        public string est_time { get; set; }
        public string as_project { get; set; }
        public string da_id { get; set; }
    }

    public class Result
    {
        public Header header { get; set; }
        public Data data { get; set; }
    }

    public class SauceNao
    {
        public Header header { get; set; }
        public List<Result> results { get; set; }
    }
}
