using __CoreGameLib._Scripts._Services._Leaderboards;
using _game._LevelsProviding;
using _Gameplay._UI;
using _UI;
using _UI.Screens;
using core.ads;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour {
    [Inject] private SignalBus _signalBus;
    [Inject] private ILevelProvider _levelProvider;
    [Inject] private GameplayController _gameplayController;
    [Inject] private PlayerProgressService _playerProgressService;
    [Inject] private IAdsService _adsService;
    [Inject] private ILeaderboardService _leaderboardService;
    [Inject] private UIManager _uiManager;

    [SerializeField] private LoseDialog _loseScreen;

    private void Start() {
        DOTween.SetTweensCapacity(9000, 5000);
        
        _signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
        _signalBus.Subscribe<NextLevelClickedSignal>(LoadNextLevel);
        _signalBus.Subscribe<RestartLevelClickedSignal>(RestartCurrentLevel);
        _signalBus.Subscribe<GameOverSignal>(ShowLoseDialog);
        _signalBus.Subscribe<ProcessAdForReviveSignal>(ProcessReviveAd);
    }

    private void OnDestroy() {
        _signalBus.Unsubscribe<LevelCompletedSignal>(OnLevelCompleted);
        _signalBus.Unsubscribe<NextLevelClickedSignal>(LoadNextLevel);
        _signalBus.Unsubscribe<RestartLevelClickedSignal>(RestartCurrentLevel);
        _signalBus.Unsubscribe<GameOverSignal>(ShowLoseDialog);
        _signalBus.Unsubscribe<ProcessAdForReviveSignal>(ProcessReviveAd);
    }

    private void OnLevelCompleted() {
        // prepare level results
        var snakesCount = _gameplayController.SnakesCountForLevel;
        var stars = 10; // example: should be taken from level data
        var coins = snakesCount * 2; 

        // get and setup win screen
        var winScreen = _uiManager.GetScreen<WinScreen>();
        winScreen.Setup(stars, coins);
        winScreen.OnClosed += HandleWinScreenClosed;
        
        _uiManager.ShowScreen<WinScreen>();
        
        // internal progress tracking
        //_playerProgressService.AddScore(snakesCount);
        _playerProgressService.CompleteLevel();
    }

    private void HandleWinScreenClosed() {
        var winScreen = _uiManager.GetScreen<WinScreen>();
        winScreen.OnClosed -= HandleWinScreenClosed;

        // decision logic: where to go next
        if (winScreen.DidUnlock) {
            // transition to meta game progress
            _uiManager.ShowScreen<MainScreen>();
        } else {
            // just start next level
            LoadNextLevel();
        }
    }

    private void LoadNextLevel() {
        _uiManager.ShowScreen<TransitionScreen>(() => {
            _gameplayController.ClearLevel();
            _gameplayController.LoadLevel();
            _uiManager.ShowScreen<GameplayScreen>();
        });
    }

    private void RestartCurrentLevel() {
        _uiManager.ShowScreen<TransitionScreen>(() => {
            _gameplayController.ClearLevel();
            _gameplayController.LoadLevel();
            _uiManager.ShowScreen<GameplayScreen>();
        });
    }

    private void ShowLoseDialog() {
        DOVirtual.DelayedCall(0.5f, () => _loseScreen.Show(_gameplayController.ReviveCount));
    }

    private void ProcessReviveAd() {
        _adsService.ShowRewarded(
            onRewardGranted: () => {
                _signalBus.Fire<ReviveSignal>();
            }, 
            onAdClosed: null
        );
    }
}