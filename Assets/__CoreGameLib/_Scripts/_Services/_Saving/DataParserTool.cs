using System;
using System.Collections.Generic;
using System.Globalization;
using _Services._Saving;
using UnityEngine;

namespace _Infrastructure {
    public class DataParserTool {
        private Dictionary<string, object> _data;
        private IKeysStorage _keysStorage;

        public DataParserTool(Dictionary<string, object> data, IKeysStorage keysStorage) {
            _data = data;
            _keysStorage = keysStorage;
        }

        //------------------------------------------------------------------
        public float GetDataFloat(string key) =>
            GetParsed(key, v => (float.TryParse(v, NumberStyles.Float,
                CultureInfo.InvariantCulture, out var r), r));

        //------------------------------------------------------------------
        public bool GetDataBool(string key) {
            return GetParsed(key, v => {

                // Сначала пробуем распарсить как булево слово ("True"/"False")
                if (bool.TryParse(v, out var boolResult)) return (true, boolResult);

                // Если не вышло, пробуем как число ("1"/"0")
                if (int.TryParse(v, out var intResult)) return (true, intResult != 0);

                // Если ничего не подошло
                return (false, false);
            });
        }

        //------------------------------------------------------------------
        public int GetDataInt(string key) {
            return GetParsed(key, v => (int.TryParse(v, out var r), r));
        }
        public long GetDataLong(string key) {
            return GetParsed(key, v => (long.TryParse(v, out var r), r));
        }

        //------------------------------------------------------------------
        public string GetDataString(string key) {
            return GetParsed(key, v => (true, v));
        }

        //------------------------------------------------------------------
        private T GetParsed<T>(string key, Func<string, (bool, T)> parser) {
            var defaultValue = _keysStorage.GetDefaultValue<T>(key);
            //Debug.Log($"DS__ defaultValue: {defaultValue}, type: {defaultValue.GetType()}");

            if (_data == null) {
                return defaultValue;
            }

            if (!_data.ContainsKey(key)) {
                PrintNoValueMessage(key, $"defaultValue: {defaultValue}, if 1");
                return defaultValue;
            }

            var value = _data[key];
            if (value == null || string.IsNullOrEmpty(value.ToString()) ||
                string.IsNullOrWhiteSpace(value.ToString())) {
                PrintNoValueMessage(key, "if 2");
                return defaultValue;
            }

            var (success, parsed) = parser(value.ToString());
            if (success) return parsed;

            PrintCouldNotParseMessage(value.ToString());
            return defaultValue;
        }

        //------------------------------------------------------------------
        private void PrintNoValueMessage(string key, string placement) {
            Debug.LogWarning($"Value doesn't exist for key {key}. Return default value, placement: {placement}");
        }

        private void PrintCouldNotParseMessage(string value) {
            Debug.LogWarning($"Couldn't parse value: {value}");
        }
    }
}