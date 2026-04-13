// Файл: Assets/_CoreGame/_Scripts/Boosters/BoosterInventory.cs
// Сборка: CoreLib.asmdef
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace core.boosters {
    
    public abstract class BoosterInventory<TBoosterId> : IBoosterInventory<TBoosterId>, IInitializable
        where TBoosterId : Enum {
        
        private Dictionary<TBoosterId, int> _caps;
        public event Action<TBoosterId, int> OnChanged;

        public BoosterInventory(Dictionary<TBoosterId, int> caps) {
            _caps = caps;
        }

        public void Initialize() { }

        // --- АБСТРАКТНЫЕ МЕТОДЫ ДЛЯ СЛОЯ GAME ---
        public abstract int GetCount(TBoosterId id);
        protected abstract void SaveCount(TBoosterId id, int newValue);

        // --- ЛОГИКА CORE (Осталась без изменений) ---
        public bool TryAdd(TBoosterId id, int amount, string source) {
            if (amount <= 0) return false;
            var current = GetCount(id);
            var cap = GetCap(id);
            var newValue = Mathf.Min(current + amount, cap);
            var delta = newValue - current;
            if (delta <= 0) return false;

            SaveCount(id, newValue); // Вызываем абстрактный метод
            OnChanged?.Invoke(id, delta);
            return true;
        }

        public bool TryConsume(TBoosterId id, int amount, string reason) {
            if (amount <= 0) return false;
            var current = GetCount(id);
            if (current < amount) return false;

            var newValue = current - amount;
            SaveCount(id, newValue); // Вызываем абстрактный метод
            OnChanged?.Invoke(id, -amount);
            return true;
        }

        public int GetCap(TBoosterId id) {
            return _caps.GetValueOrDefault(id, int.MaxValue);
        }

        public void SetCap(TBoosterId id, int cap) {
            _caps[id] = Mathf.Max(0, cap);
        }
    }
}