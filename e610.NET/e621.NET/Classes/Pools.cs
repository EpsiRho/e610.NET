using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace e610.NET
{
    public class Pool
    {
        public int id { get; set; }
        public string name { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public int creator_id { get; set; }
        public string description { get; set; }
        public bool is_active { get; set; }
        public string category { get; set; }
        public bool is_deleted { get; set; }
        public List<int> post_ids { get; set; }
        public string creator_name { get; set; }
        public int post_count { get; set; }
    }
    
}
