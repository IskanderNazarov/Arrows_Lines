using __CoreGameLib._Scripts._Services._Leaderboards;
using __Gameplay;
using _game._LevelsProviding;
using _Zenject;
using core.ads;
using DG.Tweening;
using Game.Boosters;
using GamePush;
using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour {
    [Inject] private SignalBus _signalBus;
    [Inject] private ILevelProvider _levelProvider;
    [Inject] private GameplayController _gameplayController;
    [Inject] private PlayerProgressService _playerProgressService;
    [Inject] private IAdsService _adsService;
    [Inject] private ILeaderboardService _leaderboardService;
    [Inject] private GameBoosterInventory _boosterInventory;


    [Header("UI")] [SerializeField] private WinScreen _winScreen;
    [SerializeField] private LoseDialog _loseScreen;

    private void Start() {
        DOTween.SetTweensCapacity(9000, 5000);
        // Подписываемся на события
        _signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
        _signalBus.Subscribe<NextLevelClickedSignal>(LoadNextLevel);
        _signalBus.Subscribe<RestartLevelClickedSignal>(RestartCurrentLevel);
        _signalBus.Subscribe<BoosterBtnClickSignal>(OnBoosterButtonClicked);
        _signalBus.Subscribe<GameOverSignal>(ShowLoseDialog);
        _signalBus.Subscribe<ProcessAdForReviveSignal>(ProcessReviveAd);
        _signalBus.Subscribe<OnSnakeClickedSignal>(OnSnakeClicked);
    }

    private void OnDestroy() {
        // Отписываемся от событий
        _signalBus.Unsubscribe<LevelCompletedSignal>(OnLevelCompleted);
        _signalBus.Unsubscribe<NextLevelClickedSignal>(LoadNextLevel);
        _signalBus.Unsubscribe<RestartLevelClickedSignal>(RestartCurrentLevel);
        _signalBus.Unsubscribe<BoosterBtnClickSignal>(OnBoosterButtonClicked);
        _signalBus.Unsubscribe<GameOverSignal>(ShowLoseDialog);
        _signalBus.Unsubscribe<ProcessAdForReviveSignal>(ProcessReviveAd);
        _signalBus.Unsubscribe<OnSnakeClickedSignal>(OnSnakeClicked);
    }

    private void OnSnakeClicked() {
        _adsService.ShowInterstitial(null);
    }

    private void OnLevelCompleted() {
        GP_Analytics.Goal("level_completed_i", _playerProgressService.CurrentLevelIndex);
        GP_Analytics.Goal("level_completed_s", _playerProgressService.CurrentLevelIndex.ToString());

        // Говорим сервису прогресса, что уровень пройден
        _playerProgressService.CompleteLevel();
        var snakesCount = _gameplayController.SnakesCountForLevel;
        _playerProgressService.AddScore(snakesCount);

        Debug.Log("GM: Level Complete. Next level index: " + _playerProgressService.CurrentLevelIndex);
        _winScreen.Show(snakesCount);
    }

    // Этот метод теперь приватный и вызывается только по сигналу от шины
    private void LoadNextLevel() {
        _winScreen.Hide(delegate {
            _gameplayController.ClearLevel();
            _gameplayController.LoadLevel();

            GP_Analytics.Goal("level_started_i", _playerProgressService.CurrentLevelIndex);
            GP_Analytics.Goal("level_started_s", _playerProgressService.CurrentLevelIndex.ToString());
        });
    }

    private void RestartCurrentLevel(RestartLevelClickedSignal a) {
        Debug.Log($"GM: Restarting current level..., a.levelNumber: {a.levelNumber}'}}");

        // Так как индекс в LevelProvider не менялся, загрузится текущий уровень.
        _gameplayController.ClearLevel();
        _gameplayController.LoadLevel(a.levelNumber);
    }

    public void OnBoosterButtonClicked(BoosterBtnClickSignal boosterBtnClickSignal) {
        var boosterId = boosterBtnClickSignal.BoosterId;
        var startPos = boosterBtnClickSignal.StartPos;

        // 1. Проверяем, есть ли бустер в инвентаре
        if (_boosterInventory.GetCount(boosterId) > 0) {
            // 2. Передаем команду в GameplayController. 
            _gameplayController.TryUseBooster(boosterId, startPos.position, () => {
                // 3. Списываем бустер
                _boosterInventory.TryConsume(boosterId, 1, "use_in_game");

                // Здесь же можно проиграть звук списания бустера, если нужно
            });
        } else {
            // Показать окно покупки бустера или предложить посмотреть рекламу
            Debug.Log($"[GameManager] Недостаточно бустеров: {boosterId}. Запуск рекламы...");
            // _adsService.ShowRewarded(...);
        }
    }

    private void ShowLoseDialog() {
        DOVirtual.DelayedCall(0.5f, delegate { _loseScreen.Show(_gameplayController.ReviveCount); });
    }

    private void ProcessReviveAd() {
        // GameManager обращается к сервису рекламы
        _adsService.ShowRewarded(
            placement: "revive_rewarded",
            onRewardGranted: () => {
                Debug.Log("Реклама успешно просмотрена, возрождаем игрока!");
                // Отправляем сигнал возрождения, который восстановит жизни в GameplayController
                _signalBus.Fire<ReviveSignal>();
            }, onAdClosed: null /*,
            onFailed: () => {
                Debug.Log("Ошибка показа рекламы");
                // Если реклама не загрузилась, можно снова показать LoseDialog
                _loseScreen.Show();
            }*/
        );
    }
}