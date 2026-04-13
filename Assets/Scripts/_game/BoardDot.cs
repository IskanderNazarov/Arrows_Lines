using DG.Tweening;
using UnityEngine;

namespace _game {
    public class BoardDot : MonoBehaviour {
        [SerializeField] private Transform _transform;
        [SerializeField] private SpriteRenderer _sr;
        [SerializeField] private float _dotInitScale;
        [SerializeField] private Color _originColor;
        [SerializeField] private float _appearDuration;
        
        public void PLayAppearAnimFromUnderSnake(float delay, Color snakeColor) {
            var tr = _transform;
            
            tr.transform.localScale = Vector3.zero;
            tr.transform.DOScale(_dotInitScale, _appearDuration).SetDelay(delay).SetEase(Ease.OutBack);
            /*tr.transform.localScale = Vector3.one * _dotInitScale;
            tr.transform.DOPunchScale(Vector3.one * (-_dotInitScale * 0.8f), _appearDuration, 5).SetDelay(delay);*/
            
            _sr.color = snakeColor;
            //Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(snakeColor)}>snakeColor: {snakeColor}.</color>");
            _sr.DOColor(_originColor, _appearDuration).From(snakeColor).SetDelay(delay + _appearDuration * 0.4f);
        }

        public void ResetDot() {
            _transform.DOKill();
            _sr.DOKill();
            EnableTrails(false);
            
            _transform.localScale = Vector3.one *  _dotInitScale;
            _sr.color = _originColor;
        }

        public void EnableTrails(bool enable) {
            //todo enable diable trails
        }
    }
}