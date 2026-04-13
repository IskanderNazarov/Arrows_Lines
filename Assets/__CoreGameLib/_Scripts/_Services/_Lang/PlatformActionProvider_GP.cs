using __CoreGameLib._Scripts._Services._Lang;
using GamePush;

namespace __CoreGameLib._Scripts {
    public class PlatformActionProvider_GP : IPlatformActionProvider {
        public string GetISO() {
            return GP_Language.CurrentISO();
        }

        public void CallGameReadyAPI() {
            GP_Game.GameReady();
        }

        public void SetMinimalDelayForInterstitial() {
            //controlled from GP dashboard
        }

        public bool IsRemoteConfigSupported() {
            return true;
        }
    }
}