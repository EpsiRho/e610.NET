using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace e610.NET
{
    public class VoteResponse
    {
        public int score { get; set; }
        public int up { get; set; }
        public int down { get; set; }
        public int our_score { get; set; }
    }
}
