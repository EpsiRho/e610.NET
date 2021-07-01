# Version 1.1

- Right click to save images, videos, flash
- Pool View shows pool name, post count, description, and allows for quick downloading of a pool
- Harder Better Faster Stronger Tagsview (Treeview caused more problems than it was worth)
- Comments now load on posts(profile pictures don't show, not sure on how to get them)
- Posts show their descriptions now
- Better tag adding, Left click on tag to add it, Right click on tag to add with (-)
- Better page loading
- Better Images loading

# Version 1.2
- Post Favoriting added
- Fixed Downvoting
- Added info popup
- Added Accounts page
- Added Settings Popup
- Moved Safety toggle into Settings
- Added Movement buttons on SinglePostView
- Added Movement selector(choose to mvoe between search or pool)
- Reformatted Single Post Buttons
- Fixed Safety
- Moved connected pools list in single post view
- Added Image Resizing button(not functional yet)

# Version 1.3
- Tag autocomplete
- Fixed posts trying to load when no more posts are available
- Fixed favoriting
- Added Image resizing 
- Added setting to disable comments

# Version 1.4
- Adjusted background colors
- Reduced memory usage
- Moved buttons on home page
- Lowered api requests when going from post to post
- Blacklist is now shown on the account page(but is not editable)
- Swipe controls added
- Now built for ARM and ARM64
- Back button disabled due to an issue, will come back later

# Version 1.5
- Fixed description not hiding when closing a post by searching 
- Added Copy Post, Copy Post Link, and Copy Content Link to the post context menu
- Moved to new swipe gesture handling
- Fixed swiping on posts navigating through pages instead
- Fixed Tag Autocomplete not hiding after searching
- Fixed image clipping into tp bar
- Fixed image set to page height not changing size on window size change
- Changed top bar text when a post is open
- Updated pool loading
- Updated app settings
- A few changed were made in preparation for an upcoming feature, but they are disabled in this update

# Version 1.6
- Clear button added next to search box when typing
- New info popup design
- Followed tags Notifications
- Fixed description, pools, and comments not updating when switching posts
- Saved posts get tags saved to the file data
- Fixed video resizing
- Fixed account favorites viewing
- Removed old pages
- Added Tabs
  - Middle click posts to open them in another tab
  - Middle click/right click tags to open then in another tab

# Version 1.7

* Updated controls to use WinUI 2.6 (Windows 11 Style)
* Made description and comment Text selectable
* Made image saving more memory efficient and progress is more informative
* Added Experimental menu to settings
  * Here you can enable a beta version of the SauceNao Integration
    * SauceNao can be queried from within e610! Simply input your SauceNao API Key select an image from your desktop. 
    * Any e621 links found with a SauceNao query are opened in app, others are opened in your default browser.
* Fixed description text not wrapping when resizing the window
* Fixed Loading bar when e621 doesn't respond