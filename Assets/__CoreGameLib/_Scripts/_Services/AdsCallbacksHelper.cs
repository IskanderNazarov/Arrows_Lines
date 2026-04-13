using core.ads;
using UnityEngine;
using Zenject;

namespace Core._Services {
    public class AdsCallbacksHelper : MonoBehaviour {
        [Inject] private IAdsService _adsService;
        [Inject] private SoundManager _soundManager;

        private void Start() {
            _adsService.OnAdStart -= OnAdStart;
            _adsService.OnAdStart += OnAdStart;
            _adsService.OnResumeToGameAfterAd -= OnResumeToGameAfterAd;
            _adsService.OnResumeToGameAfterAd += OnResumeToGameAfterAd;
            //_adsService.OnGameShouldPause += OnGameShouldPause;
        }

        private void OnAdStart() {
            OnGameShouldPause(true);
        }

        private void OnResumeToGameAfterAd() {
            OnGameShouldPause(false);
        }

        private void OnGameShouldPause(bool pause) {
            _soundManager.MuteSounds(pause);
        }
    }
}