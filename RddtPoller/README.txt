*************************************
*************************************
  REDDIT POLLER CONSOLE APPLICATION
*************************************
*************************************

This is a C# Console App that connects to specified Subreddits on www.reddit.com, and polls the subreddit(s) until the user interrupts. This application will periodically report a few quick stats on the console while it polls for new posts or comments. When the user interrupts, it displays a final list of statistics gathered on the posts or comments that came in while the application was running.

Please note that this application uses the Reddit.NET library (NuGet), which handles the OAuth2 authentication process, refreshing of tokens, polling of subreddits, threading, etc.

Reddit.NET can also be found here:
https://github.com/sirkris/Reddit.NET/



MAIN CLASSES
------------

1. PostPoller
This class polls the given Reddit subreddit for all new posts made while this application is running.

It also gathers statistics, such as:
- Total number of posts made during this run time
- Most recent post
- User who made the most posts
- Post with the most upvotes
- Post with the most downvotes


2. CommentPoller
This class polls the given Reddit subreddit for all new comments made while this application is running.

It also gathers statistics, such as:
- Total number of comments made during this run time
- Most recent comment
- User who made the most comments
- Comment with the most upvotes
- Comment with the most downvotes


3. RedditPoller
This class is the test driver, and contains the Main method.

The expected input arguments are:
- String containing the Application ID
- String containing the Refresh Token
- String containing the Application Secret

Usage Statement:
Usage: RddtPoller <Reddit ClientID> <Reddit RefreshToken> <Reddit AppSecret>

Using the input parameters, it authenticates with Reddit so that the rate limit is once per second (or 60 per minute). It then presents the user with 2 menus, and based on the selections made, starts monitoring posts or comments in one or more specified subreddits. When the user presses an interrupt key, it stops monitoring the subs, and after a 2nd interrupt key, it exits.




NOTES
-----

1. The goal of this project was just to make a quick app that can hit Reddit's API, and poll for posts and comments. This app is far from perfect, and could use some more tweaking in the future.


2. Once the user has made the 2 selections and the app gets going, there is a message that says to press any key to stop polling. If the app runs for a while this message will scroll off, so it's easy to forget that's how to stop it.


3. When the user presses a key during monitoring, it will stop and print out a final set of statistics. Sometimes it takes a few seconds for the threads to respond, and shut down.


4. Pressing any key once more will exit the app.



