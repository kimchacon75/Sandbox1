using Reddit;
using Reddit.Controllers;
using System;
using System.Collections.Generic;


namespace RddtPoller
{

  /// <summary>
  /// This is a Console App that connects to specified Subreddits on www.reddit.com,
  /// and polls the subreddit(s) until the user interrupts. This application will periodically
  /// report a few quick stats on the console while it polls for new posts or comments. When the
  /// user interrupts, it displays a final list of statistics gathered on the posts or comments
  /// that came in while the application was running.
  /// 
  /// Please note that this application uses the Reddit.NET library (NuGet), which handles the
  /// OAuth2 authentication process, refreshing of tokens, polling of subreddits, threading, etc.
  /// </summary>
  public class RedditPoller
  {
    // Reddit Client reference
    // RedditClient is part of the Reddit.NET library.
    private RedditClient redditClient;

    // Constants
    private const string ITEM_MENU_TYPE = "ITEM";
    private const string SUB_MENU_TYPE = "SUB";
    private const string POST_ITEM_TYPE = "POST";
    private const string CMMT_ITEM_TYPE = "COMMENT";


    /// <summary>
    /// Constructor.
    /// This constructor handles OAuth2 authentication, signing this app into Reddit as my user.
    /// </summary>
    /// <param name="appID">
    /// String containing the Applicaton ID (AKA Client ID)
    /// </param>
    /// <param name="refreshToken">
    /// String containing the refresh token for this application
    /// </param>
    /// <param name="appSecret">
    /// String containing the App Secret for this application
    /// </param>
    public RedditPoller(string appID, string refreshToken, string appSecret)
    {
      // Initialize RedditClient with the input paramters
      this.redditClient = new RedditClient(appId: appID, refreshToken: refreshToken, appSecret: appSecret);

      // Retrieve some info about the user authenticated via OAuth2
      User user = redditClient.Account.Me;
    }


    /// <summary>
    /// Main
    /// This method creates an instance of RedditPoller, and using the input parameters,
    /// authenticates with Reddit. It then presents the user with 2 menus, and based on
    /// the selections made, starts monitoring posts or comments in 1 or more specified
    /// subreddits. When the user presses an interrupt key, it stops monitoring the subs,
    /// and after a 2nd interrupt key, it exits.
    /// </summary>
    /// <param name="args">
    /// The expected input arguments are:
    /// - String containing the Application ID
    /// - String containing the Refresh Token
    /// - String containing the Application Secret
    /// </param>
    public static void Main(string[] args)
    {
      // This app expects 3 input parameters, so print the usage if at least 3 were
      // not provided
      if (args.Length < 3)
      {
        Console.WriteLine("Usage: RddtPoller <Reddit ClientID> <Reddit RefreshToken> <Reddit AppSecret>");
      }

      else
      {
        // Input parameters
        string appID = args[0];
        string refreshToken = args[1];
        string appSecret = args[2];

        // Instantiate the RedditPoller instance (test driver) and authenticate the user
        RedditPoller redditPoller = new RedditPoller(appID, refreshToken, appSecret);

        // Allows the user to select the item to poll
        int itemSelection = redditPoller.ProcessMenuSelection(ITEM_MENU_TYPE, "");

        // Allows the user to select the subreddit to poll
        int subSelection = -1;

        // Post item selected
        if (itemSelection == 1)
        {
          subSelection = redditPoller.ProcessMenuSelection(SUB_MENU_TYPE, POST_ITEM_TYPE);
        }

        // Comment item selected
        else
        {
          subSelection = redditPoller.ProcessMenuSelection(SUB_MENU_TYPE, CMMT_ITEM_TYPE);
        }

        // List of names of subreddits to be polled
        List<string> subNames = new List<string>();

        // Menu options are:
        // 1. r/funny
        // 2. r/gaming
        // 3. r/cats
        // 4. All 3! r/funny, r/gaming, and r/cats
        switch (subSelection)
        {
          case 1:
            subNames.Add("funny");
            break;

          case 2:
            subNames.Add("gaming");
            break;

          case 3:
            subNames.Add("cats");
            break;

          case 4:
            subNames.Add("funny");
            subNames.Add("gaming");
            subNames.Add("cats");
            break;
        }

        // Start polling
        // The user selected posts
        if (itemSelection == 1)
        {
          redditPoller.PollSubreddits(subNames, POST_ITEM_TYPE);
        }

        // The user selected comments
        else
        {
          redditPoller.PollSubreddits(subNames, CMMT_ITEM_TYPE);
        }
      }

      // Keep the console window open until the user presses another key
      Console.WriteLine("Press any key to exit........");
      Console.ReadKey();
    }


    /// <summary>
    /// Displays the menu options and loops until the user enters a valid selection,
    /// which includes the option to quit. If the user enters the quit selection, the
    /// program exits. This method handles both menus: items to be polled (posts vs
    /// comments); subreddit to be polled
    /// </summary>
    /// <param name="menuType">
    /// String indicating which menu to handle
    /// </param>
    /// <returns>
    /// Int containing the user's menu selection
    /// </returns>
    private int ProcessMenuSelection(string menuType, string itemType)
    {
      // Valid menu selections
      List<char> validChars = null;

      // Posts vs Comments
      if (menuType.Equals(ITEM_MENU_TYPE))
      {
        validChars = new List<char>() { '1', '2', 'Q', 'q' };

        // Print the menu and prompt the user for an item selection
        Console.WriteLine("WELCOME TO THE REDDIT POLLER!\n");
        PrintItemMenu();
      }

      // Subreddits
      else if (menuType.Equals(SUB_MENU_TYPE))
      {
        // Posts, provide all 4 options
        if (itemType.Equals(POST_ITEM_TYPE))
        {
          validChars = new List<char>() { '1', '2', '3', '4', 'Q', 'q' };
        }

        // Comments, provide 3 options
        else if (itemType.Equals(CMMT_ITEM_TYPE))
        {
          validChars = new List<char>() { '1', '2', '3', 'Q', 'q' };
        }

        // Print the menu and prompt the user for a subreddit selection
        PrintSubredditMenu(itemType);
      }

      char userSelection = Console.ReadKey(true).KeyChar;
      Console.WriteLine($"==> You entered: {userSelection}\n");

      // Keep prompting the user for a valid menu selection
      while (!validChars.Contains(userSelection))
      {
        Console.WriteLine("\n\n\n");
        
        // Posts vs Comments
        if (menuType.Equals(ITEM_MENU_TYPE))
        {
          PrintItemMenu();
        }

        // Subreddits
        else
        {
          PrintSubredditMenu(itemType);
        }

        userSelection = Console.ReadKey(true).KeyChar;
        Console.WriteLine($"==> You entered: {userSelection}\n");
      }

      // User selected Quit
      if (Char.ToUpper(userSelection) == 'Q')
      {
        Environment.Exit(0);
      }

      return int.Parse(userSelection.ToString());
    }


    /// <summary>
    /// Prints the item menu
    /// </summary>
    private void PrintItemMenu()
    {
      Console.WriteLine("Enter the option number of the item you wish to poll for:");
      Console.WriteLine("1. Posts");
      Console.WriteLine("2. Comments");
      Console.WriteLine("\nOr press Q to Quit");
    }


    /// <summary>
    /// Prints the subreddit menu
    /// </summary>
    private void PrintSubredditMenu(string itemType)
    {
      Console.WriteLine("Enter the option number of the subreddit(s) you wish to poll:");
      Console.WriteLine("1. r/funny");
      Console.WriteLine("2. r/gaming");
      Console.WriteLine("3. r/cats");

      // Posts, provide all 4 options
      // NOTE: Comments for all 3 subs will violate the rate limit rules.
      if (itemType.Equals(POST_ITEM_TYPE))
      {
        Console.WriteLine("4. All 3! r/funny, r/gaming, and r/cats");
      }

      Console.WriteLine("\nOr press Q to Quit");
    }


    /// <summary>
    /// Starts polling each of the subreddits in the given list for new items (posts or
    /// comments based on the user's selection), waits for the user to interrupt to stop
    /// polling -- printing out a few quick stats once a minute, stops polling, and then
    /// prints out the final list of stats.
    /// </summary>
    /// <param name="subNames">
    /// List containing subreddit names
    /// </param>
    /// <param name="itemType">
    /// String containing the item type to be polled for
    /// </param>
    private void PollSubreddits(List<string> subNames, string itemType)
    {
      // Polling intro message
      PrintPollingIntroMsg(subNames, itemType);

      // List of all of the sub pollers
      List<IPoller> subPollers = new List<IPoller>();

      // Create a sub poller for each sub name in the subNames list, and store them in
      // the subPollers list
      foreach (string subName in subNames)
      {
        Subreddit sub = redditClient.Subreddit(subName).About();

        // Posts
        if (itemType.Equals(POST_ITEM_TYPE))
        {
          PostPoller subPoller = new PostPoller(sub, subName);
          subPollers.Add(subPoller);
        }

        // Comments
        else if (itemType.Equals(CMMT_ITEM_TYPE))
        {
          CommentPoller subPoller = new CommentPoller(sub, subName);
          subPollers.Add(subPoller);
        }
      }

      // Start monitoring each subreddit
      StartMonitoringProcess(subPollers, itemType);

      // Poll until the user interrupts
      HandleWaitingForUserInterrupt(subPollers);

      // Stop monitoring each subreddit
      StopMonitoringProcess(subPollers, itemType);

      // Print the final stats each subreddit
      Console.WriteLine();

      foreach (IPoller subPoller in subPollers)
      {
        Console.WriteLine(subPoller.RetrieveFinalStats() + "\n");
      }

      Console.WriteLine();
    }


    /// <summary>
    /// Starts the monitoring process by calling the into the Reddit.NET library for
    /// this subreddit, and prints out a message notifying the user that monitoring
    /// has actually started.
    /// </summary>
    /// <param name="subNames">
    /// List containing subreddit names
    /// </param>
    ///<param name="itemType">
    /// String containing the item type to be polled for
    /// </param>
    private void StartMonitoringProcess(List<IPoller> subPollers, string itemType)
    {
      foreach (IPoller subPoller in subPollers)
      {
        subPoller.StartMonitoring();

        // Posts
        if (itemType.Equals(POST_ITEM_TYPE))
        {
          Console.WriteLine($"STARTED monitoring [r/{((PostPoller)subPoller).Sub.Name}] posts...........................................................\n");
        }

        // Comments
        else if (itemType.Equals(CMMT_ITEM_TYPE))
        {
          Console.WriteLine($"STARTED monitoring [r/{((CommentPoller)subPoller).Sub.Name}] comments...........................................................\n");
        }
      }
    }


    /// <summary>
    /// Stops the monitoring process by calling the into the Reddit.NET library for
    /// this subreddit, and prints out a message notifying the user that monitoring
    /// has actually stopped.
    /// </summary>
    /// <param name="subNames">
    /// List containing subreddit names
    /// </param>
    /// <param name="itemType">
    /// String containing the item type to be polled for
    /// </param>
    private void StopMonitoringProcess(List<IPoller> subPollers, string itemType)
    {
      foreach (IPoller subPoller in subPollers)
      {
        subPoller.StopMonitoring();

        // Posts
        if (itemType.Equals(POST_ITEM_TYPE))
        {
          Console.WriteLine($"STOPPED monitoring [r/{((PostPoller)subPoller).Sub.Name}] posts...........................................................\n");
        }

        // Comments
        else if (itemType.Equals(CMMT_ITEM_TYPE))
        {
          Console.WriteLine($"STOPPED monitoring [r/{((CommentPoller)subPoller).Sub.Name}] comments...........................................................\n");
        }
      }
    }


    /// <summary>
    /// Prints the polling intro message.
    /// </summary>
    /// <param name="subNames">
    /// List containing subreddit names
    /// </param>
    /// <param name="itemType">
    /// String containing the constant value for post or comment
    /// </param>
    private void PrintPollingIntroMsg(List<string> subNames, string itemType)
    {
      // Message for multiple subreddits selected
      if (subNames.Count > 1)
      {
        Console.WriteLine($"This app will poll the [r/{string.Join("], [r/", subNames)}] subreddits continuously, checking on average once per second");
      }

      // Message for only one subreddit selected
      else
      {
        Console.WriteLine($"This app will poll the [r/{subNames[0]}] subreddit continuously, checking on average once per second");
      }

      // Posts
      if (itemType.Equals(POST_ITEM_TYPE))
      {
        Console.WriteLine("for new posts, and will attempt display each post as it's made on Reddit.\n");
      }

      // Comments
      else
      {
        Console.WriteLine("for new comments, and will attempt display each comment as it's made on Reddit.\n");
      }

      Console.WriteLine("*****                               *****");
      Console.WriteLine("***** Press ANY key to stop polling *****");
      Console.WriteLine("*****                               *****\n");
    }


    /// <summary>
    /// When this method is called, the given Post/Comment Pollers are actively polling
    /// their corresponding subreddit for new posts/comments. This method waits for one
    /// minute at a time, and then prints out a few quick stats gathered by each Poller.
    /// The user can interrupt this method at any time by pressing any key, which will
    /// cause this method to stop and return to the calling method.
    /// </summary>
    /// <param name="subPollers">
    /// List containing subreddit Post/Comment Poller objects
    /// </param>
    private void HandleWaitingForUserInterrupt(List<IPoller> subPollers)
    {
      // Wait for 1 minute each time
      DateTime startDateTime = DateTime.Now;
      DateTime endDateTime = startDateTime.AddMinutes(1);

      // Waiting for the user to interrupt
      while (!Console.KeyAvailable)
      {
        // Wait for 1 minute unless the user interrupts
        while ((!Console.KeyAvailable) && (DateTime.Now < endDateTime))
        {
          // waiting...
        }

        // 1 minute has passed
        // Print some quick stats
        foreach (IPoller subPoller in subPollers)
        {
          // Post
          if (subPoller is PostPoller)
          {
            PrintQuickStatsForPosts((PostPoller)subPoller);
          }

          // Comment
          else if (subPoller is CommentPoller)
          {
            PrintQuickStatsForComments((CommentPoller)subPoller);
          }

        }

        // Set up the wait for 1 more minute
        startDateTime = DateTime.Now;
        endDateTime = startDateTime.AddMinutes(1);
      }

      // Catch the keypress that caused the above loops to quit
      Console.ReadKey(true);
    }


    /// <summary>
    /// Prints out a few quick statistics gathered so far by the given Post Poller
    /// instance when watching for new posts.
    /// </summary>
    /// <param name="subPoller">
    /// Post Poller instance that polls a specific subreddit
    /// </param>
    private void PrintQuickStatsForPosts(PostPoller subPoller)
    {
      Console.WriteLine($"\n:::::::::::::::::::::::::::::: Quick Stats for [r/{subPoller.Sub.Name}] ::::::::::::::::::::::::::::::");
      Console.WriteLine("Total Posts So Far: " + subPoller.RetrieveTotalNumberOfPosts());

      string mostActiveUser = subPoller.RetrieveMostActivePostingUser();

      if (!string.IsNullOrEmpty(mostActiveUser))
      {
        Console.WriteLine("User With Most Posts So Far: u/" + mostActiveUser);
      }
      else
      {
        Console.WriteLine("User With Most Posts So Far: --");
      }

      Post mostLikedPost = subPoller.RetrievePostWithMostUpvotes();

      if (mostLikedPost != null)
      {
        Console.WriteLine("Post With Most Upvotes So Far: " + mostLikedPost.Title + "\n");
      }
      else
      {
        Console.WriteLine("Post With Most Upvotes So Far: --\n");
      }
    }


    /// <summary>
    /// Prints out a few quick statistics gathered so far by the given Comment Poller
    /// instance when watching for new comments on a specific post.
    /// </summary>
    /// <param name="subPoller">
    /// Comment Poller instance that polls for comments
    /// </param>
    private void PrintQuickStatsForComments(CommentPoller subPoller)
    {
      Console.WriteLine($"\n:::::::::::::::::::::::::::::: Quick Stats for [r/{subPoller.Sub.Name}] ::::::::::::::::::::::::::::::");
      Console.WriteLine("Total Comments So Far: " + subPoller.RetrieveTotalNumberOfComments());

      string mostActiveUser = subPoller.RetrieveMostActiveCommentingUser();

      if (!string.IsNullOrEmpty(mostActiveUser))
      {
        Console.WriteLine("User With Most Comments So Far: u/" + mostActiveUser);
      }
      else
      {
        Console.WriteLine("User With Most Comments So Far: --");
      }

      Comment mostLikedComment = subPoller.RetrieveCommentWithMostUpVotes();

      if (mostLikedComment != null)
      {
        Console.WriteLine("Comment With Most Upvotes So Far: " + subPoller.ShortenCommentForPreview(mostLikedComment.Body) + "\n");
      }
      else
      {
        Console.WriteLine("Comment With Most Upvotes So Far: --\n");
      }
    }


  }
}
