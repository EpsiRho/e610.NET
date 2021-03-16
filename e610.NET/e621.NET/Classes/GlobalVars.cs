using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace e610.NET
{
    public class GlobalVars
    {
        // Holds the Posts loaded from a search. Used to set PostView back without sending another api request
        public static PostsViewModel ViewModel { get; set; }
        public static PostsViewModel PoolViewModel { get; set; }

        // Holds the text in the searchbox to pass between pages
        public static string searchText;

        // True if the PostsView needs to load new posts on load
        public static bool newSearch;
        public static bool newPool;

        // The number of posts to load
        public static int postCount;

        // the current page
        public static int pageCount;

        // Login Info
        public static string Username;
        public static string APIKey;

        public static Pool PoolName;

        public static LoggedInUser LoggedInInfo;

        public static string Rating;

        public static bool ShowComments;

        public static string Binding;

        public static bool MuteVolume;
    }
}
