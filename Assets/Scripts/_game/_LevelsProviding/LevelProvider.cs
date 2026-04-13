using System.Collections.Generic;
using _ScriptableObjects._LevelData;
using UnityEngine;
using Zenject;

namespace _game._LevelsProviding {
    public class LevelProvider : ILevelProvider {
        // Инжектим сервис прогресса
        [Inject] private PlayerProgressService _progressService;

        private List<LevelData> _allLevels;

        // С какого индекса начинаем зацикливание (Индекс 29 = 30-й уровень)
        private const int LOOP_START_INDEX = 30;

        [Inject]
        private void Construct(LevelsDatabase levelsDatabase) {
            _allLevels = levelsDatabase.levelDatas;
        }

        public LevelData GetCurrentLevel() {
            // Берем реальный (бесконечный) индекс уровня игрока
            int rawIndex = _progressService.CurrentLevelIndex;
            // Прогоняем через метод получения с зацикливанием
            return GetLevel(rawIndex);
        }

        public LevelData GetLevel(int level) {
            if (_allLevels == null || _allLevels.Count == 0) {
                Debug.LogError("LevelsDatabase is empty or null!");
                return null;
            }

            if (level < 0) {
                Debug.LogWarning($"Negative level requested: {level}. Defaulting to 0.");
                level = 0;
            }

            // Высчитываем правильный индекс для базы данных
            int mappedIndex = GetMappedLevelIndex(level);

            return _allLevels[mappedIndex];
        }

        // --- МАТЕМАТИКА ЗАЦИКЛИВАНИЯ ---
        private int GetMappedLevelIndex(int rawIndex) {
            int totalLevels = _allLevels.Count;

            // 1. Если уровень в пределах уникальных (например, от 0 до 99), отдаем как есть
            if (rawIndex < totalLevels) {
                return rawIndex;
            }

            // Страховка: если в базе данных меньше 30 уровней, используем обычное зацикливание с нуля
            if (totalLevels <= LOOP_START_INDEX) {
                return rawIndex % totalLevels;
            }

            // 2. Если уровни закончились, начинаем математическое зацикливание
            // Вычисляем длину цикла (например, 100 - 29 = 71 уровень в цикле)
            int loopLength = totalLevels - LOOP_START_INDEX;

            // Формула: стартовый индекс цикла + (остаток от деления пройденных сверх лимита уровней на длину цикла)
            int mappedIndex = LOOP_START_INDEX + ((rawIndex - totalLevels) % loopLength);

            return mappedIndex;
        }
    }
}