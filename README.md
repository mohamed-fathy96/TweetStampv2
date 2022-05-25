# TweetStampv2

This is a website clone (based on https://tweetstamp.org) for a freelancing project built using ASP.NET MVC and utilizing Twitter's APIs, with the core Twitter Api utilized being Twitter's Account Activity API

# Details

It utilizes Twitter's account activity API, to receive a tweet link from user and timestamp that tweet. 

The website is built using ASP.NET MVC, it utilizes TweetInvi library (https://github.com/linvi/tweetinvi) to connect to Twitter's API and setup webhook to utilize Twitter's account activity API.

# Functionality

The application's main functionality is that users send a Tweet link (or mention them in a tweet) to the Twitter developer account used in the app, and using a Twitter API endpoint which utilizies webhooks, the tweet is sent to the application, the tweet is then converted to JSON and saved to a SQL Server database, it is also timestamped using MA3 Api and a page returns to the user containing the stamped Tweet, an embedded original Tweet, the tweet JSON, stringified JSON, the SHA256 Hash digest of the tweet, the timestamp proof and the timestamp verification information.

# Screenshots

![image](https://user-images.githubusercontent.com/94698429/167291723-04ec04d0-aa26-440c-a026-7b37ea5bcc2e.png)

![image](https://user-images.githubusercontent.com/94698429/167291765-d63ae8dd-d927-4bed-afa7-3899d7d21218.png)

![image](https://user-images.githubusercontent.com/94698429/167291775-b79bf405-a8a2-487f-8110-51faf413ef50.png)

![image](https://user-images.githubusercontent.com/94698429/167291792-a894d9f5-8510-48c1-9ece-7496cf1290ed.png)
