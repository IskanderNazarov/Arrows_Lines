using System.Collections;
using _ExtensionsHelpers;
using _game._LevelsProviding;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace _UI {
    public class FXCanvas : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI _congratsText;
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private RectTransform _congratsTMP_Tr;
        [SerializeField] private ParticleSystem _starsCollectParticles;

        private Color _textOriginColor;
        [Inject] private ILevelProvider _levelProvider;
        [Inject] private PlayerProgressService _progressService;
        [Inject] private SoundHelper _soundHelper;

        private void Start() {
            _textOriginColor = _congratsText.color;
        }

        public IEnumerator PlayStarsCollectParticles() {
            var dur = _starsCollectParticles.main.duration;
            _starsCollectParticles.gameObject.SetActive(true);
            _starsCollectParticles.Play();

            yield return new WaitForSeconds(dur);
            _starsCollectParticles.gameObject.SetActive(false);
        }

        public IEnumerator AnimateCongratsText(Vector2 centerPos) {
            _soundHelper.PlayCongratsText();
           // _congratsText.text = Localizer.Congrats[_progressService.CurrentLevelIndex % Localizer.Congrats.Length];
            var textAnimDur = 0.85f;
            _congratsText.gameObject.SetActive(true);
            _congratsText.color = _textOriginColor;
            _congratsText.transform.position = centerPos; // - Vector2.up * 0.5f;
            var tween = _congratsText.transform.DOScale(Vector3.one, textAnimDur).From(0).SetEase(Ease.OutBack, 2);

            yield return new WaitForSeconds(tween.Duration() * 0.3f);
            _particleSystem.gameObject.SetActive(true);
            _particleSystem.transform.position = new Vector2(0, centerPos.y + 0f);
            _particleSystem.Play();

            var fadeTween = _congratsText.DOFade(0, textAnimDur * 0.3f).SetDelay(1);
            //_congratsText.transform.DOMove(centerPos + Vector2.up * 5, 1).SetSpeedBased(true).SetEase(Ease.Linear);

            yield return fadeTween.WaitForCompletion();
            yield return new WaitForSeconds(0.4f);

            _congratsText.DOKill();
            _congratsText.transform.DOKill();
            _congratsText.gameObject.SetActive(false);

            //todo made thumb up anim
        }
    }
}