using __CoreGameLib._Scripts._Services._Leaderboards;
using _Infrastructure.Services._Leaderboards;
using _Services._Saving;
using Core._Services;
using Core._Services._Saving;

namespace _Infrastructure {
    public class ScoreSetter {

        private CurrencyManager _currencyManager;
        private ILeaderboardService _leaderboardService;
        private IDataSaver _dataSaver;

        private ScoreSetter(CurrencyManager currencyManager, IDataSaver dataSaver,
            ILeaderboardService leaderboardService) {
            _currencyManager = currencyManager;
            _leaderboardService = leaderboardService;
            _dataSaver = dataSaver;
            
            //todo subscribe to events from different classes to catch score adding
            //_game.OnItemsMerged += OnItemsMerged;
            
        }
    }
}