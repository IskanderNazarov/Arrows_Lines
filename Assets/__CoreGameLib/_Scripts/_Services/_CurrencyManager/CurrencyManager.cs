using System;
using System.Collections;
using System.Collections.Generic;
using __CoreGameLib._Scripts._Services._Saving;
using Core._Services._Saving;
using UnityEngine;
using Zenject;

namespace Core._Services {
    public class CurrencyManager {
        private Dictionary<CurrencyType, int> currencyMap;


        public Action<CurrencyType, int> OnCurrencyCountChanged; //type and added value

        private int scoreCount;
        private bool isDataLoaded;
        public bool IsInitialized;
        private IDataSaver _dataSaver;

        [Inject]
        private void Constr(IDataSaver dataSaver) {
            _dataSaver = dataSaver;
        }

        private CurrencyManager() {
            currencyMap = new Dictionary<CurrencyType, int>();
        }

        public IEnumerator Initialize() {
            //private void OnLoadComplete(bool isSuccess, List<string> values)

            var curTypes = (CurrencyType[])Enum.GetValues(typeof(CurrencyType));
            for (var i = 0; i < curTypes.Length; i++) {
                var key = GetKey(curTypes[i]);
                var value = _dataSaver.GetDataInt(key);
                currencyMap.Add(curTypes[i], value);
            }

            isDataLoaded = true;

            yield return null;
        }

        public int GetValue(CurrencyType currencyType) {
            if (!currencyMap.ContainsKey(currencyType)) {
                return 0;
            }

            return currencyMap[currencyType];
        }

        public void SetValue(CurrencyType type, int value) {
            if (!currencyMap.ContainsKey(type)) {
                currencyMap.Add(type, 0);
            }

            currencyMap[type] = 0;
            AddValue(type, value);
        }

        public void AddValue(CurrencyType type, int addedValue) {
            if (!currencyMap.ContainsKey(type)) {
                currencyMap.Add(type, 0);
            }

            currencyMap[type] += addedValue;
            OnCurrencyCountChanged?.Invoke(type, addedValue);

            if (type == CurrencyType.Score) {
                //save score record each time a value added to score counter
                if (currencyMap[type] > _dataSaver.GetDataInt(CoreKeys.HighScoreKey)) {
                    _dataSaver.SetData(CoreKeys.HighScoreKey, currencyMap[type].ToString());
                }
            }

            SaveData(type, currencyMap[type]);
        }

        public void ConsumeValue(CurrencyType type, int value) {
            if (!currencyMap.ContainsKey(type) || currencyMap[type] < value) {
                return;
            }

            currencyMap[type] -= value;
            currencyMap[type] = Mathf.Max(0, currencyMap[type]);
            OnCurrencyCountChanged?.Invoke(type, -value);
            SaveData(type, currencyMap[type]);
        }

        public bool CanConsumeValue(CurrencyType type, int value) {
            return currencyMap.ContainsKey(type) && currencyMap[type] >= value;
        }

        private void SaveData(CurrencyType type, int value) {
            var key = GetKey(type);
            Debug.Log($"SaveData for type: {type}");
            //Bridge.storage.Set(key, value.ToString());
            _dataSaver.SetData(key, value.ToString());
        }

        private string GetKey(CurrencyType type) {
            return type switch {
                CurrencyType.Coins => CoreKeys.CoinsKey,
                CurrencyType.Gems => CoreKeys.GemsKey,
                CurrencyType.Score => CoreKeys.ScoreKey
            };
        }
    }
}
