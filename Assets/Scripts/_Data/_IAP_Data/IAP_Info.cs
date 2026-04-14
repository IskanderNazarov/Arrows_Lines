using Core._RewardPresenter;
using UnityEngine;

namespace _ScriptableObjects._IAP_Data {
    [CreateAssetMenu(fileName = "config", menuName = "Scriptable/IAP_Info", order = 0)]
    public class IAP_Info : ScriptableObject {
        public ProductId productId;
        public bool isConsumable;
        public Reward reward;
    }
}