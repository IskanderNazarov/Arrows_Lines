using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSaveData {
    public int Version = 1; // Для будущих обновлений базы
    public int CurrentLevelIndex = 0;
    public int CurrentScore = 0;
    public bool IsMusicOn = true;
    public bool IsSoundOn = true;
    
    public List<int> BoosterCounts = new List<int> { 2, 2, 2 };
}