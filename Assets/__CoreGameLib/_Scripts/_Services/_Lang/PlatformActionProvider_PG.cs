using __CoreGameLib._Scripts._ScriptableObjects;
using __CoreGameLib._Scripts._Services._Lang;
using Playgama;
using Playgama.Modules.Platform;
using Zenject;

namespace __CoreGameLib._Scripts {
    public class PlatformActionProvider_PG : IPlatformActionProvider {
        [Inject] private ProjectSettings _projectSettings;

        public string GetISO() {
            return Bridge.platform.language;
        }

        public void CallGameReadyAPI() {
            Bridge.platform.SendMessage(PlatformMessage.GameReady);
        }

        public void SetMinimalDelayForInterstitial() {
            Bridge.advertisement.SetMinimumDelayBetweenInterstitial(_projectSettings.minimumDelayBetweenInterstitial);
        }

        public bool IsRemoteConfigSupported() {
            return Bridge.remoteConfig.isSupported;
        }
    }
}