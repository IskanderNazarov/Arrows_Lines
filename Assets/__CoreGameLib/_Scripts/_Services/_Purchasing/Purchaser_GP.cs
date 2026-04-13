using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GamePush;
using Core._Purchasing;

namespace __CoreGameLib._Scripts._Services._Purchasing {
    public class Purchaser_GP : IPurchaser {
        
        public event Action<string, bool> OnPurchaseCompletedEvent;
        
        private List<ProductInfo> _productsInfo = new List<ProductInfo>();
        
        // internal flags for initialization
        private bool _productsFetched;
        private bool _purchasesFetched;
        private bool _isPurchaseInProgress;
        
        // temporary storage for restoration logic
        private List<string> _ownedProductTags = new List<string>();

        public IEnumerator Initialize(bool isSupported) {
            if (!isSupported) yield break;

            _productsFetched = false;
            _purchasesFetched = false;
            _ownedProductTags.Clear();

            // calling fetch to get both products and player purchases
            // based on your gp_payments.cs signature: 
            // Fetch(Action<List<FetchProducts>>, Action, Action<List<FetchPlayerPurchases>>)
            GP_Payments.Fetch(
                onFetchProducts: OnProductsReceived,
                onFetchProductsError: OnFetchError,
                onFetchPlayerPurchases: OnPurchasesReceived
            );

            // wait until both callbacks are finished
            yield return new WaitUntil(() => _productsFetched && _purchasesFetched);
            
            // process restoration after we have both lists
            ProcessRestoration();
        }

        public void BuyItem(string id) {
            if (_isPurchaseInProgress) return;

            _isPurchaseInProgress = true;

            // gamepush purchase using tag
            GP_Payments.Purchase(
                idOrTag: id,
                onPurchaseSuccess: (tag) => {
                    _isPurchaseInProgress = false;
                    // notify game: success, isRestoring = false
                    OnPurchaseCompletedEvent?.Invoke(tag, false);
                },
                onPurchaseError: () => {
                    Debug.LogWarning("gp purchase failed");
                    _isPurchaseInProgress = false;
                }
            );
        }

        public ProductInfo GetProdInfoByID(string id) {
            return _productsInfo.FirstOrDefault(p => p.id == id);
        }

        // --- internal handlers ---

        private void OnProductsReceived(List<FetchProducts> gpProducts) {
            _productsInfo.Clear();

            foreach (var gpProd in gpProducts) {
                var info = new ProductInfo {
                    id = gpProd.tag, // using tag as id
                    priceValue = gpProd.price.ToString(),
                    // formatting price: "100 YAN"
                    price = $"{gpProd.price} {gpProd.currencySymbol}" 
                };
                _productsInfo.Add(info);
            }
            
            _productsFetched = true;
        }

        private void OnPurchasesReceived(List<FetchPlayerPurchases> gpPurchases) {
            _ownedProductTags.Clear();
            if (gpPurchases != null) {
                foreach (var purchase in gpPurchases) {
                    _ownedProductTags.Add(purchase.tag);
                }
            }
            _purchasesFetched = true;
        }

        private void OnFetchError() {
            // mark as fetched to prevent infinite loading loop
            _productsFetched = true;
            _purchasesFetched = true; 
            Debug.LogWarning("gp fetch products error");
        }

        private void ProcessRestoration() {
            // now check which products are owned
            foreach (var ownedTag in _ownedProductTags) {
                // verify we actually know this product
                var product = GetProdInfoByID(ownedTag);
                if (product != null) {
                    // notify game: success, isRestoring = true
                    OnPurchaseCompletedEvent?.Invoke(ownedTag, true);
                }
            }
        }
        
        public void ConsumePurchase(string id) {
            GP_Payments.Consume(id);
        }
    }
}