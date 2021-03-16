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
        public char paginationChar;
        public int limit;

        public LoadPostsArgs(string t, int l, char pc = 'b')
        {
            tags = t;
            page = l;
            paginationChar = pc;
        }
    }
}
