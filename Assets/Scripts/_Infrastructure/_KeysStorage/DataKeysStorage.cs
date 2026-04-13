using System.Collections.Generic;
using __CoreGameLib._Scripts._Services._Saving;
using _Infrastructure;
using UnityEngine;

namespace _Services._Saving {
    public class DataKeysStorage : IKeysStorage {
        private readonly Dictionary<string, object> _allDefaults;
        private List<string> _allKeys;

        public DataKeysStorage() {
            //_allDefaults = new Dictionary<string, object>();
            //--- Get default values from Core ---
            _allDefaults = new Dictionary<string, object>(CoreKeys.DefaultValues);

            //--- Override default values from core (if needed) ---
            //_allDefaults[CoreKeys.CoinsKey] = RCH.DCC;
            // _allDefaults[CoreKeys.GemsKey] = RCH.DGC;

            //--- Add game specific keys to the keys storage ---
            AddToDefaults(GameKeys.TEST_KEY, "[]");
            AddToDefaults(GameKeys.KEY_LEVEL_INDEX, 0);
            AddToDefaults(GameKeys.PlayerData, "");
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

        /*public T GetDefaultValue<T>(string key) {
            if (KeysToDefaultValuesMap.ContainsKey(key) == false) {
                return default;
            }

            var value = KeysToDefaultValuesMap[key];

            var t = typeof(T);
            return value is T ? (T)value : default;
        }*/

        public T GetDefaultValue<T>(string key) {
            if (_allDefaults.TryGetValue(key, out var value)) {
                return value is T tValue ? tValue : default;
            }

            return default;
        }
    }
}