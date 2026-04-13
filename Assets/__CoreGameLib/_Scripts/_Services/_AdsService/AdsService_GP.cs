using System;
using __CoreGameLib._Scripts._ScriptableObjects;
using UnityEngine;
using GamePush;
using Zenject;

namespace core.ads {
    public class AdsService_GP : IAdsService, IInitializable {
        private bool _isAdShowing;
        private DateTime _startTime;

        private ProjectSettings _projectSettings;

        private AdsService_GP(ProjectSettings projectSettings) {
            _projectSettings = projectSettings;
        }

        public void Initialize() {
            _startTime = DateTime.Now;
            // gp initializes automatically via prefab
        }

        // --- interstitial ---

        public event Action OnAdStart;
        public event Action OnResumeToGameAfterAd;

        public void ShowInterstitial(Action onAdClosed) {
            if (_isAdShowing) return;

/*#if UNITY_EDITOR
            // gp_ads editor implementation doesn't invoke callbacks, simulating here
            onAdClosed?.Invoke();
            return;
#endif*/
            var timeFromStart = DateTime.Now.Subtract(_startTime);
            if (timeFromStart.TotalSeconds < _projectSettings.FirstInterstitialTime || !GP_Ads.IsFullscreenAvailable()) {
                onAdClosed?.Invoke();
                return;
            }

            _isAdShowing = true;

            // mapping to: ShowFullscreen(Action onStart, Action<bool> onClose)
            GP_Ads.ShowFullscreen(
                onFullscreenStart: () => { PauseGame(); },
                onFullscreenClose: (success) => {
                    _isAdShowing = false;
                    ResumeGame();
                    onAdClosed?.Invoke();
                }
            );
        }

        // --- rewarded ---

        public void ShowRewarded(Action onRewardGranted, Action onAdClosed, string placement = "") {
            if (_isAdShowing) return;

#if UNITY_EDITOR
            // gp_ads editor implementation doesn't invoke close callback, simulating here
            onRewardGranted?.Invoke();
            onAdClosed?.Invoke();
            return;
#endif

            if (!GP_Ads.IsRewardedAvailable()) {
                onAdClosed?.Invoke();
                return;
            }

            _isAdShowing = true;

            // mapping to: ShowRewarded(string tag, Action<string> onReward, Action onStart, Action<bool> onClose)
            GP_Ads.ShowRewarded(
                idOrTag: "REWARD",
                onRewardedReward: (tag) => { onRewardGranted?.Invoke(); },
                onRewardedStart: () => { PauseGame(); },
                onRewardedClose: (success) => {
                    _isAdShowing = false;
                    ResumeGame();
                    onAdClosed?.Invoke();
                }
            );
        }

        // --- helpers ---

        private void PauseGame() {
            OnAdStart?.Invoke();
            Time.timeScale = 0;
        }

        private void ResumeGame() {
            OnResumeToGameAfterAd?.Invoke();
            Time.timeScale = 1;
        }
    }
}