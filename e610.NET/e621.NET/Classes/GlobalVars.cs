using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace e610.NET
{
    public class GlobalVars
    {
        // Next View Post
        // Set - Post Clicked on in PostsView
        // Get - Post to Load in the SinglePostView
        public static Post nvPost;

        // Holds the Posts loaded from a search. Used to set PostView back without sending another api request
        public static PostsViewModel ViewModel { get; set; }

        // Holds the text in the searchbox to pass between pages
        public static string searchText;

        // True if the PostsView needs to load new posts on load
        public static bool newSearch;

        // The number of posts to load
        public static int postCount;

        // the current page
        public static int pageCount;

        // Login Info
        public static string Username;
        public static string APIKey;

        public static int currentFrame;
    }
}
