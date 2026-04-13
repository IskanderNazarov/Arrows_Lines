namespace __CoreGameLib._Scripts._Services._Lang {
    public interface IPlatformActionProvider {
        string GetISO();
        void CallGameReadyAPI();
        void SetMinimalDelayForInterstitial();
        bool IsRemoteConfigSupported();
    }
}