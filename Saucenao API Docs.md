# Saucenao API

## Base URL

`https://saucenao.com/search.php?`

## Queries

`output_type=<0/1/2>`: How to return the response

0 -> Return in html

1 -> Return in xml (Not Implemented) 

2 -> Return in JSON

`api_key=<your api key>`: Required to use the API without logging in

`db=<index number or 999 for all>`: Search a specific index number or all without needing to generate a bitmask.

`dbmask=<bitmask>`: Mask for selecting indexes to enable for search

`dbmaski=<bitmask>`: Mask for selecting indexes to disable for search

`dedupe=<0|1|2>`: The amount of deduping (Defaults to 2)

0 -> No result Deduping

1 -> Consolidate booru results and dedupe by item id

2 -> All implemented dedupe methods such as by series name

`testmode=1`: Causes each index which has a match to output at most 1 for testing. 

`url=<URL>`: The URL to search the database for

## Returned data (In JSON)

### Header

`user_id`: The id of the user making the request

`account_type`: The account type of the user making the request

`short_limit`: The request limit for the current user every30 seconds

`long_limit`: The request limit for the current user every 24hrs

`short_limit`: The remaining amount of requests for the current user every 30 seconds

`long_limit`: The remaining amount of requests for the current user every 24hrs

`status`: The status of the request

0< = server side error

 = success

0> = client side error

`results_requested`: The amount of results requested

`search_depth`: 

`minimum_similarity`: The minimum amount of similarity to show a result

### Results

header{

​	`similarity`: How similar this result is to the search image

​	`thumbnail`: The URL to the thumbnail of a result

​	`index_id`: The id of the database this result was under

​	`index_name`: The name of the database this result was under

​	`dupes`:

}

data{

​	ext_urls[

​		// Holds URLs to posts with this result's image

​	]

​	`title`: The title of the post

}

## Index / Database IDs

0 -> H-Magazines

2 -> H-Game CG

3 -> DoujinshiDB

5 -> pixiv Images

8 -> Nico Nico Seiga

9 -> Danbooru

10 -> drawr Images

11 -> Nijie Images

12 -> Yande.re

15 -> Shutterstock

16 -> FAKKU

18 -> H-Misc (nH)

19 -> 2D-Market

20 -> MediBang

21 -> Anime

22 -> H-Anime

23 -> Movies

24 -> Shows

25 -> Gelbooru

26 -> Konachan

27 -> Sankaku Channel

28 -> Anime-Pictures.net

29 -> e621.net

30 -> Idol Complex

31 -> bcy.net Illust

32 -> bcy.net Cosplay

33 -> PortalGraphics.net

34 -> deviantArt

35 -> Pawoo.net

36 -> Madokami (Manga)

37 -> MangaDex

38 -> H-Misc (eH)

39 -> ArtStation

40 -> FurAffinity

41 -> Twitter

42 -> Furry Network

