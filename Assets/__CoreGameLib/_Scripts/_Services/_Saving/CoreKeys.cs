using System.Collections.Generic;

namespace __CoreGameLib._Scripts._Services._Saving {
    public class CoreKeys {
        public const string CoinsKey = "coinsCount";
        public const string GemsKey = "gemsCount";
        public const string ScoreKey = "currentScore";
        public const string HighScoreKey = "high_score"; //this is for
        public const string LeaderboardScore = "score";

        public const string MusicOnKey = "IsMusicOn";
        public const string SoundOnKey = "IsSoundOn";
        
        public static readonly Dictionary<string, object> DefaultValues = new() {
            { CoinsKey, 0 }, 
            { GemsKey, 0 },
            { ScoreKey, 0 },
            { MusicOnKey, true },
            { SoundOnKey, true },
            { HighScoreKey, 0 },
            { LeaderboardScore, 0 },
        };
    }
}