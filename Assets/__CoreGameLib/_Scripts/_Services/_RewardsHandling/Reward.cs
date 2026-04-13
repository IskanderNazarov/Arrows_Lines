// Файл: Assets/_CoreGame/_Scripts/Rewards/Reward.cs
// Сборка: CoreLib.asmdef

using System;
using System.Collections.Generic;
using UnityEngine; // Нужен для [SerializeReference]

namespace Core._RewardPresenter {
    
    // 1. Маркерный интерфейс. Core не знает, что в нем будет.
    public interface IRewardItem { }

    [Serializable]
    public class Reward {
        // Стандартные валюты
        public int Coins;
        public int Gems;

        // 2. Магия Unity. Этот атрибут позволяет хранить здесь классы, 
        // которые будут написаны В ДРУГОЙ СБОРКЕ (Game), 
        // и отображать их в Инспекторе.
        [SerializeReference, SelectImplementation]
        public List<IRewardItem> Items = new List<IRewardItem>();
    }
}