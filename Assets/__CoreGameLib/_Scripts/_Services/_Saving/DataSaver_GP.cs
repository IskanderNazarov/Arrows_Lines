using System;
using System.Collections;
using System.Collections.Generic;
using _Infrastructure; // Для DataParserTool
using _Services._Saving;
using Core._Services._Saving;
using GamePush;
using UnityEngine;
using UnityEngine.Events;

namespace __CoreGameLib._Scripts._Services._Saving {
    public class DataSaver_GP : IDataSaver {
        
        private IKeysStorage _keysStorage;
        private DataParserTool _dataParser;
        
        // Локальный кэш
        private Dictionary<string, object> _savedData = new Dictionary<string, object>();
        
        // Сет ключей, которые должны дублироваться в публичные поля игрока (Score, Level и т.д.)
        private readonly HashSet<string> _publicFields;

        // Флаги состояния
        private bool _isLoaded = false;
        private bool _isSaving = false;
        private bool _isDirty = false;

        // Имя поля-контейнера в GamePush
        private const string CONTAINER_FIELD_KEY = "game_save_data";

        // --- КОНСТРУКТОР ---
        // Принимает список публичных полей. Core не знает, что это за поля, это решает Game (Installer).
        public DataSaver_GP(string[] publicFields) {
            _publicFields = new HashSet<string>(publicFields);
            _publicFields.Add(CONTAINER_FIELD_KEY);
        }

        // --- ЗАГРУЗКА ---

        public IEnumerator LoadData(IKeysStorage keysStorage) {
            _keysStorage = keysStorage;
            _savedData.Clear();
            _isLoaded = false;

            // 1. Запрашиваем загрузку (полагаем, что SDK уже инициализирован)
            bool loadFinished = false;
            
            UnityAction onLoadComplete = () => { loadFinished = true; };
            UnityAction onLoadError = () => { 
                loadFinished = true; 
                Debug.LogWarning("DS__ DataSaver_GP: Player Load Error. Using defaults/local cache."); 
            };

            GP_Player.OnLoadComplete += onLoadComplete;
            GP_Player.OnLoadError += onLoadError;

            GP_Player.Load();

            // Ждем завершения
            float timeout = 5f;
#if UNITY_EDITOR
            timeout = 0.1f;
#endif
            
            while (!loadFinished && timeout > 0) {
                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }

            GP_Player.OnLoadComplete -= onLoadComplete;
            GP_Player.OnLoadError -= onLoadError;

            // 2. Распаковка данных
            
            // А. Дефолтные значения
            var defaults = _keysStorage.GetDefaultValues();
            foreach (var kv in defaults) {
                _savedData[kv.Key] = kv.Value;
            }

            // Б. JSON Контейнер (основные данные)
            if (GP_Player.Has(CONTAINER_FIELD_KEY)) {
                Debug.Log($"DS__ CONTAINER_FIELD_KEY found: {CONTAINER_FIELD_KEY}");
                string jsonContainer = GP_Player.GetString(CONTAINER_FIELD_KEY);
                if (!string.IsNullOrEmpty(jsonContainer)) {
                    try {
                        var wrapper = JsonUtility.FromJson<JsonContainer>(jsonContainer);
                        if (wrapper != null && wrapper.items != null) {
                            foreach (var item in wrapper.items) {
                                /*Debug.Log($"DS__ _savedData[{item.k}]: {_savedData[item.k]}");
                                Debug.Log($"DS__ item.k: {item.k}");
                                Debug.Log($"DS__ item.v: {item.v}");*/
                                _savedData[item.k] = ParseValue(item.v, item.t);
                            }
                        }
                    } catch (Exception e) {
                        Debug.LogError($"DS__DataSaver_GP: JSON Parse error: {e.Message}");
                    }
                }
            }

            // В. Публичные поля (накатываем поверх, так как они приоритетнее)
            foreach (var key in _publicFields) {
                if (GP_Player.Has(key)) {
                    object defaultVal = _keysStorage.GetDefaultValue<object>(key);
                    
                    // Определяем тип по дефолтному значению, чтобы корректно достать из GP
                    if (defaultVal is int) _savedData[key] = GP_Player.GetInt(key);
                    else if (defaultVal is float) _savedData[key] = GP_Player.GetFloat(key);
                    else if (defaultVal is bool) _savedData[key] = GP_Player.GetBool(key);
                    else _savedData[key] = GP_Player.GetString(key);
                }
            }

            _dataParser = new DataParserTool(_savedData, _keysStorage);
            _isLoaded = true;
            Debug.Log("DS__ DataSaver_GP: Loaded successfully.");
        }

        // --- СОХРАНЕНИЕ ---

        public void SetData(string key, string value) {
            UpdateLocalAndPublic(key, value);
            TrySync();
        }

        public void SetData(Dictionary<string, object> map) {
            // ОПТИМИЗАЦИЯ: Сначала обновляем все данные в памяти и полях GP
            foreach (var item in map) {
                UpdateLocalAndPublic(item.Key, item.Value);
            }
            
            // И только ПОТОМ вызываем синхронизацию один раз
            TrySync();
        }

        // Внутренний метод обновления кэша и публичных полей (без Sync)
        private void UpdateLocalAndPublic(string key, object value) {
            // 1. Локальный кэш
            if (_savedData.ContainsKey(key)) _savedData[key] = value;
            else _savedData.Add(key, value);

            // 2. Если поле публичное - обновляем модель GP сразу
            if (_publicFields.Contains(key)) {
                if (value is int iVal) GP_Player.Set(key, iVal);
                else if (value is float fVal) GP_Player.Set(key, fVal);
                else if (value is bool bVal) GP_Player.Set(key, bVal);
                else GP_Player.Set(key, value.ToString());
            }

            _isDirty = true;
        }

        // Запуск процесса сохранения
        private void TrySync() {
            if (!_isSaving) {
                Sync();
            }
        }

        private void Sync() {
            if (!_isDirty) {
                _isSaving = false;
                return;
            }

            _isSaving = true;
            _isDirty = false;

            // --- Упаковка в JSON ---
            // Используем внутренние DTO классы
            JsonContainer container = new JsonContainer();
            foreach (var kv in _savedData) {
                if (kv.Key == CONTAINER_FIELD_KEY) continue;

                string typeStr = "string";
                if (kv.Value is int) typeStr = "int";
                else if (kv.Value is float) typeStr = "float";
                else if (kv.Value is bool) typeStr = "bool";

                container.items.Add(new JsonItem {
                    k = kv.Key,
                    v = kv.Value.ToString(),
                    t = typeStr
                });
            }

            string jsonString = JsonUtility.ToJson(container);
            //Debug.Log($"DS__ jsonString: {jsonString},");
            GP_Player.Set(CONTAINER_FIELD_KEY, jsonString);

            // --- Отправка ---
            UnityAction onSync = null;
            UnityAction onError = null;

            onSync = () => {
                GP_Player.OnSyncComplete -= onSync;
                GP_Player.OnSyncError -= onError;
                
                // Если данные изменились пока мы сохранялись — повторяем
                if (_isDirty) {
                    Sync();
                } else {
                    _isSaving = false;
                }
            };

            onError = () => {
                GP_Player.OnSyncComplete -= onSync;
                GP_Player.OnSyncError -= onError;
                
                Debug.LogWarning("DataSaver_GP: Sync Failed.");
                _isSaving = false;
            };

            GP_Player.OnSyncComplete += onSync;
            GP_Player.OnSyncError += onError;

            GP_Player.Sync();
        }

        // --- ВНУТРЕННИЕ DTO (Infrastructure only) ---
        // Эти классы приватные. Они не видны ни Core.asmdef (снаружи класса), ни Game.asmdef.
        // Они нужны только для того, чтобы этот конкретный класс мог сохранить словарь в JSON.
        
        [Serializable]
        private class JsonContainer {
            public List<JsonItem> items = new List<JsonItem>();
        }

        [Serializable]
        private class JsonItem {
            public string k; // Key
            public string v; // Value
            public string t; // Type
        }

        private object ParseValue(string value, string type) {
            if (type == "int" && int.TryParse(value, out int i)) return i;
            if (type == "float" && float.TryParse(value, out float f)) return f;
            if (type == "bool" && bool.TryParse(value, out bool b)) return b;
            return value;
        }

        // --- Boilerplate геттеры ---
        public float GetDataFloat(string key) => _dataParser != null ? _dataParser.GetDataFloat(key) : 0f;
        public int GetDataInt(string key) {
            //if (key == "max_level_reached") return 34;
            return _dataParser != null ? _dataParser.GetDataInt(key) : 0;
        }
        
        public long GetDataLong(string key) {
            return _dataParser != null ? _dataParser.GetDataLong(key) : 0;
        }

        public string GetDataString(string key) => _dataParser != null ? _dataParser.GetDataString(key) : null;
        public bool GetDataBool(string key) => _dataParser != null && _dataParser.GetDataBool(key);

        public void DeleteAll() {
            GP_Player.ResetPlayer();
            _savedData.Clear();
            var defaults = _keysStorage.GetDefaultValues();
            SetData(defaults); 
        }

        public void Delete(string key) {
            var defVal = _keysStorage.GetDefaultValue<object>(key);
            SetData(key, defVal.ToString());
        }

        public IKeysStorage GetKeysStorage() => _keysStorage;

        public void RegisterSaveKey(Dictionary<string, object> defaultValues) {
            foreach (var (key, value) in defaultValues) _keysStorage?.TryToAddDefaultValue(key, value);
        }

        public void RegisterSaveKey(string key, object defaultValue) {
             _keysStorage?.TryToAddDefaultValue(key, defaultValue);
        }
    }
}