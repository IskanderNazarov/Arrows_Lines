using System.Collections;
using System.Collections.Generic;
using __CoreGameLib._Scripts._Services._Leaderboards;
using __Gameplay;
using _ExtensionsHelpers;
using _ScriptableObjects._LevelData;
using _UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
// Для BoosterId

public class GameUI : MonoBehaviour {
    
    [Header("Boosters")]
    [SerializeField] private BoosterButton[] _boosterButtons; // Массив всех кнопок бустеров

    [Header("System Buttons")]
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _restartDebugButton;
    [SerializeField] private Button _resetLevelNumberButton;
    [SerializeField] private Button _lbButton;
    
    [Header("Hearts")]
    [SerializeField] private GameObject[] _hearts;
    [SerializeField] private HeartBreakAnim[] _heartsAnim;
    
    [Header("Score & UI")]
    [SerializeField] private TextMeshProUGUI _scoreCountTMP;
    [SerializeField] private TextMeshProUGUI _addedScoreTMP;
    [SerializeField] private GameObject _adSign;
    [SerializeField] private TMP_Dropdown dropdownDebug;

    [Header("Energy FX")]
    [SerializeField] private RectTransform energyToFly;
    [SerializeField] private ParticleSystem stars;
    [SerializeField] private RectTransform lbBtn;

    [Inject] private SignalBus _signalBus;
    [Inject] private SoundHelper _soundHelper;
    [Inject] private ILeaderboardService _leaderboardService;
    [Inject] private PlayerProgressService _playerProgressService;
    [Inject] private GameConfig _gameConfig;
    [Inject] private LevelsDatabase _levelsDatabase;
    [Inject] private GameBoosterInventory _boosterInventory;

    private void Start() {
        // 1. Инициализация бустеров
        foreach (var boosterBtn in _boosterButtons) {
            // Подписываемся на клик внутри кнопки
            boosterBtn.Initialize(OnBoosterClicked);
            // Задаем стартовое визуальное состояние
            boosterBtn.UpdateVisuals(_boosterInventory.GetCount(boosterBtn.BoosterId));
        }
        
        // Подписываемся на изменения количества бустеров в инвентаре
        _boosterInventory.OnChanged += OnBoosterInventoryChanged;

        // 2. Подписка остальных кнопок
        _restartButton.onClick.AddListener(() => _signalBus.Fire<RestartLevelClickedSignal>());
        _restartDebugButton.onClick.AddListener(() => _signalBus.Fire<RestartLevelClickedSignal>());
        _lbButton.onClick.AddListener(ShowNativeLeaderboard);
        _resetLevelNumberButton.onClick.AddListener(() => _playerProgressService.DeleteAllData());

        _signalBus.Subscribe<LivesChangedSignal>(UpdateHearts);

        // 3. Дебаг дропдаун уровней
        var N = _levelsDatabase.levelDatas.Count;
        var optionsList = new List<TMP_Dropdown.OptionData>();
        for (var i = 0; i < N; i++) {
            optionsList.Add(new TMP_Dropdown.OptionData(i.ToString()));
        }

        dropdownDebug.options = optionsList;
        dropdownDebug.onValueChanged.AddListener(LevelSelectedFromDropDown);
        
        foreach (var bb in _boosterButtons) {
            var count = _boosterInventory.GetCount(bb.BoosterId);
            bb.UpdateVisuals(count);
        }

        UpdateScoreVisual();
    }

    private void OnDestroy() {
        _signalBus.Unsubscribe<LivesChangedSignal>(UpdateHearts);
        _boosterInventory.OnChanged -= OnBoosterInventoryChanged;
        dropdownDebug.onValueChanged.RemoveListener(LevelSelectedFromDropDown);
    }

    // --- ЛОГИКА БУСТЕРОВ ---

    private void OnBoosterClicked(BoosterId id, Transform btnTransform) {
        // Передаем сигнал в GameManager
        _signalBus.Fire(new BoosterBtnClickSignal(id, btnTransform));
    }

    private void OnBoosterInventoryChanged(BoosterId id, int delta) {
        // Ищем нужную кнопку в массиве и обновляем ее UI
        int newCount = _boosterInventory.GetCount(id);

        foreach (var btn in _boosterButtons) {
            if (btn.BoosterId == id) {
                btn.UpdateVisuals(newCount);
                
                // Опционально: анимация прибавки бустера
                if (delta > 0) {
                    btn.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5);
                }
                break;
            }
        }
    }

    // --- ОСТАЛЬНАЯ ЛОГИКА ---

    private void UpdateScoreVisual() {
        _scoreCountTMP.text = _playerProgressService.Score.ToString();
    }

    private void ShowNativeLeaderboard() {
        var playerScore = _playerProgressService.Score;
        _leaderboardService.SetPlayerScore("score", playerScore,
            delegate { DOVirtual.DelayedCall(0.1f, delegate { _leaderboardService.ShowNativeLeaderboard("score", null); }); });
    }

    private void LevelSelectedFromDropDown(int index) {
        _signalBus.Fire(new RestartLevelClickedSignal { levelNumber = index });
    }

    private void UpdateHearts(LivesChangedSignal signal) {
        for (int i = 0; i < _hearts.Length; i++) {
            var wasActive = _hearts[i].activeSelf;
            var isActive = i < signal.CurrentLives;
            _hearts[i].SetActive(isActive);

            if (wasActive && !isActive) {
                _heartsAnim[i].PlayBreakAnim();
            }
        }
    }

    public void MoveEnergy(int addedScore, Vector2 energyStartPos, float delay) {
        StartCoroutine(MoveEnergyRoutine(addedScore, energyStartPos, delay));
    }

    public IEnumerator MoveEnergyRoutine(int addedScore, Vector2 energyStartPos, float delay) {
        energyToFly.gameObject.SetActive(true);
        energyToFly.position = energyStartPos;
        energyToFly.localScale = Vector3.one;
        _addedScoreTMP.text = addedScore.ToString();

        _soundHelper.EnergyStartMoveSound();
        yield return new WaitForSeconds(delay);

        stars.transform.SetParent(energyToFly);
        stars.transform.localPosition = Vector3.zero;
        stars.transform.localScale = Vector3.one * 65;

        yield return new WaitForSeconds(0.4f);
        stars.gameObject.SetActive(true);
        stars.Play();

        energyToFly.position = energyStartPos;
        const float dur = 1.3f;
        var sequence = DOTween.Sequence();
        sequence.Append(CreatePathForEnergyFlight(energyStartPos, dur).SetEase(Ease.InSine));
        sequence.Insert(dur * 0.2f, energyToFly.DOScale(1.1f, dur * 0.4f).SetEase(Ease.InOutSine));
        sequence.Insert(dur * 0.6f, energyToFly.DOScale(0.5f, dur * 0.4f).SetEase(Ease.InOutSine));
        yield return sequence.Play().WaitForCompletion();

        _soundHelper.PlayEnergyHitSound();
        lbBtn.DOPunchScale(Vector3.one * 0.1f, 0.75f, 3);
        UpdateScoreVisual();
        stars.transform.SetParent(energyToFly.parent);
        energyToFly.gameObject.SetActive(false);
        stars.Stop();
    }

    private Tween CreatePathForEnergyFlight(Vector2 energyStartPos, float dur) {
        var endPos = lbBtn.position - Vector3.up * 0.3f;
        var path = new[] {
            endPos,
            new(energyStartPos.x, energyStartPos.y - 1.8f),
            new(energyStartPos.x - 2, energyStartPos.y - 1)
        };

        var tweenPath = energyToFly.DOPath(path, dur, PathType.CubicBezier);
        return tweenPath;
    }
}