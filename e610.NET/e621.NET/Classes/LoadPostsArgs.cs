using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace e610.NET
{
    public class LoadPostsArgs
    {
        public string tags;
        public int page;

        public LoadPostsArgs(string t, int l)
        {
            tags = t;
            page = l;
        }
    }
}
