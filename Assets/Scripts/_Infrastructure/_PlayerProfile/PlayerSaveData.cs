using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSaveData {
    public int Version = 1;
    public int CurrentLevelIndex = 0;
    public int CurrentScore = 0;
    
    // new fields for meta-game
    public int CurrentStars = 0;
    public int CurrentCoins = 1000;
    public int BigStars = 0;
    public int UnlockedPlanetsCount = 0;
    
    public bool IsMusicOn = true;
    public bool IsSoundOn = true;
    
    public List<int> BoosterCounts = new List<int> { 2, 2, 2 };
}