using UnityEngine;
using Zenject;

public class AspectRatioVisibilityController : MonoBehaviour {
    [Inject] private SignalBus _signalBus;

    [Header("Aspect Ratio Threshold")]
    [Tooltip("Например: Width = 1, Height = 1 (для 1:1)")]
    [SerializeField] private float _ratioWidth = 1f; 
    [SerializeField] private float _ratioHeight = 1f;

    [Header("Settings")]
    [Tooltip("Если TRUE: скроет элемент на узких экранах (где ratio < threshold).\nЕсли FALSE: скроет на широких экранах (где ratio > threshold).")]
    [SerializeField] private bool _hideIfLess = true;

    [Tooltip("Оставь пустым, чтобы скрывать сам объект, на котором висит этот скрипт.")]
    [SerializeField] private GameObject _targetToHide;

    private void Start() {
        if (_targetToHide == null) {
            _targetToHide = gameObject;
        }

        _signalBus.Subscribe<ScreenSizeChangedSignal>(OnScreenSizeChanged);

        // Применяем проверку сразу при старте сцены
        float initialAspect = (float)Screen.width / Screen.height;
        CheckVisibility(initialAspect);
    }

    private void OnDestroy() {
        _signalBus.Unsubscribe<ScreenSizeChangedSignal>(OnScreenSizeChanged);
    }

    private void OnScreenSizeChanged(ScreenSizeChangedSignal signal) {
        CheckVisibility(signal.AspectRatio);
    }

    private void CheckVisibility(float currentAspect) {
        if (_ratioHeight == 0) return; // Защита от деления на ноль

        float thresholdAspect = _ratioWidth / _ratioHeight;
        
        bool shouldHide = _hideIfLess 
            ? currentAspect < thresholdAspect 
            : currentAspect > thresholdAspect;

        _targetToHide.SetActive(!shouldHide);
    }
}