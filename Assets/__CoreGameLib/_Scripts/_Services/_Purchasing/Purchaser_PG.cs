// file: assets/_coregame/_scripts/purchasing/purchaser.cs
// assembly: corelib.asmdef

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Playgama;
using UnityEngine;

namespace Core._Purchasing {
    public class Purchaser_PG : IPurchaser {
        private bool _isPurchaseAvailable;
        private bool _isInitialized;
        private bool _isGetPurchaseCompleted;
        private bool _isSupported;
        public List<ProductInfo> productsInfo { get; private set; }

        // reports (string productID, bool isRestoringPurchase)
        public event Action<string, bool> OnPurchaseCompletedEvent;

        public IEnumerator Initialize(bool isSupported) {
            _isSupported = isSupported;

            Debug.Log($"purchaser start init");

            if (isSupported) {
                productsInfo = new List<ProductInfo>();
                Bridge.payments.GetCatalog(OnCatalogLoaded);
                yield return new WaitUntil(() => _isInitialized);

                Bridge.payments.GetPurchases(OnGetPurchaseComplete);
                yield return new WaitUntil(() => _isGetPurchaseCompleted);
            } else {
                _isInitialized = true;
            }

            _isPurchaseAvailable = true;
        }

        //--------------------------------------------------------------------------
        private void OnCatalogLoaded(bool isSuccess, List<Dictionary<string, string>> catalog) {
            _isInitialized = true;
            if (!isSuccess || catalog == null) {
                Debug.LogWarning($"failed to load catalog. issuccess: {isSuccess}, catalog == null: {catalog}");
                return;
            }

            Debug.Log("catalog loaded:");
            foreach (var d in catalog) {
                var prodInfo = new ProductInfo {
                    id = d["id"],
                    price = d["price"],
                    priceValue = d["priceValue"],
                };
                productsInfo.Add(prodInfo);
                Debug.Log($"-- {prodInfo.id}: {prodInfo.price}");
            }
        }

        private void OnGetPurchaseComplete(bool success, List<Dictionary<string, string>> purchasesList) {
            _isGetPurchaseCompleted = true;
            Debug.Log("ongetpurchasecomplete: success = " + success);

            if (success && purchasesList != null) {
                foreach (var purchase in purchasesList) {
                    // isrestoringpurchase = true
                    HandleBuyProduct(true, purchase, true);
                }
            }
        }

        //--------------------------------------------------------------------------
        public void BuyItem(string id) {
            Debug.Log($"purchaser: attempting to buy item: {id}");
            if (!_isPurchaseAvailable || !_isInitialized) {
                Debug.LogWarning($"purchase failed: available={_isPurchaseAvailable}, initialized={_isInitialized}");
                return;
            }

            _isPurchaseAvailable = false;
            Bridge.payments.Purchase(id, OnBuyProductComplete);
        }

        //--------------------------------------------------------------------------
        private void OnBuyProductComplete(bool isSuccess, Dictionary<string, string> purchases) {
            // isrestoringpurchase = false
            HandleBuyProduct(isSuccess, purchases, false);
            _isPurchaseAvailable = true;
        }

        /// <summary>
        /// unified handler for new and restored purchases
        /// </summary>
        private void HandleBuyProduct(bool isSuccess, Dictionary<string, string> purchases, bool isRestoringPurchase) {
            if (!isSuccess || purchases == null) {
                Debug.LogError($"handlebuyproduct failed. issuccess: {isSuccess}");
                return;
            }

            if (!purchases.ContainsKey("id")) {
                Debug.LogError("handlebuyproduct: 'id' not found in purchase dictionary.");
                return;
            }

            var id = purchases["id"];
            Debug.Log($"purchaser: handlebuyproduct success. id: {id}, isrestoring: {isRestoringPurchase}");

            OnPurchaseCompletedEvent?.Invoke(id, isRestoringPurchase);
        }

        //--------------------------------------------------------------------------
        public ProductInfo GetProdInfoByID(string id) {
            return productsInfo.FirstOrDefault(p => p.id == id);
        }

        public void ConsumePurchase(string id) {
            Bridge.payments.ConsumePurchase(id, OnConsumeFinished);
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