using System;
using System.Collections;
using System.Collections.Generic;
using _Infrastructure;
using Core._Services._Saving;
using Playgama;
using Playgama.Modules.Storage;
using UnityEngine;

namespace _Services._Saving {
    public class DataSaver_PG : IDataSaver {
        private bool isLoaded = false;
        private IKeysStorage _keysStorage;
        private DataParserTool _dataParser;
        private Dictionary<string, object> _savedData;
        private DateTime startLoadTime;

        // --- Флаги для надежного сохранения ---
        private bool isSaving = false;
        private bool isDirty = false; // Флаг, который показывает, что данные изменились и их нужно сохранить

        private DataSaver_PG() {
            Debug.Log($"ZEN__ PlaygamaDataSaver constructor");
        }

        public IEnumerator LoadData(IKeysStorage keysStorage) {
            Debug.Log($"ZEN__ PlaygamaDataSaver LoadData");

            _keysStorage = keysStorage;
            isLoaded = false;
            Bridge.storage.Get(_keysStorage.GetAllKeys(), OnLoadComplete, GetStorageType());
            startLoadTime = DateTime.Now;
            yield return new WaitUntil(() => isLoaded);
        }

        /** -- !!! ATTENTION !!! ---
         * This method MUST BE CALLED exceptionally before the LoadData method called.
         * IF YOU ARE NOT SURE THAT THE LoadData BE CALLED AFTER THIS METHOD
         * then define keys with default values in your custom KeysStorage.
        */
        public void RegisterSaveKey(string key, object defaultValue) {
            RegisterSaveKey(new Dictionary<string, object> {
                { key, defaultValue }
            });
        }

        /** -- !!! ATTENTION !!! ---
         * This method MUST BE CALLED exceptionally before the LoadData method called.
         * IF YOU ARE NOT SURE THAT THE LoadData BE CALLED AFTER THIS METHOD
         * then define keys with default values in your custom KeysStorage.
        */
        public void RegisterSaveKey(Dictionary<string, object> defaultValues) {
            Debug.Log($"ZEN__ PlaygamaDataSaver RegisterSaveKey");
            foreach (var (key, value) in defaultValues) {
                _keysStorage.TryToAddDefaultValue(key, value);
            }
        }

        // --- Главные изменения здесь ---

        // Перегрузка для совместимости со старыми вызовами

        public void SetData(string key, string value) {
            SetData(key, (object)value);
        }

        public void SetData(Dictionary<string, object> map) {
            foreach (var (key, value) in map) {
                if (_savedData.ContainsKey(key)) {
                    _savedData[key] = value;
                } else {
                    _savedData.Add(key, value);
                }
            }

            isDirty = true; // Помечаем, что данные "грязные" (изменились)

            // Если процесс сохранения не запущен, запускаем его
            if (!isSaving) {
                StartSave();
            }
        }

        public void SetData(string key, object value) {
            if (_savedData == null) {
                Debug.LogError("SetData called before data was loaded. This should not happen.");
                return;
            }

            _savedData[key] = value;
            isDirty = true; // Помечаем, что данные "грязные" (изменились)

            // Если процесс сохранения не запущен, запускаем его
            if (!isSaving) {
                StartSave();
            }
        }

        private void StartSave() {
            // Если сохранять нечего (флаг не взведен), выходим
            if (!isDirty) {
                isSaving = false;
                return;
            }

            isSaving = true;
            isDirty = false; // Сбрасываем флаг, так как мы начинаем сохранение этих данных

            var keys = new List<string>(_savedData.Keys);
            var values = new List<object>(_savedData.Values);
            Bridge.storage.Set(keys, values, OnSaveComplete, GetStorageType());
        }

        private void OnSaveComplete(bool success) {
            isSaving = false;
            // Если данные снова изменились, ПОКА шло сохранение, запускаем новый процесс
            if (isDirty) {
                StartSave();
            }
        }

        // --- Остальная часть класса без изменений ---

        private void OnLoadComplete(bool isSuccess, List<string> values) {
            Debug.Log($"PGS_ isSuccess: {isSuccess}");
            Debug.Log($"PGS_ values is null: {values == null}");
            var time = (DateTime.Now - startLoadTime).TotalSeconds;
            isLoaded = true;
            _savedData = new Dictionary<string, object>();
            if (!isSuccess) {
                _dataParser = new DataParserTool(_keysStorage.GetDefaultValues(), _keysStorage);
                //return;
            }

            if (values == null) {
                _dataParser = new DataParserTool(_keysStorage.GetDefaultValues(), _keysStorage);
            }

            var allKeys = _keysStorage.GetAllKeys();
            if (values != null) {
                for (var i = 0; i < allKeys.Count; i++) {
                    // Defensive check to prevent out of bounds if saved values are less than keys
                    if (i < values.Count) {
                        _savedData.Add(allKeys[i], values[i]);
                        //Debug.Log($"allKeys[{i}] = {values[i]}]");
                    }
                }
            }

            foreach (var kv in _keysStorage.GetDefaultValues()) {
                if (!_savedData.ContainsKey(kv.Key)) {
                    _savedData.Add(kv.Key, kv.Value);
                } else if (_savedData[kv.Key] == null || string.IsNullOrEmpty(_savedData[kv.Key].ToString())) {
                    _savedData[kv.Key] = kv.Value;
                }
            }

            _dataParser = new DataParserTool(_savedData, _keysStorage);
        }

        public void DeleteAll() {
            Bridge.storage.Delete(_keysStorage.GetAllKeys(), null, GetStorageType());
        }

        public void Delete(string key) {
            Bridge.storage.Delete(key, null, GetStorageType());
        }

        public IKeysStorage GetKeysStorage() {
            return _keysStorage;
        }

        private StorageType GetStorageType() {
#if UNITY_EDITOR
            // В редакторе всегда используем локальное хранилище для удобства
            return StorageType.LocalStorage;
#else
            // В билде проверяем, авторизован ли игрок
            return Bridge.player.isAuthorized
                ?
                // Если да, используем облачное хранилище платформы
                StorageType.PlatformInternal
                :
                // Если нет (гость), используем локальное хранилище браузера
                StorageType.LocalStorage;
#endif
        }

        public int GetDataInt(string key) => _dataParser.GetDataInt(key);
        public long GetDataLong(string key) => _dataParser.GetDataLong(key);
        public float GetDataFloat(string key) => _dataParser.GetDataFloat(key);

        public bool GetDataBool(string key) {
            Debug.Log($"PG GetDataBool key: {key}");
            return _dataParser.GetDataBool(key);
        }

        public string GetDataString(string key) => _dataParser.GetDataString(key);
    }
}