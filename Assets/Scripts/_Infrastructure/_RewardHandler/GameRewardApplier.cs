// Файл: Assets/Game/Scripts/Rewards/GameRewardApplier.cs
// Сборка: Game.asmdef

using Core._RewardPresenter;
using Core._Services;
using core.rewards;
using Rewards;
using UnityEngine;
using Zenject;
// Там где CurrencyManager
// Там где интерфейс IRewardApplier
// Там где твои бустеры

namespace Services {
    public class GameRewardApplier : IRewardApplier {

        private readonly CurrencyManager _currencyManager;

        [Inject]
        public GameRewardApplier(CurrencyManager currencyManager) {
            _currencyManager = currencyManager;
        }

        public void ApplyReward(Reward reward, string placement) {
            Debug.Log($"[GameRewardApplier] Applying reward from: {placement}");

            // 1. Базовые валюты (Core логика)
            if (reward.Coins > 0) {
                _currencyManager.AddValue(CurrencyType.Coins, reward.Coins);
                Debug.Log($"Added {reward.Coins} Coins");
            }

            if (reward.Gems > 0) {
                _currencyManager.AddValue(CurrencyType.Gems, reward.Gems);
                Debug.Log($"Added {reward.Gems} Gems");
            }

            // 2. Кастомные предметы (Game логика)
            if (reward.Items != null) {
                foreach (var item in reward.Items) {
                    ApplyGameItem(item, placement);
                }
            }
        }

        private void ApplyGameItem(IRewardItem item, string placement) {
            switch (item) {
            // Если это Флаг (твой новый тип)
            case FlagRewardItem flagItem:
                Debug.Log($"Added {flagItem.Amount} Flags (Logics not implemented yet)");
                // _flagInventory.AddFlags(flagItem.Amount);
                break;
            default:
                Debug.LogWarning($"[GameRewardApplier] Unknown reward item type: {item.GetType().Name}");
                break;
            }
        }
    }
}
