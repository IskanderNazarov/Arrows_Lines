using System.Collections;
using System.Collections.Generic;
using __CoreGameLib._Scripts._Services._Lang;
using __CoreGameLib._Scripts._Services._RemoteConfig;
using _Services._Saving;
using Playgama;
using UnityEngine;
using Zenject;

namespace _Infrastructure {
    public class RemoteConfig_PG : IRemoteConfig {
        private bool _isLoaded = true;
        private IKeysStorage _keysStorage;
        private DataParserTool _dataParserTool;


        public IEnumerator LoadConfigs(IKeysStorage  keysStorage) {
            _keysStorage = keysStorage;
            Debug.Log($"111 _keysStorage == null: {_keysStorage == null}");
            _dataParserTool = new DataParserTool(_keysStorage.GetDefaultValues(), _keysStorage);
            Debug.Log($"222 _keysStorage == null: {_keysStorage == null}");
            
            if (Bridge.remoteConfig.isSupported) {
                //if(Bridge.platform.id == "yandex")
                Debug.Log("RC_ LoadConfigs 1");
                _isLoaded = false;

                var defValues = _keysStorage.GetDefaultValues();
                Debug.Log($"defValues == null: {defValues == null}");
                foreach (var kv in defValues) {
                    Debug.Log($"k: {kv.Key}, v: {kv.Value}");
                }

                var clientFeatures = new object[] { _keysStorage.GetDefaultValues() };

                var options = new Dictionary<string, object>(); //code from Playgama docs
                //options.Add("clientFeatures", clientFeatures);
                /*var optionsJSON = JsonUtility.ToJson(options);
                Debug.Log($"optionsJSON: {optionsJSON}");*/

                /*clientFeatures = new object[] {
                    _keysStorage.GetDefaultValues()
                };*/

                options.Add("clientFeatures", clientFeatures);

                Bridge.remoteConfig.Get(options, OnLoadComplete);
                //Debug.Log("RC_ LoadConfigs 2");
                yield return new WaitUntil(() => _isLoaded);
                //Debug.Log("RC_ LoadConfigs 3");
            }
            else {
                yield return null;
            }
        }

        public string GetValue(string key) {
            return _dataParserTool.GetDataString(key);
        }

        private void OnLoadComplete(bool success, Dictionary<string, string> map) {
            Debug.Log("RC__ success = " + success);
            _isLoaded = true;
            if (!success) return;

            var defMap = _keysStorage.GetDefaultValues();
            var data = new Dictionary<string, object>();
            foreach (var kv in defMap) {
                Debug.Log($"RC LoadCompleted__ key: {kv.Key},  value: {kv.Value}");
                if (map.ContainsKey(kv.Key)) {
                    data[kv.Key] = map[kv.Key];
                }
                else {
                    data[kv.Key] = defMap[kv.Key];
                }
            }

            _dataParserTool = new DataParserTool(data, _keysStorage);
        }
    }
}