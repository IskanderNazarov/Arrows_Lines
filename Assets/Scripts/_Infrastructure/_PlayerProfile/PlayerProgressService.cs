// File: Assets/Game/Scripts/PlayerProgressService.cs

using System;
using System.Collections;
using _Infrastructure;
using Core._Services;
using Core._Services._Saving;

public class PlayerProgressService : SaveManager<PlayerSaveData> {
    public int CurrentLevelIndex => Data.CurrentLevelIndex;
    public int Score => Data.CurrentScore;

    public event Action<int> OnLevelChanged;

    // constructor passes game specific key
    public PlayerProgressService(IDataSaver dataSaver) 
        : base(dataSaver, GameKeys.PlayerData) { }

    public override IEnumerator Initialize() {
        yield return base.Initialize();
        CheckMigrations();
    }

    public void AddScore(int scoreDelta) {
        Data.CurrentScore += scoreDelta;
        MarkDirty(); // don't save to cloud immediately
    }

    public void CompleteLevel() {
        Data.CurrentLevelIndex++;
        MarkDirty();
        OnLevelChanged?.Invoke(Data.CurrentLevelIndex);
        SaveImmediate(); // save immediately on important milestone
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