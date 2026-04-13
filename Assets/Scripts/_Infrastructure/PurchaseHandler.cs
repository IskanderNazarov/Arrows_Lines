// file: assets/game/scripts/purchasehandler.cs
// assembly: game.asmdef

using System.Collections.Generic;
using _ScriptableObjects._IAP_Data;
using Core._Purchasing;
using UnityEngine;
using Zenject;

namespace core.purchasing {
    public class PurchaseHandler : IInitializable { 
        
        private readonly IPurchaser _purchaser;
        private readonly IAP_Config _iapConfig;
        private readonly RewardHandler _rewardHandler;

        [Inject]
        public PurchaseHandler(IPurchaser purchaser, IAP_Config iapConfig, RewardHandler rewardHandler) {
            _purchaser = purchaser;
            _iapConfig = iapConfig;
            _rewardHandler = rewardHandler;
        }

        public void Initialize() {
            _purchaser.OnPurchaseCompletedEvent += OnPurchaseCompleted;
        }

        private void OnPurchaseCompleted(string id, bool isRestoringPurchase) {
            Debug.Log($"purchasehandler: received purchase id: {id}, isrestoring: {isRestoringPurchase}");
            
            var iapData = _iapConfig.GetInfoById(id);
            if (iapData == null) {
                Debug.LogError($"purchasehandler: iap_info not found for id: {id}");
                return;
            }
            
            // 1. grant reward
            var reward = iapData.reward;
            
            // don't show ui popup if restoring
            _rewardHandler.HandlerReward(reward, "purchase", !isRestoringPurchase);

            // 2. consume if needed
            if (iapData.isConsumable) {
                Debug.Log($"purchasehandler: consuming item id: {id}");
                _purchaser.ConsumePurchase(id);
            }
        }

        private void OnConsumeFinished(bool isSuccess, Dictionary<string, string> purchases) {
            if (isSuccess) {
                Debug.Log($"purchasehandler: consume successful.");
            } else {
                Debug.LogError($"purchasehandler: consume failed.");
            }
        }
    }
}
