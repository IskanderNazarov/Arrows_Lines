using System;
using System.Collections.Generic;
using UnityEngine;
using _Gameplay._UI;

namespace _Data {
    [CreateAssetMenu(fileName = "ProgressRewardsConfig", menuName = "Game/ProgressRewardsConfig")]
    public class ProgressRewardsConfig : ScriptableObject {
        [Tooltip("Список наград в порядке их открытия")]
        public List<PlanetRewardData> Rewards;
    }

    [Serializable]
    public struct PlanetRewardData {
        public Sprite Sprite; // Сама картинка планеты
        public string DisplayName; // Имя для UI (например, "Солнце")
    }
}