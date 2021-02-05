# Search for artists with Machine Learning!

## What is this?
I'm trying to train a machine learning model to detect what artist drew an image.
If this works, anytime someone doesn't post the sauce, you can load an image into the e610 and get a list of closest matches to what artist might have drawn it.

## Why are you doing this?
Well I learn new concepts best by doing. Since I've been meaning to learn machine learning for awhile, I decided this project would be a good way to start
I also just want my sauce :eyes:<br><br>
(I also might have downloaded 2TB of images off the e621 database, for science, obviously)

## Now why would you download 2TB of images, Epsi?
I needed training data, However I ran into a problem. As you might have guessed, 2TB is a *lot* of data. So much that by the 900,000th image, 
My pc ran out of memory, and the training failed. So this next time I'll be using an Azure ML server. This only leaves me with 300GB(Unless I pay a lot more).
<br>
I'll be training a smaller dataset as a test, but I still would like to try and train the full dataset at a later date. 

## How can I help?
Well, thanks for asking! I'm currently working on getting a dataset for the first test What I need is more cropped images to help the AI get artist correct when aa full image
isn't given. So, if you happen to have any, memes or otherwise, DM me on twitter, [@EpsilonRho](https://twitter.com/EpsilonRho), with those images **and** the name of the artist 
that drew the original. Once I get a bunch of those, the top artists with the most images(up to 300GB) will be used in the dataset.

## When / Where will I be able to use this tool?
It will be built into the e610.NET app first, though I may make a standalone app for it as well. As for when, that'll be whenever it's done training and I can Implement the 
model into the program. Training will start whenever I get enough images, so please contribute if you can!

## Do you use any of my data?
No. The model is saved in the program itself, and will never talk to any server of my own. e610 only talks to the e621 api. <br>
There is one issue, though. I can't retrain it as of now. Microsoft's ML.Net doesn't seem to support retraining of image classification models. So any improvemnts will have to 
come with updates as I train new models with more data.

## Will it be free?
Absolutely! It'll also be fully open source, I'll even host the model on this github if you want to use it in your own projects (it'll be saved as .onnx)<br>
I program for fun, and I'm learning a lot of interesting stuff as I do. However, it can be difficult paying for things like high capacity Azure servers. So if you happen to 
feel like helping out and leaving a donation, I'd appreciate it a hell of a lot! You can donate to me [on Ko-fi](https://ko-fi.com/epsilonrho) if you'd like.

## What if it doesn't work that well?
Well I'll keep working on it to make it as best as I can, and if you have any suggestions or tips my DMs are always open. If it just doesn't work out though, 
I'll still have more than 2TB of e621 data saved, and I can always try to use it for something else, maybe other types of machine learning :wink:<br>
If anything, I'll keep working on e610 for awhile, as it's something I'll use to keep up with artists and tags I love on e621.
