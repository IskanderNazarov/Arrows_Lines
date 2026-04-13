using System;
using _Infrastructure.Services._Leaderboards;

namespace __CoreGameLib._Scripts._Services._Leaderboards {
    public interface ILeaderboardService {
        bool IsInitialized { get; }
        bool IsPlayerAuthorized { get; } // NEW: to check authorization status
        event Action OnInitialized;
        
        void SetPlayerScore(string leaderboardName, int score, Action<bool> onComplete);
        void ShowNativeLeaderboard(string leaderboardName, Action<bool> onComplete);
        void GetLeaderboardEntries(string leaderboardName, Action<LeaderboardData> onSuccess, Action<string> onError);
        
        // NEW: method to manually trigger authorization
        void AuthorizePlayer(Action<bool> onComplete);
    }
}
