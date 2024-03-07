using Reddit.Controllers;
using System;


namespace RddtPoller
{
  /// <summary>
  /// This interface defines the contract for a Post Poller instance that is intended to be
  /// used with IPoller, which polls a specific Reddit subreddit for new posts. This interface
  /// has methods that return statistics gathered those posts.
  /// </summary>
  public interface IPostPoller
  {
    int RetrieveTotalNumberOfPosts();
    Post RetrieveMostRecentPost();
    Post RetrievePostWithMostUpvotes();
    Post RetrievePostWithMostDownvotes();
    string RetrieveMostActivePostingUser();
  }
}
