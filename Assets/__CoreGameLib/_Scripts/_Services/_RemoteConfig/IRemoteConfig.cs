using System.Collections;
using _Services._Saving;

namespace __CoreGameLib._Scripts._Services._RemoteConfig {
    public interface IRemoteConfig {
        IEnumerator LoadConfigs(IKeysStorage  keysStorage);
        string GetValue(string key);
    }
}