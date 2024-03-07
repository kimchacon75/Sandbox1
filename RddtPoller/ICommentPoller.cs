using Reddit.Controllers;
using System;


namespace RddtPoller
{
  /// <summary>
  /// This interface defines the contract for a Comment Poller instance that is intended to be
  /// used with IPoller, which polls a specific Reddit subreddit for new comments. This interface
  /// has methods that return statistics gathered those comments.
  /// </summary>
  public interface ICommentPoller
  {
    int RetrieveTotalNumberOfComments();
    Comment RetrieveMostRecentComment();
    Comment RetrieveCommentWithMostUpVotes();
    Comment RetrieveCommentWithMostDownVotes();
    string RetrieveMostActiveCommentingUser();

  }
}
