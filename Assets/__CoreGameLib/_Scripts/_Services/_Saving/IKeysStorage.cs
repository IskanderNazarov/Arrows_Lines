using System.Collections.Generic;

namespace _Services._Saving {
    public interface IKeysStorage {
        List<string> GetAllKeys();
        Dictionary<string, object> GetDefaultValues();
        T GetDefaultValue<T>(string key);

        void TryToAddDefaultValue(string key, object value);
    }
}