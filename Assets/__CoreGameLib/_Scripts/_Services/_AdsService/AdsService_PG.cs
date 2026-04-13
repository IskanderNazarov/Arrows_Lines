// file: assets/_coregame/_scripts/ads/adservice.cs
// assembly: corelib.asmdef

using System;
using Playgama;
using Playgama.Modules.Advertisement;
using UnityEngine;
using Zenject;
// needed for iinitializable

namespace core.ads {
    // implementing iinitializable to subscribe to sdk events on start
    public class AdsService_PG : IInitializable, IAdsService {

        // [inject] private core.audio.AudioService _audioService;

        private bool _isAdShowing = false;

        // stored callbacks from the game layer
        private Action _onInterstitialClosed;
        private Action _onRewardGranted;
        private Action _onRewardedAdClosed;

        // internal flag to track if reward was given
        private bool _rewardGrantedThisSession = false;

        /// <summary>
        /// subscribes to sdk events on game start
        /// </summary>
        public void Initialize() {
            Bridge.advertisement.interstitialStateChanged += OnInterstitialStateChanged;
            Bridge.advertisement.rewardedStateChanged += OnRewardedStateChanged;
        }

        // --- interstitial ---

        public event Action OnAdStart;
        public event Action OnResumeToGameAfterAd;

        /// <summary>
        /// shows an interstitial ad.
        /// </summary>
        /// <param name="onAdClosed">callback when ad is closed or fails.</param>
        public void ShowInterstitial(Action onAdClosed) {
            if (_isAdShowing) {
                onAdClosed?.Invoke(); // fail fast
                return;
            }

            _onInterstitialClosed = onAdClosed;
            Bridge.advertisement.ShowInterstitial();
        }

        /// <summary>
        /// internal handler for sdk interstitial states
        /// </summary>
        private void OnInterstitialStateChanged(InterstitialState state) {
            switch (state) {
            case InterstitialState.Opened:
                _isAdShowing = true;
                PauseGame();
                break;
            case InterstitialState.Closed:
            case InterstitialState.Failed:
                if (!_isAdShowing) return; // prevent multiple calls

                ResumeGame();
                _onInterstitialClosed?.Invoke();

                // clear for next ad
                _onInterstitialClosed = null;
                _isAdShowing = false;
                break;
            }
        }

        // --- rewarded ---

        /// <summary>
        /// shows a rewarded ad.
        /// </summary>
        /// <param name="onRewardGranted">callback if reward was granted.</param>
        /// <param name="onAdClosed">callback when ad is closed (always, even if no reward).</param>
        public void ShowRewarded(Action onRewardGranted, Action onAdClosed, string placement = "") {
            if (_isAdShowing) {
                onAdClosed?.Invoke(); // fail fast
                return;
            }

            _rewardGrantedThisSession = false; // reset flag
            _onRewardGranted = onRewardGranted;
            _onRewardedAdClosed = onAdClosed;

            Bridge.advertisement.ShowRewarded();
        }

        /// <summary>
        /// internal handler for sdk rewarded states
        /// </summary>
        private void OnRewardedStateChanged(RewardedState state) {
            switch (state) {
            case RewardedState.Opened:
                _isAdShowing = true;
                PauseGame();
                break;
            case RewardedState.Rewarded:
                // sdk confirms reward
                _rewardGrantedThisSession = true;
                _onRewardGranted?.Invoke(); // call reward callback now
                break;
            case RewardedState.Closed:
            case RewardedState.Failed:
                if (!_isAdShowing) return; // prevent multiple calls

                ResumeGame();
                _onRewardedAdClosed?.Invoke(); // always call 'closed'

                // clear for next ad
                _onRewardGranted = null;
                _onRewardedAdClosed = null;
                _isAdShowing = false;
                _rewardGrantedThisSession = false;
                break;
            }
        }

        // --- helpers (game-agnostic) ---

        private void PauseGame() {
            OnAdStart?.Invoke();
            Time.timeScale = 0;
            // _audioService.MuteAll();
        }

        private void ResumeGame() {
            OnResumeToGameAfterAd?.Invoke();
            Time.timeScale = 1;
            // _audioService.UnmuteAll();
        }
    }
}
