using Reddit.Controllers;
using Reddit.Controllers.EventArgs;
using System;
using System.Text;


namespace RddtPoller
{

  /// <summary>
  /// This class polls the given Reddit subreddit for all new comments made while
  /// this application is running. It also gathers statistics, such as:
  /// - Total number of comments made during this run time
  /// - Most recent comment
  /// - User who made the most comments
  /// - Comment with the most upvotes
  /// - Comment with the most downvotes
  /// </summary>
  public class CommentPoller : IPoller, ICommentPoller
  {
    // Private members with properties
    private Subreddit sub;
    public Subreddit Sub { get => sub; set => sub = value; }

    private Post post;
    public Post Post { get => post; set => post = value; }


    // Private members
    List<Comment> comments;
    private Dictionary<string, int> UsersAndNumComments;
    private Dictionary<string, int> CommentAndNumUVotes;
    private Dictionary<string, int> CommentAndNumDVotes;


    /// <summary>
    /// Constructor.
    /// This instance will poll the top post in the given subreddit for new comments.
    /// </summary>
    /// <param name="sub">
    /// Subreddit object that represents the actual subreddit to be polled
    /// </param>
    /// <param name="subName">
    /// String containing the name of the subreddit
    /// </param>
    public CommentPoller(Subreddit sub, string subName)
    {
      this.sub = sub;
      this.sub.Name = subName;
      this.comments = new List<Comment>();
      this.UsersAndNumComments = new Dictionary<string, int>();
      this.CommentAndNumUVotes = new Dictionary<string, int>();
      this.CommentAndNumDVotes = new Dictionary<string, int>();

      // Get the top post in the subreddit.
      // This is the post that will be watched for new comments
      this.post = this.sub.Posts.GetTop("day")[0];
    }


    /// <summary>
    /// This method adds a new comment event to the built-in event handler in the Reddit.NET
    /// library, and calls its MonitorNew() method to start monitoring this subreddit and post.
    /// 
    /// Please note that this monitoring/polling is handled 100% by the Reddit.NET library.
    /// Reddit has rate limits, and this library adheres to these limits. The polling should
    /// only hit Reddit's API's once per second on average, with a limit of 60 per minute.
    /// </summary>
    public void StartMonitoring()
    {
      // Retrieve new comments to prepare for anything new that comes in after right now
      this.sub.Comments.GetNew();

      this.sub.Comments.NewUpdated += NewCommentAddedEventHandler;
      this.sub.Comments.MonitorNew();
    }


    /// <summary>
    /// This method calls the Reddit.NET library's MonitorNew() method to stop monitoring this
    /// subreddit and post, and then removes the new comment event that was added to the built-in
    /// event handler when monitoring was started.
    /// </summary>
    public void StopMonitoring()
    {
      this.sub.Comments.MonitorNew();
      this.sub.Comments.NewUpdated -= NewCommentAddedEventHandler;
    }


    /// <summary>
    /// Custom event handler that prints out each new comment, adds it to the comments list,
    /// and updates the statistical counts.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">
    /// List of new comments
    /// </param>
    private void NewCommentAddedEventHandler(object sender, CommentsUpdateEventArgs e)
    {
      foreach (Comment comment in e.Added)
      {
        Console.Write($"New comment in [r/{comment.Subreddit}/{comment.Root.Title}]: author [u/{comment.Author}], date [");
        Console.WriteLine($"{comment.Created.ToString("MM/dd/yyyy HH:mm:ss")}], comment [{ShortenCommentForPreview(comment.Body)}...]\n");

        // Save this comment in the posts list
        comments.Add(comment);

        // Update the counts
        UpdateUserCount(comment);
        UpdateUvoteCount(comment);
        UpdateDvoteCount(comment);
      }
    }


    /// <summary>
    /// Shortens comments longer than 50 chars to the preview length of 50 chars;
    /// otherwise, returns the comment as is
    /// </summary>
    /// <param name="comment">
    /// String containing the body of a comment
    /// </param>
    /// <param name="limit">
    /// OPTIONAL! Int indicating the size limit for the length of the preview comment;
    /// defaults to 50 if not provided
    /// </param>
    /// <returns>
    /// String containing the body of a comment, limited to 50 chars
    /// </returns>
    public string ShortenCommentForPreview(string comment, int limit = 50)
    {
      string commentPreview = string.Empty;

      // Length is over the limit, so truncate this comment
      if ((!string.IsNullOrEmpty(comment)) && (comment.Length > limit))
      {
        commentPreview = comment.Substring(0, limit);
      }

      // Under the limit, use as is
      else
      {
        commentPreview = comment;
      }

      return commentPreview;
    }


    /// <summary>
    /// If this is the first time this user has commented while this application has
    /// been running, the user is just added into the dictionary. If not, then
    /// the user's count is incremented.
    /// </summary>
    /// <param name="comment">
    /// A new comment
    /// </param>
    private void UpdateUserCount(Comment comment)
    {
      int count = 0;

      // This user already exists in the dictionary, so increment its count
      if (this.UsersAndNumComments.TryGetValue(comment.Author, out count))
      {
        this.UsersAndNumComments[comment.Author] = count + 1;
      }

      // First time seeing this user, so add it
      else
      {
        this.UsersAndNumComments.Add(comment.Author, 1);
      }
    }


    /// <summary>
    /// If this is the first time this comment has been upvoted while this application
    /// has been running, the comment's ID is just added into the dictionary. If not,
    /// then the comment's upvote count is updated.
    /// </summary>
    /// <param name="comment">
    /// A comment with a new upvote
    /// </param>
    private void UpdateUvoteCount(Comment comment)
    {
      int upvoteCount = 0;

      // There is at least 1 upvote for this comment
      if (comment.UpVotes > 0)
      {
        // This comment already exists in the dictionary, so increment its count
        if (this.CommentAndNumUVotes.TryGetValue(comment.Id, out upvoteCount))
        {
          this.CommentAndNumUVotes[comment.Id] = comment.UpVotes;
        }

        // First time seeing this comment, so update it
        else
        {
          this.CommentAndNumUVotes.Add(comment.Id, comment.UpVotes);
        }
      }
    }


    /// <summary>
    /// If this is the first time this comment has been downvoted while this application
    /// has been running, the comment's ID is just added into the dictionary. If not,
    /// then the comment's downvote count is updated.
    /// </summary>
    /// <param name="comment">
    /// A comment with a new downvote
    /// </param>
    private void UpdateDvoteCount(Comment comment)
    {
      int downvoteCount = 0;

      // There is at least 1 downvote for this comment
      if (comment.DownVotes > 0)
      {
        // This comment already exists in the dictionary, so update its count
        if (this.CommentAndNumDVotes.TryGetValue(comment.Id, out downvoteCount))
        {
          this.CommentAndNumDVotes[comment.Id] = comment.DownVotes;
        }

        // First time seeing this comment, so add it
        else
        {
          this.CommentAndNumDVotes.Add(comment.Id, comment.DownVotes);
        }
      }
    }


    /// <summary>
    /// Returns the current size of the comments list.
    /// </summary>
    /// <returns>
    /// Int containing the count of comments in the list
    /// </returns>
    public int RetrieveTotalNumberOfComments()
    {
      return this.comments.Count;
    }


    /// <summary>
    /// Finds the user who has made the most comments while this application has been
    /// running, and returns that user name.
    /// </summary>
    /// <returns>
    /// String containing the username with the most comments; otherwise, empty string.
    /// </returns>
    public string RetrieveMostActiveCommentingUser()
    {
      string activeUser = string.Empty;

      // At least one user commented
      if (this.UsersAndNumComments.Count > 0)
      {
        // Greatest number of comments made
        int mostComments = this.UsersAndNumComments.Values.Max();

        foreach (string username in this.UsersAndNumComments.Keys)
        {
          // Pick the first one found
          // In real-world implementation, there should be a tie-breaker
          // or change this method to return a list instead
          if (this.UsersAndNumComments[username] == mostComments)
          {
            activeUser = username;
            break;
          }
        }
      }

      return activeUser;
    }


    /// <summary>
    /// Simply returns the last comment added to the comments list, because they are
    /// inserted in the same order in which they are encountered.
    /// </summary>
    /// <returns>
    /// Last comment encountered; otherwise, null.
    /// </returns>
    public Comment RetrieveMostRecentComment()
    {
      Comment mostRecentComment = null;

      // At least one comment has been made
      if (this.comments.Count > 0)
      {
        mostRecentComment = comments.Last();
      }

      return mostRecentComment;
    }


    /// <summary>
    /// Finds the comment that has been downvoted the most while this application has been
    /// running, and returns that comment instance.
    /// </summary>
    /// <returns>
    /// Comment with the most downvotes; otherwise, null.
    /// </returns>
    public Comment RetrieveCommentWithMostDownVotes()
    {
      Comment dislikedComment = null;

      // At least one comment has been downvoted
      if (this.CommentAndNumDVotes.Count > 0)
      {
        // Greatest number of downvotes made
        int mostDvotes = this.CommentAndNumDVotes.Values.Max();

        // First look for the number of downvotes
        foreach (string ID in this.CommentAndNumDVotes.Keys)
        {
          if (this.CommentAndNumDVotes[ID] == mostDvotes)
          {
            // Then use that comment ID to find the same comment in the list
            foreach (Comment comment in comments)
            {
              // Pick the first one found
              // In real-world implementation, there should be a tie-breaker
              // or change this method to return a list instead
              if (comment.Id == ID)
              {
                dislikedComment = comment;
                break;
              }
            }
          }
        }
      }

      return dislikedComment;
    }


    /// <summary>
    /// Finds the comment that has been upvoted the most while this application has been
    /// running, and returns that comment instance.
    /// </summary>
    /// <returns>
    /// Comment with the most upvotes; otherwise, null.
    /// </returns>
    public Comment RetrieveCommentWithMostUpVotes()
    {
      Comment likedComment = null;

      // At least one comment has been upvoted
      if (this.CommentAndNumUVotes.Count > 0)
      {
        // Greatest number of upvotes made
        int mostUvotes = this.CommentAndNumUVotes.Values.Max();

        // First look for the number of upvotes
        foreach (string ID in this.CommentAndNumUVotes.Keys)
        {
          if (this.CommentAndNumUVotes[ID] == mostUvotes)
          {
            // Then use that comment ID to find the same comment in the list
            foreach (Comment comment in comments)
            {
              // Pick the first one found
              // In real-world implementation, there should be a tie-breaker
              // or change this method to return a list instead
              if (comment.Id == ID)
              {
                likedComment = comment;
                break;
              }
            }
          }
        }
      }

      return likedComment;
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
      finalStats.Append($":::::::::::::::::::::::::: FINAL STATISTICS FOR *** [r/{this.sub.Name}/{this.post.Title}]");
      finalStats.AppendLine(" *** SUBREDDIT ::::::::::::::::::::::::::\n");

      // Total count of all comments encountered
      finalStats.AppendLine($"Total New Comments: {this.comments.Count}");

      // The user who commented the most
      string username = RetrieveMostActiveCommentingUser();
      if (!string.IsNullOrEmpty(username))
      {
        finalStats.AppendLine($"Most Active User: u/{username}\n");
      }
      else
      {
        finalStats.AppendLine($"Most Active User: --\n");
      }

      // The last comment made
      Comment mostRecentComment = RetrieveMostRecentComment();
      if (mostRecentComment != null)
      {
        finalStats.AppendLine($"Most Recent Comment: ID [{mostRecentComment.Id}], Comment [{ShortenCommentForPreview(mostRecentComment.Body)}...]");
        finalStats.Append($"                         User [u/{mostRecentComment.Author}], Date ");
        finalStats.AppendLine($"[{mostRecentComment.Created.ToString("MM/dd/yyyy HH:mm:ss")}]\n");
      }
      else
      {
        finalStats.AppendLine($"Most Recent Comment: --\n");
      }

      // Comment with the most upvotes
      Comment mostLikedComment = RetrieveCommentWithMostUpVotes();
      if (mostLikedComment != null)
      {
        finalStats.AppendLine($"Comment with Most Upvotes: ID [{mostLikedComment.Id}], Comment [{ShortenCommentForPreview(mostLikedComment.Body)}...]");
        finalStats.AppendLine($"                           Upvotes [{mostLikedComment.UpVotes}], User [u/{mostLikedComment.Author}]");
        finalStats.AppendLine($"                           Date [{mostLikedComment.Created.ToString("MM/dd/yyyy HH:mm:ss")}]\n");
      }
      else
      {
        finalStats.AppendLine($"Comment with Most Upvotes: --\n");
      }

      // Comment with the most downvotes
      Comment mostDislikedComment = RetrieveCommentWithMostDownVotes();
      if (mostDislikedComment != null)
      {
        finalStats.AppendLine($"Comment with Most Downvotes: ID [{mostDislikedComment.Id}], Comment [{ShortenCommentForPreview(mostDislikedComment.Body)}...],");
        finalStats.AppendLine($"                             Downvotes [{mostDislikedComment.DownVotes}], User [u/{mostDislikedComment.Author}]");
        finalStats.AppendLine($"                             Date [{mostDislikedComment.Created.ToString("MM/dd/yyyy HH:mm:ss")}]\n");
      }
      else
      {
        finalStats.AppendLine($"Comment with Most Downvotes: --\n");
      }

      return finalStats.ToString();
    }

  }
}
