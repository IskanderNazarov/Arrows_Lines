using _ScriptableObjects._IAP_Data;
using _UI._Market;
using Core._Purchasing;
using Core._RewardPresenter;
using Core._Services;
using core.purchasing;
using TMPro;
using UnityEngine;
using Zenject;

namespace Core._UI {
    public class Market : MonoBehaviour {
        [SerializeField] private MarketPanel[] panels;
        [SerializeField] private TextMeshProUGUI getRewardTitle;
        [SerializeField] private TextMeshProUGUI rewardPanelTitle;
        [SerializeField] private TextMeshProUGUI marketTitle;

        private IPurchaser _purchaser;
        private RewardHandler _rewardHandler;
        private GameConfig _gameConfig;
        private IAP_Config _iapConfig;
        private PurchaseHandler _purchaseHandler;
        private bool _isInit;

        [Inject]
        private void Construct(IPurchaser purchaser, GameConfig gameConfig, IAP_Config iapConfig, RewardHandler rewardHandler) {
            _purchaser = purchaser;
            _gameConfig = gameConfig;
            _iapConfig = iapConfig;
            _rewardHandler = rewardHandler;
        }

        public void Show() {
            gameObject.SetActive(true);

            if (!_isInit) {
                for (var i = 0; i < panels.Length; i++) {
                    var panel = panels[i];
                    var storeLiteralId = _iapConfig.GetStoreId(panel.ProductId);
                    var productInfo = _purchaser.GetProdInfoByID(storeLiteralId);
                    var iapInfo = _iapConfig.GetInfoById(panel.ProductId);
                    //Debug.Log($"info {i}, idsList[{i}: {iapInfo.productId}]:\n {productInfo}");
                    panel.Init();
                    panel.UpdatePanel(productInfo, iapInfo);
                    panel.OnBuyClicked += OnBuyClicked;
                }

                _isInit = true;
            }

            // getRewardTitle.text = Localizer.Get;
            // marketTitle.text = Localizer.Market;
            //rewardPanelTitle.text = string.Format(Localizer.GetFreeCoins, _gameConfig.gemsForRewarded);
        }

        public void Hide() {
            gameObject.SetActive(false);
        }

        private void OnEnable() {
            _purchaser.OnPurchaseCompletedEvent += OnPurchaseCompletedEvent;
        }

        private void OnDisable() {
            _purchaser.OnPurchaseCompletedEvent -= OnPurchaseCompletedEvent;
        }

        private void OnPurchaseCompletedEvent(string id, bool showPresenter) {
            Hide();
        }

        public void GetFreeCoins() {
        }

        private void OnBuyClicked(ProductId productId) {
            print($"OnBuyClicked, productId: {productId}");
#if UNITY_EDITOR
            _rewardHandler.HandlerReward(new Reward {
                //Gems = _gameConfig.gemsForRewarded
            }, "RewardInMarket");
            return;
#endif
            var id = _iapConfig.GetStoreId(productId);
            _purchaser.BuyItem(id);
        }
    }
}