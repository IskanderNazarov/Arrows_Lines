using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _UI {
    public class Dialog : MonoBehaviour {
        [SerializeField] private Transform _panel;
        [SerializeField] private Image _darkBg;
        [SerializeField] private float _showHideDur = 0.55f;

        private GraphicRaycaster _raycaster;
        private Color _darkBgOriginColor;
        private bool _isInited;

        private void Init() {
            var parsed = ColorUtility.TryParseHtmlString("#00001DB2", out _darkBgOriginColor);
            Debug.Log($"parsed: {parsed}");
            if (_darkBg != null) _darkBgOriginColor = _darkBg.color;
            _raycaster = GetComponent<GraphicRaycaster>();
        }


        public virtual void Show() {
            Show(null);
        }

        public virtual void Show(Action onShowFinished = null) {
            if (!_isInited) {
                _isInited = true;
                Init();
            }

            gameObject.SetActive(true);
            StartCoroutine(PlayShowAnim(onShowFinished));
        }

        public virtual void Hide() {
            Hide(null);
        }

        public virtual void Hide(Action onCompleted) {
            StartCoroutine(HideAnim(onCompleted));
        }

        protected virtual IEnumerator PlayShowAnim(Action onShowFinished = null) {
            StopAnim();
            EnableInteraction(false);

            if (_panel != null) {
                _panel.localScale = Vector3.one;
                _panel.DOPunchScale(Vector3.one * 0.1f, _showHideDur, 2);
            }

            if (_darkBg != null) {
                _darkBg.DOColor(_darkBgOriginColor, 0.5f).From(new Color(0, 0, 0, 0));
            }

            yield return new WaitForSeconds(_showHideDur);
            EnableInteraction(true);
            onShowFinished?.Invoke();
        }

        protected virtual IEnumerator HideAnim(Action onCompleted) {
            StopAnim();
            EnableInteraction(false);

            if (_panel != null) {
                _panel.DOScale(Vector3.zero, _showHideDur).SetEase(Ease.InBack);
            }

            if (_darkBg != null) {
                _darkBg.DOColor(_darkBgOriginColor, 0.5f).From(new Color(0, 0, 0, 0));
            }

            yield return new WaitForSeconds(_showHideDur);
            gameObject.SetActive(false);
            onCompleted?.Invoke();
        }

        private void StopAnim() {
            if (_panel != null) _panel.DOKill();
            if (_darkBg != null) _darkBg.DOKill();
        }

        public void EnableInteraction(bool enable) {
            _raycaster.enabled = enable;
        }
    }
}