using System;


namespace RddtPoller
{
  /// <summary>
  /// This interface defines the contract for a Poller instance that polls a specific
  /// Reddit subreddit for new posts.
  /// </summary>
  public interface IPoller
  {
    void StartMonitoring();
    void StopMonitoring();

    string RetrieveFinalStats();
  }
}
