using System;
using System.Collections;
using _ExtensionsHelpers;
using _UI;
using _Zenject;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LoseDialog : Dialog {
    [Inject] private SignalBus _signalBus;

    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _watchAdButton;
    [SerializeField] private RectTransform _heartTr;
    [SerializeField] private TextMeshProUGUI _reviveCountTMP;
    [SerializeField] private TextMeshProUGUI _restartTMP;
    [SerializeField] private TextMeshProUGUI _continueTMP;
    [SerializeField] private float _angle;
    [SerializeField] private float _heartDur;
    [SerializeField] private float _heartDelay;
    private int _reviveCount;

    [Inject] private GameConfig _gameConfig;
    [Inject] private SoundHelper _soundHelper;

    public void Show(int reviveCount, Action onShowFinished = null) {
        _reviveCount = reviveCount;
        Show(onShowFinished);
    }

    public override void Show(Action onShowFinished = null) {
        _restartButton.onClick.AddListener(OnRestartClicked);
        _watchAdButton.onClick.AddListener(OnWatchAdClicked);
        _watchAdButton.gameObject.SetActive(_reviveCount > 0);
        /*_restartTMP.text = Localizer.Restart;
        _continueTMP.text = Localizer.Continue;*/
            
        _heartTr.DOKill();
        _heartTr.localEulerAngles = new Vector3(0, 0, 0);
        _reviveCountTMP.text = $"{_reviveCount}/{_gameConfig.ReviveCountCap}";
        _heartTr.gameObject.SetActive(_reviveCount > 0);
        
        base.Show(() => {
            if(_reviveCount > 0) StartCoroutine(HeartRoutine());
        });
    }

    private IEnumerator HeartRoutine() {
        _heartTr.DOKill();
        var delay = new WaitForSeconds(_heartDelay);
        yield return new WaitForSeconds(0.5f);
        while (true) {
            _heartTr.DOKill();
            _heartTr.localEulerAngles = new Vector3(0, 0, 0);
            yield return _heartTr.DOLocalRotate(Vector3.forward * _angle, _heartDur).From(Vector3.zero).SetEase(Ease.InSine)
                .SetLoops(4, LoopType.Yoyo).WaitForCompletion();
            yield return delay;
        }
    }

    public override void Hide(Action onCompleted) {
        StopAllCoroutines();
        _heartTr.DOKill();

        _restartButton.onClick.RemoveListener(OnRestartClicked);
        _watchAdButton.onClick.RemoveListener(OnWatchAdClicked);
        base.Hide(onCompleted);
    }

    private void OnRestartClicked() {
        Hide(null);
        _signalBus.Fire<RestartLevelClickedSignal>();
    }

    // ИСПРАВЛЕННЫЙ МЕТОД: Никакого вызова рекламы здесь!
    private void OnWatchAdClicked() {
        // Мы просто отправляем намерение (Intent) игрока посмотреть рекламу
        _signalBus.Fire<ProcessAdForReviveSignal>();
        
        // Диалог можно пока скрыть, или можно повесить на него лоадер "Загрузка рекламы..."
        Hide(null);
    }
}