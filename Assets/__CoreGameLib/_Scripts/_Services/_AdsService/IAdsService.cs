using System;

namespace core.ads {
    public interface IAdsService {
        event Action OnAdStart;
        event Action OnResumeToGameAfterAd;
        void ShowInterstitial(Action onAdClosed);
        void ShowRewarded(Action onRewardGranted, Action onAdClosed, string placement = "");
    }
}