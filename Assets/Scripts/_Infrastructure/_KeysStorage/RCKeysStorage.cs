using System.Collections.Generic;
using _Services._Saving;
using Sripts._Services._KeysStorage;

namespace _Infrastructure {
    public class RCKeysStorage : IKeysStorage {
        private readonly Dictionary<string, object> _allDefaults;
        private List<string> _allKeys;

        public RCKeysStorage() {
            UnityEngine.Debug.Log("RCKeysStorage constructor");
            //--- Get default values from Core ---
            _allDefaults = new Dictionary<string, object>();

            //--- Override default values from core (if needed) ---
            //_allDefaults[CoreKeys.CoinsKey] = RCH.DCC;

            //--- Add game specific keys to the keys storage ---
            //AddToDefaults(RCCoreKeys.INTERSTITIAL_TIME, "80");
            AddToDefaults(RCGameKeys.TEST_KEY, "4.8"); //key -MUST BE- string
            AddToDefaults(RCGameKeys.IS_LINE_SMOKE, "0"); //key -MUST BE- string
        }

        private void AddToDefaults(string key, object value) {
            if (!_allDefaults.ContainsKey(key)) {
                _allDefaults.Add(key, value);
            }
        }

        public List<string> GetAllKeys() {
            return _allKeys ??= new List<string>(_allDefaults.Keys);
        }

        public Dictionary<string, object> GetDefaultValues() {
            return _allDefaults;
        }

        public void TryToAddDefaultValue(string key, object value) {
            if (_allDefaults.ContainsKey(key)) {
                _allDefaults[key] = value;
            } else {
                _allDefaults.Add(key, value);
                _allKeys = null; // Сбрасываем кэш ключей, чтобы пересоздался при следующем вызове
            }
        }

        public T GetDefaultValue<T>(string key) {
            if (_allDefaults.TryGetValue(key, out var value)) {
                return value is T tValue ? tValue : default;
            }

            return default;
        }
    }
}