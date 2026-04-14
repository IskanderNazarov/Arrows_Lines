// File: Assets/Game/Scripts/PlayerProgressService.cs

using System;
using System.Collections;
using _Infrastructure;
using _Services;
using Core._Services;
using Core._Services._Saving;

public class PlayerProgressService : SaveManager<PlayerSaveData> {
    private const string SaveKey = "player_data";
    
    public int CurrentLevelIndex => Data.CurrentLevelIndex;
    public int Score => Data.CurrentScore;
    public bool IsAnyPlanetUnlocked => Data.UnlockedPlanetsCount > 0;

    public event Action<int> OnLevelChanged;

    public PlayerProgressService(IDataSaver dataSaver) 
        : base(dataSaver, SaveKey) { }

    public override IEnumerator Initialize() {
        yield return base.Initialize();
        CheckMigrations();
    }
    
    // method for UI to get snapshot of progress changes
    public ProgressUpdateResult AddStarsAndGetResult(int starsToAdd) {
        var result = new ProgressUpdateResult();
        
        // collect state before changes
        result.StartStars = Data.CurrentStars;
        result.TargetStarsBefore = 100; // todo: get from config/planet data

        // apply changes
        Data.CurrentStars += starsToAdd;

        // check for unlock (big star)
        if (Data.CurrentStars >= result.TargetStarsBefore) {
            result.DidUnlock = true;
            Data.BigStars++;
            Data.CurrentStars -= result.TargetStarsBefore;
            
            // todo: set UnlockedReward based on current planet
            // result.UnlockedReward = ... 
        }

        // collect state after changes
        result.EndStars = Data.CurrentStars;
        result.TargetStarsAfter = 100; // todo: get next target if needed

        MarkDirty();
        return result;
    }

    public void AddStars(int count) {
        Data.CurrentStars += count;
        MarkDirty();
    }

    public void AddCoins(int count) {
        Data.CurrentCoins += count;
        MarkDirty();
    }

    public void AddBigStar() {
        Data.BigStars++;
        MarkDirty();
        SaveImmediate();
    }

    public void CompleteLevel() {
        Data.CurrentLevelIndex++;
        MarkDirty();
        OnLevelChanged?.Invoke(Data.CurrentLevelIndex);
        SaveImmediate(); 
    }

    public int GetBoosterCount(int boosterId) {
        if (boosterId < 0 || boosterId >= Data.BoosterCounts.Count) return 0;
        return Data.BoosterCounts[boosterId];
    }

    public void SetBoosterCount(int boosterId, int count) {
        while (Data.BoosterCounts.Count <= boosterId) {
            Data.BoosterCounts.Add(2); // default count
        }
        
        Data.BoosterCounts[boosterId] = count;
        MarkDirty();
    }

    private void CheckMigrations() {
        // handle versioning here
        if (Data.Version < 1) {
            // update logic
        }
    }

    public void ResetProgress() {
        Data = new PlayerSaveData();
        MarkDirty();
        SaveImmediate();
    }
}