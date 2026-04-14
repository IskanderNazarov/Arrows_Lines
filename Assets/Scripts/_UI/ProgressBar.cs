using System;
using System.Collections;
using _Data;
using _Services;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Gameplay._UI {
    public class ProgressBar : MonoBehaviour {
        [Header("UI References")] [SerializeField]
        private Image _fillImage;

        [SerializeField] private RectTransform _barContainer; // Для эффекта баунса (п. 8)
        [SerializeField] private RectTransform _rocketRect; // Сама ракета
        [SerializeField] private Image _previousRewardIcon; // Иконка планеты слева (п. 9, 11)

        [Header("Rocket Movement Settings")] [SerializeField]
        private RectTransform _startPoint; // Левый край бара

        [SerializeField] private RectTransform _endPoint; // Правый край бара


        [Header("Visual Settings")] [SerializeField, Range(0f, 1f)]
        private float _minVisualFill = 0.1f; // п. 6 (10% минимум)

        [SerializeField] private float _moveAwayDuration = 1.5f;


        /// <summary>
        /// Метод для мгновенной синхронизации (используется при загрузке MainScreen)
        /// </summary>
        public void SyncVisuals(int currentStars, int targetStars, PlanetRewardData previousReward) {
            Debug.Log($"currentStars: {currentStars}");
            Debug.Log($"targetStars: {targetStars}");
            var logicalFill = targetStars > 0 ? Mathf.Clamp01((float)currentStars / targetStars) : 0f;
            SetVisualFill(logicalFill);
            UpdatePreviousRewardIcon(previousReward);
        }

        /*public void AnimateProgress(ProgressUpdateResult result, Action<bool> onComplete) {
            StartCoroutine(AnimateProgressRoutine(result, onComplete));
        }*/

        public IEnumerator AnimateProgressRoutine(ProgressUpdateResult result, Action<Action> onUnlockReached) {
            // 1. Ставим начальный визуал
            var startFill = result.TargetStarsBefore > 0 ? (float)result.StartStars / result.TargetStarsBefore : 0f;
            SetVisualFill(startFill);

            if (result.DidUnlock) {
                // Анимируем до 100%
                yield return StartCoroutine(TweenFillRoutine(startFill, 1f, 1.5f));

                // Ракета улетает вправо за экран
                yield return StartCoroutine(FlyRocketOffScreenRoutine());

                // --- ПРЕРЫВАНИЕ АНИМАЦИИ ---
                bool isUnlockScreenClosed = false;
                
                // Передаем наверх колбек, который контроллер должен вызвать при закрытии окна
                onUnlockReached?.Invoke(() => isUnlockScreenClosed = true);
                
                // Ждем, пока флаг не станет true
                yield return new WaitUntil(() => isUnlockScreenClosed);
                // ---------------------------

                // Только ТЕПЕРЬ показываем новую планету в прогресс баре
                UpdatePreviousRewardIcon(result.UnlockedReward); 

                // Возвращаем ракету слева
                SetVisualFill(0f);
                yield return StartCoroutine(FlyRocketInScreenRoutine());

                // Дозаполняем остаток (EndStars) к новой цели (TargetStarsAfter)
                var finalFill = result.TargetStarsAfter > 0 ? (float)result.EndStars / result.TargetStarsAfter : 0f;
                yield return StartCoroutine(TweenFillRoutine(0f, finalFill, 0.65f));
            } else {
                // Обычное заполнение без переполнения
                var endFill = result.TargetStarsBefore > 0 ? (float)result.EndStars / result.TargetStarsBefore : 0f;
                yield return StartCoroutine(TweenFillRoutine(startFill, endFill, 1.5f));
            }
        }

        // --- Вспомогательные методы анимации и логики ---

        private void SetVisualFill(float logicalFill) {
            // п. 6: Пустой прогресс бар выглядит заполненным на 10%
            var visualFill = Mathf.Lerp(_minVisualFill, 1f, logicalFill);

            if (_fillImage != null) _fillImage.fillAmount = visualFill;

            if (_rocketRect != null && _startPoint != null && _endPoint != null) {
                // Двигаем ракету между стартовой и конечной точкой в зависимости от визуала
                _rocketRect.position = Vector3.Lerp(_startPoint.position, _endPoint.position, visualFill);
            }
        }

        private IEnumerator TweenFillRoutine(float startFill, float endFill, float duration) {
            _rocketRect.DOKill();
            _fillImage.DOKill();

            // DOVirtual.Float плавно интерполирует число от startFill до endFill
            // и каждый кадр вызывает лямбду (v => SetVisualFill(v))
            Tween fillTween = DOVirtual.Float(startFill, endFill, duration, v => { SetVisualFill(v); }).SetEase(Ease.Linear);

            // Ждем завершения анимации
            yield return fillTween.WaitForCompletion();

            //yield return new WaitForSeconds(0.5f); // Твоя пауза, если она нужна

            // На всякий случай гарантируем точное конечное значение
            SetVisualFill(endFill);
        }

        private IEnumerator FlyRocketOffScreenRoutine() {
            if (_rocketRect == null) yield break;

            // Ракета летит точно за правый край экрана
            var targetX = GetOffScreenRightX();
            Tween flyTween = _rocketRect.DOAnchorPosX(targetX, _moveAwayDuration).SetEase(Ease.InBack, 1.1f);

            yield return flyTween.WaitForCompletion();
        }

        //-----------------------------------------------------------------
        private IEnumerator FlyRocketInScreenRoutine() {
            if (_rocketRect == null) yield break;

            // Ставим ракету точно за левый край экрана
            var startX = GetOffScreenLeftX();
            _rocketRect.anchoredPosition = new Vector2(startX, _rocketRect.anchoredPosition.y);

            // Летим на позицию 0% (или точнее 10% визуальных)
            //SetVisualFill(0f);

            var properStartPos = Vector3.Lerp(_startPoint.position, _endPoint.position, _minVisualFill);
            Tween flyTween = _rocketRect.DOMoveX(properStartPos.x, _moveAwayDuration).SetEase(Ease.OutBack, 1.1f);

            yield return flyTween.WaitForCompletion();
        }

        private float UpdatePreviousRewardIcon(PlanetRewardData reward) {
            if (reward.Sprite != null) {
                const float dur = 0.45f;
                _previousRewardIcon.gameObject.SetActive(true);
                _previousRewardIcon.sprite = reward.Sprite;
                _previousRewardIcon.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), dur);
                return dur;
            } else {
                _previousRewardIcon.gameObject.SetActive(false);
            }

            return 0;
        }

        private float GetOffScreenRightX() {
            var canvas = GetComponentInParent<Canvas>();
            var parentRect = _rocketRect.parent as RectTransform;
            var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            // Находим точку правого края экрана в координатах родителя ракеты
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                new Vector2(Screen.width, Screen.height / 2f),
                cam,
                out var rightEdge);

            // Возвращаем координату края + ширину ракеты (чтобы она улетела целиком)
            return rightEdge.x + _rocketRect.rect.width;
        }

        private float GetOffScreenLeftX() {
            var canvas = GetComponentInParent<Canvas>();
            var parentRect = _rocketRect.parent as RectTransform;
            var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            // Находим точку левого края экрана (X = 0)
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                new Vector2(0, Screen.height / 2f),
                cam,
                out var leftEdge);

            // Возвращаем координату левого края - ширину ракеты
            return leftEdge.x - _rocketRect.rect.width;
        }
    }
}