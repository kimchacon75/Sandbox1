using Reddit.Controllers;
using Reddit.Controllers.EventArgs;
using System;
using System.Text;


namespace RddtPoller
{

  /// <summary>
  /// This class polls the given Reddit subreddit for all new posts made while
  /// this application is running. It also gathers statistics, such as:
  /// - Total number of posts made during this run time
  /// - Most recent post
  /// - User who made the most posts
  /// - Post with the most upvotes
  /// - Post with the most downvotes
  /// </summary>
  public class PostPoller : IPoller, IPostPoller
  {
    // Private members with properties
    private Subreddit sub;
    public Subreddit Sub { get => sub; set => sub = value; }


    // Private members
    List<Post> posts;
    private Dictionary<string, int> UsersAndNumPosts;
    private Dictionary<string, int> PostAndNumUVotes;
    private Dictionary<string, int> PostAndNumDVotes;


    /// <summary>
    /// Constructor.
    /// This instance will poll the given subreddit for new posts.
    /// </summary>
    /// <param name="sub">
    /// Subreddit object that represents the actual subreddit to be polled
    /// </param>
    /// <param name="subName">
    /// String containing the name of the subreddit
    /// </param>
    public PostPoller(Subreddit sub, string subName)
    {
      this.sub = sub;
      this.sub.Name = subName;
      this.posts = new List<Post>();
      this.UsersAndNumPosts = new Dictionary<string, int>();
      this.PostAndNumUVotes = new Dictionary<string, int>();
      this.PostAndNumDVotes = new Dictionary<string, int>();

    }


    /// <summary>
    /// This method adds a new post event to the built-in event handler in the Reddit.NET
    /// library, and calls its MonitorNew() method to start monitoring this subreddit.
    /// 
    /// Please note that this monitoring/polling is handled 100% by the Reddit.NET library.
    /// Reddit has rate limits, and this library adheres to these limits. The polling should
    /// only hit Reddit's API's once per second on average, with a limit of 60 per minute.
    /// </summary>
    public void StartMonitoring()
    {
      // Retrieve new posts to prepare for anything new that comes in after right now.
      // NOTE: This intentionally uses a new list reference that will not be used again,
      // because this call draws a line marking now.
      List<Post> newFunnyPosts = this.sub.Posts.New;

      this.sub.Posts.NewUpdated += NewPostAddedEventHandler;
      this.sub.Posts.MonitorNew();
    }


    /// <summary>
    /// This method calls the Reddit.NET library's MonitorNew() method to stop monitoring this
    /// subreddit, and then removes the new post event that was added to the built-in event
    /// handler when monitoring was started.
    /// </summary>
    public void StopMonitoring()
    {
      this.sub.Posts.MonitorNew();
      this.sub.Posts.NewUpdated -= NewPostAddedEventHandler;
    }


    /// <summary>
    /// Custom event handler that prints out each new post, adds it to the posts list,
    /// and updates the statistical counts.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">
    /// List of new posts
    /// </param>
    private void NewPostAddedEventHandler(object sender, PostsUpdateEventArgs e)
    {
      // Handle all new posts
      foreach (Post post in e.Added)
      {
        Console.Write($"New post in [r/{post.Subreddit}]: ID [{post.Id}], author [u/{post.Author}], title [{post.Title}], ");
        Console.WriteLine($"date [{post.Created.ToString("MM/dd/yyyy HH:mm:ss")}]\n");

        // Save this post in the posts list
        posts.Add(post);

        // Update the counts
        UpdateUserCount(post);
        UpdateUvoteCount(post);
        UpdateDvoteCount(post);
      }
    }


    /// <summary>
    /// If this is the first time this user has posted while this application has
    /// been running, the user is just added into the dictionary. If not, then
    /// the user's count is incremented.
    /// </summary>
    /// <param name="post">
    /// A new post
    /// </param>
    private void UpdateUserCount(Post post)
    {
      int count = 0;

      // This user already exists in the dictionary, so increment its count
      if (this.UsersAndNumPosts.TryGetValue(post.Author, out count))
      {
        this.UsersAndNumPosts[post.Author] = count + 1;
      }

      // First time seeing this user, so add it
      else
      {
        this.UsersAndNumPosts.Add(post.Author, 1);
      }
    }


    /// <summary>
    /// If this is the first time this post has been upvoted while this application
    /// has been running, the post's ID is just added into the dictionary. If not,
    /// then the posts's upvote count is updated.
    /// </summary>
    /// <param name="post">
    /// A post with a new upvote
    /// </param>
    private void UpdateUvoteCount(Post post)
    {
      int upvoteCount = 0;

      // There is at least 1 upvote for this post
      if (post.UpVotes > 0)
      {
        // This post already exists in the dictionary, so increment its count
        if (this.PostAndNumUVotes.TryGetValue(post.Id, out upvoteCount))
        {
          this.PostAndNumUVotes[post.Id] = post.UpVotes;
        }

        // First time seeing this post, so update it
        else
        {
          this.PostAndNumUVotes.Add(post.Id, post.UpVotes);
        }
      }
    }


    /// <summary>
    /// If this is the first time this post has been downvoted while this application
    /// has been running, the post's ID is just added into the dictionary. If not,
    /// then the posts's downvote count is updated.
    /// </summary>
    /// <param name="post">
    /// A post with a new downvote
    /// </param>
    private void UpdateDvoteCount(Post post)
    {
      int downvoteCount = 0;

      // There is at least 1 downvote for this post
      if (post.DownVotes > 0)
      {
        // This post already exists in the dictionary, so increment its count
        if (this.PostAndNumUVotes.TryGetValue(post.Id, out downvoteCount))
        {
          this.PostAndNumUVotes[post.Id] = post.DownVotes;
        }

        // First time seeing this post, so update it
        else
        {
          this.PostAndNumUVotes.Add(post.Id, post.DownVotes);
        }
      }
    }


    /// <summary>
    /// Returns the current size of the posts list.
    /// </summary>
    /// <returns>
    /// Int containing the count of posts in the list
    /// </returns>
    public int RetrieveTotalNumberOfPosts()
    {
      return this.posts.Count;
    }


    /// <summary>
    /// Finds the user who has made the most posts while this application has been
    /// running, and returns that user name.
    /// </summary>
    /// <returns>
    /// String containing the username with the most posts; otherwise, empty string.
    /// </returns>
    public string RetrieveMostActivePostingUser()
    {
      string activeUser = string.Empty;

      // At least one user posted
      if (this.UsersAndNumPosts.Count > 0)
      {
        // Greatest number of posts made
        int mostPosts = this.UsersAndNumPosts.Values.Max();

        foreach (string username in this.UsersAndNumPosts.Keys)
        {
          // Pick the first one found
          // In real-world implementation, there should be a tie-breaker
          // or change this method to return a list instead
          if (this.UsersAndNumPosts[username] == mostPosts)
          {
            activeUser = username;
            break;
          }
        }
      }

      return activeUser;
    }


    /// <summary>
    /// Simply returns the last post added to the posts list, because they are inserted
    /// in the same order in which they are encountered.
    /// </summary>
    /// <returns>
    /// Last post encountered; otherwise, null.
    /// </returns>
    public Post RetrieveMostRecentPost()
    {
      Post mostRecentPost = null;

      // At least one post has been made
      if (this.posts.Count > 0)
      {
        mostRecentPost = posts.Last();
      }

      return mostRecentPost;
    }


    /// <summary>
    /// Finds the post that has been downvoted the most while this application has been
    /// running, and returns that post instance.
    /// </summary>
    /// <returns>
    /// Post with the most downvotes; otherwise, null.
    /// </returns>
    public Post RetrievePostWithMostDownvotes()
    {
      Post dislikedPost = null;

      // At least one post has been downvoted
      if (this.PostAndNumDVotes.Count > 0)
      {
        // Greatest number of downvotes made
        int mostDvotes = this.PostAndNumDVotes.Values.Max();

        // First look for the number of downvotes
        foreach (string ID in this.PostAndNumDVotes.Keys)
        {
          if (this.PostAndNumDVotes[ID] == mostDvotes)
          {
            // Then use that post ID to find the same post in the list
            foreach (Post post in posts)
            {
              // Pick the first one found
              // In real-world implementation, there should be a tie-breaker
              // or change this method to return a list instead
              if (post.Id == ID)
              {
                dislikedPost = post;
                break;
              }
            }
          }
        }
      }

      return dislikedPost;
    }


    /// <summary>
    /// Finds the post that has been upvoted the most while this application has been
    /// running, and returns that post instance.
    /// </summary>
    /// <returns>
    /// Post with the most upvotes; otherwise, null.
    /// </returns>
    public Post RetrievePostWithMostUpvotes()
    {
      Post likedPost = null;

      // At least one post has been upvoted
      if (this.PostAndNumUVotes.Count > 0)
      {
        // Greatest number of upvotes made
        int mostUvotes = this.PostAndNumUVotes.Values.Max();

        // First look for the number of upvotes
        foreach (string ID in this.PostAndNumUVotes.Keys)
        {
          if (this.PostAndNumUVotes[ID] == mostUvotes)
          {
            // Then use that post ID to find the same post in the list
            foreach (Post post in posts)
            {
              // Pick the first one found
              // In real-world implementation, there should be a tie-breaker
              // or change this method to return a list instead
              if (post.Id == ID)
              {
                likedPost = post;
                break;
              }
            }
          }
        }
      }

      return likedPost;
    }


    /// <summary>
    /// Builds and returns a formatted string containing all of the statistical info
    /// gathered while the application was running.
    /// </summary>
    /// <returns>
    /// String containing all of the stats gathered
    /// </returns>

    public string RetrieveFinalStats()
    {
      StringBuilder finalStats = new StringBuilder();
      finalStats.Append($":::::::::::::::::::::::::: FINAL STATISTICS FOR *** [r/{this.sub.Name}]");
      finalStats.AppendLine(" *** SUBREDDIT ::::::::::::::::::::::::::\n");

      // Total count of all posts encountered
      finalStats.AppendLine($"Total New Posts: {this.posts.Count}");

      // The user who posted the most
      string username = RetrieveMostActivePostingUser();
      if (!string.IsNullOrEmpty(username))
      {
        finalStats.AppendLine($"Most Active User: u/{username}\n");
      }
      else
      {
        finalStats.AppendLine($"Most Active User: --\n");
      }

      // The last post made
      Post mostRecentPost = RetrieveMostRecentPost();
      if (mostRecentPost != null)
      {
        finalStats.AppendLine($"Most Recent Post: ID [{mostRecentPost.Id}], Title [{mostRecentPost.Title}]");
        finalStats.Append($"                  User [u/{mostRecentPost.Author}], Date ");
        finalStats.AppendLine($"[{mostRecentPost.Created.ToString("MM/dd/yyyy HH:mm:ss")}]\n");
      }
      else
      {
        finalStats.AppendLine($"Most Recent Post: --\n");
      }

      // Post with the most upvotes
      Post mostLikedPost = RetrievePostWithMostUpvotes();
      if (mostLikedPost != null)
      {
        finalStats.AppendLine($"Post with Most Upvotes: ID [{mostLikedPost.Id}], Title [{mostLikedPost.Title}]");
        finalStats.AppendLine($"                        Upvotes [{mostLikedPost.UpVotes}], User [u/{mostLikedPost.Author}]");
        finalStats.AppendLine($"                        Date [{mostLikedPost.Created.ToString("MM/dd/yyyy HH:mm:ss")}]\n");
      }
      else
      {
        finalStats.AppendLine($"Post with Most Upvotes: --\n");
      }

      // Post with the most downvotes
      Post mostDislikedPost = RetrievePostWithMostDownvotes();
      if (mostDislikedPost != null)
      {
        finalStats.AppendLine($"Post with Most Downvotes: ID [{mostDislikedPost.Id}], Title [{mostDislikedPost.Title}],");
        finalStats.AppendLine($"                          Upvotes [{mostDislikedPost.DownVotes}], User [u/{mostDislikedPost.Author}]");
        finalStats.AppendLine($"                          Date [{mostDislikedPost.Created.ToString("MM/dd/yyyy HH:mm:ss")}]\n");
      }
      else
      {
        finalStats.AppendLine($"Post with Most Downvotes: --\n");
      }

      return finalStats.ToString();
    }

  }
}
