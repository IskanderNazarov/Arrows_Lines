using System;
using __CoreGameLib._Scripts._Services._Leaderboards;
using _Infrastructure;
using Core._Services._Saving;
using UnityEngine;
using Zenject;

public class PlayerProgressService {
    [Inject] private IDataSaver _dataSaver;
    [Inject] private GameConfig _progressConfig;
    [Inject] private ILeaderboardService _leaderboardService;

    private PlayerSaveData _data;

    public int CurrentLevelIndex => _data.CurrentLevelIndex;
    public int Score => _data.CurrentScore;

    public event Action<int> OnLevelChanged;

    public void Initialize() {
        string json = _dataSaver.GetDataString(GameKeys.PlayerData);

        Debug.Log($"Player json: {json}");
        if (string.IsNullOrEmpty(json)) {
            // 1. ИГРОК НОВЫЙ: Создаем профиль из конфига
            CreateNewProfile();
        }
        else {
            // 2. ИГРОК СТАРЫЙ: Загружаем профиль
            _data = JsonUtility.FromJson<PlayerSaveData>(json);

            // Здесь можно вызывать проверку миграций (см. ниже)
            CheckMigrations();
        }
    }

    public void DeleteAllData() {
        _data = new PlayerSaveData();
        SaveData();
    }

    public void AddScore(int scoreDelta) {
        _data.CurrentScore += scoreDelta;
        SaveData();
    }
    
    public int GetBoosterCount(int boosterId) {
        if (_data == null || boosterId < 0 || boosterId >= _data.BoosterCounts.Count) return 0;
        return _data.BoosterCounts[boosterId];
    }

    // Безопасное сохранение (автоматически расширяет список при необходимости)
    public void SetBoosterCount(int boosterId, int count) {
        if (_data == null) return;
        
        while (_data.BoosterCounts.Count <= boosterId) {
            // Если игрок новый или добавился новый бустер в апдейте, 
            // выдаем базовые 2 штуки (или 0, как захочешь)
            _data.BoosterCounts.Add(2); 
        }
        
        _data.BoosterCounts[boosterId] = count;
        SaveData();
    }

    private void CreateNewProfile() {
        _data = new PlayerSaveData();

        // Заполняем данные значениями из RemoteConfig / ScriptableObject
        //_data.CurrentLevelIndex = _progressConfig.StartLevelIndex;

        SaveData(); // Сразу сохраняем созданный профиль на диск
    }

    public void CompleteLevel() {
        _data.CurrentLevelIndex++;
        SaveData();
        OnLevelChanged?.Invoke(_data.CurrentLevelIndex);
    }

    private void SaveData() {
        string json = JsonUtility.ToJson(_data);
        _dataSaver.SetData(GameKeys.PlayerData, json);
    }

    private void CheckMigrations() {
        // Пример миграции: если мы обновили игру и добавили новую валюту (например, гемы),
        // у старых игроков поле Gems будет равно 0, потому что в их старом JSON его не было.
        // Здесь мы можем выдать им стартовые значения из конфига, если версия сохранения старая.
    }
}