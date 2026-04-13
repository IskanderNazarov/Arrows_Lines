using System;
using __Gameplay;
using core.boosters; // Убедись, что пространство имен BoosterId доступно
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoosterButton : MonoBehaviour {
    [SerializeField] private BoosterId _boosterId; // Выбираем в инспекторе (Hint, Hammer, Ruler)
    [SerializeField] private Button _button;
    [SerializeField] private Image _icon; // Ссылка на иконку
    [SerializeField] private TextMeshProUGUI _counterText; // Текст с количеством
    [SerializeField] private GameObject _adOverlay; // (Опционально) Значок рекламы/плюсик, если бустеров 0

    public BoosterId BoosterId => _boosterId;

    // Инициализация из GameUI
    public void Initialize(Action<BoosterId, Transform> onClickCallback) {
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => {
            // Легкая анимация нажатия самой кнопки для отзывчивости
            transform.DOKill(true);
            transform.DOPunchScale(Vector3.one * -0.1f, 0.15f, 1);
            
            onClickCallback?.Invoke(_boosterId, transform);
        });
    }

    // Обновление визуала
    public void UpdateVisuals(int count) {
        if (_counterText != null) {
            _counterText.text = count > 0 ? count.ToString() : ""; // Если 0, можно скрыть число
        }

        if (_adOverlay != null) {
            _adOverlay.SetActive(count <= 0); // Показываем рекламу/плюсик только если бустеров нет
        }
    }
}