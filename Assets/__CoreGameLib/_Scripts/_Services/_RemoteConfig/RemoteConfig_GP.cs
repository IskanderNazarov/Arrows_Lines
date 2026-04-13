using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePush;
using _Infrastructure; // Для RCKeysStorage
using _Services._Saving; // Для IKeysStorage

namespace __CoreGameLib._Scripts._Services._RemoteConfig {
    public class RemoteConfig_GP : IRemoteConfig {
        
        // Локальный кэш конфигурации (Ключ -> Значение)
        private Dictionary<string, string> _configCache;
        
        // Флаг завершения сетевого запроса
        private bool _isFetchCompleted;
        
        // Флаг использования переменных платформы
        private readonly bool _usePlatformVariables;
        private IKeysStorage _keysStorage;

        // --- Конструктор ---
        public RemoteConfig_GP(bool usePlatformVariables) {
            _usePlatformVariables = usePlatformVariables;
        }

        public IEnumerator LoadConfigs(IKeysStorage keysStorage) {
            _keysStorage = keysStorage;
            // 1. Инициализируем кэш дефолтными значениями
            InitializeDefaults();

            _isFetchCompleted = false;

            // 2. Выбираем, откуда тянуть переменные
            if (_usePlatformVariables && GP_Variables.IsPlatformVariablesAvailable()) {
                Debug.Log("RemoteConfig_GP: Fetching variables from Platform...");
                // Запрашиваем переменные конкретной площадки
                GP_Variables.FetchPlatformVariables(OnPlatformFetchSuccess, OnPlatformFetchError);
            } else {
                Debug.Log("RemoteConfig_GP: Fetching variables from GamePush...");
                // Запрашиваем переменные GamePush (по умолчанию)
                GP_Variables.Fetch(OnFetchSuccess, OnFetchError);
            }

            // 3. Ждем ответа или тайм-аута
            float timeout = 5.0f; 
#if UNITY_EDITOR
            timeout = 0.1f;
#endif
            while (!_isFetchCompleted && timeout > 0) {
                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }

            if (timeout <= 0) {
                Debug.LogWarning("RemoteConfig_GP: Fetch timed out. Using default values.");
            }
        }

        public string GetValue(string key) {
            // Если кэш есть и ключ найден — возвращаем значение
            if (_configCache != null && _configCache.TryGetValue(key, out var value)) {
                return value;
            }

            // Fallback: если что-то пошло не так, пробуем достать дефолтное значение напрямую
            Debug.LogWarning($"RemoteConfig_GP: Key '{key}' not found in cache. Trying defaults.");
            return _keysStorage.GetDefaultValue<object>(key)?.ToString();
        }

        // --- Внутренние методы ---

        private void InitializeDefaults() {
            _configCache = new Dictionary<string, string>();
            
            foreach (var kv in _keysStorage.GetDefaultValues()) {
                // Конвертируем все в строки, так как GamePush и IRemoteConfig работают со строками
                _configCache[kv.Key] = kv.Value.ToString();
            }
        }

        // --- Колбэки для GamePush переменных ---

        private void OnFetchSuccess(List<VariablesData> variables) {
            Debug.Log($"RemoteConfig_GP: GP Fetch success. Received {variables.Count} variables.");
            
            foreach (var variable in variables) {
                if (_configCache.ContainsKey(variable.key)) {
                    _configCache[variable.key] = variable.value;
                } else {
                    _configCache.Add(variable.key, variable.value);
                }
            }
            
            _isFetchCompleted = true;
        }

        private void OnFetchError() {
            Debug.LogWarning("RemoteConfig_GP: GP Fetch failed. Keeping default values.");
            _isFetchCompleted = true;
        }

        // --- Колбэки для Платформенных переменных ---

        private void OnPlatformFetchSuccess(Dictionary<string, string> variables) {
            Debug.Log($"RemoteConfig_GP: Platform Fetch success. Received {variables.Count} variables.");
            
            foreach (var kvp in variables) {
                if (_configCache.ContainsKey(kvp.Key)) {
                    _configCache[kvp.Key] = kvp.Value;
                } else {
                    _configCache.Add(kvp.Key, kvp.Value);
                }
            }
            
            _isFetchCompleted = true;
        }

        private void OnPlatformFetchError(string error) {
            Debug.LogWarning($"RemoteConfig_GP: Platform Fetch failed. Error: {error}. Keeping default values.");
            _isFetchCompleted = true;
        }
    }
}