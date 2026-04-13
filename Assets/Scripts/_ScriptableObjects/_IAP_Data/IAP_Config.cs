using System.Collections.Generic;
using System.Linq;
using _Infrastructure;
using UnityEngine;

namespace _ScriptableObjects._IAP_Data {
    [CreateAssetMenu(fileName = "config", menuName = "Scriptable/IAP_Config", order = 0)]
    public class IAP_Config : ScriptableObject {
        public IAP_Info[] infos;

        private Dictionary<ProductId, string> productIdsMapping = new() {
            { ProductId.coins_1, "gems_1" },
            { ProductId.coins_2, "gems_2" },
            { ProductId.coins_3, "gems_3" },
        };

        private bool _isRemoteSetup;

        /*private void SetupFromRemoteConfig() {
            if (_isRemoteSetup) return;
            _isRemoteSetup = true;

            for (var i = 0; i < infos.Length; i++) {
                var iapInfo = infos[i];
                if (i < RCH.GEMS_VALUES.Length) {
                    iapInfo.reward.Gems = RCH.GEMS_VALUES[i];
                }
            }
        }*/

        public string GetStoreId(ProductId productId) {
            return productIdsMapping[productId];
        }

        private ProductId GetProductId(string productId) {
            foreach (var kv in productIdsMapping) {
                if (kv.Value == productId) {
                    return kv.Key;
                }
            }

            return default;
        }

        public IAP_Info GetInfoById(ProductId id) {
            //SetupFromRemoteConfig();
            
            return infos.First(d => d.productId == id);
        }

        public IAP_Info GetInfoById(string id) {
            //SetupFromRemoteConfig();
            
            var productId = GetProductId(id);
            return infos.First(d => d.productId == productId);
        }

        /*public Reward GetReward(ProductId) {

        }*/
    }
}

public enum ProductId {
    coins_1 = 1,
    coins_2 = 2,
    coins_3 = 3
}