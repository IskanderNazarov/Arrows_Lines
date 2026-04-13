using System.Collections.Generic;

namespace _Infrastructure.Services._Leaderboards {
// a container for leaderboard data retrieved from the service.
    public class LeaderboardData {
        public List<LeaderboardEntry> Entries { get; set; } = new List<LeaderboardEntry>();
        public LeaderboardEntry PlayerEntry { get; set; }
    }

// represents a single player's entry in the leaderboard.
    public class LeaderboardEntry {
        public int Rank { get; set; }
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public string PlayerId { get; set; }
        public string PlayerPhotoUrl { get; set; }
    }
}