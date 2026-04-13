using System;
using _ScriptableObjects._IAP_Data;
using Core._Purchasing;
using TMPro;
using UnityEngine;
using Zenject;

namespace _UI._Market {
    public class MarketPanel : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI priceTMP;
        [SerializeField] private TextMeshProUGUI titleTMP;
        [SerializeField] private ProductId productId;

        public Action<ProductId> OnBuyClicked;
        public ProductId ProductId => productId;

        private GameConfig _gameConfig;

        [Inject]
        private void Constr(GameConfig gameConfig, IAP_Config iAP_Config) {
            _gameConfig = gameConfig;
        }

        public void Init() {
        }

        public void UpdatePanel(ProductInfo info, IAP_Info iapInfo) {
            if (info == null) return;
            Debug.Log($"info.price: {info.price}, name: {name}");
            priceTMP.text = info.price;

            var c = ColorUtility.ToHtmlStringRGBA(_gameConfig.marketValueCountColor);
            var f = $"<color=#{c}>{iapInfo.reward.Gems}</color>";
           // titleTMP.text = string.Format(Localizer.MarketPanelTitle, f);
        }

        public void ClickOnBuyButton() {
            OnBuyClicked?.Invoke(productId);
        }
    }
}