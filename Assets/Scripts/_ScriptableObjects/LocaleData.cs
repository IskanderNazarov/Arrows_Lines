using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/LocaleData", order = 3)]
public class LocaleData : ScriptableObject {
    public LangCode language;
    
    public string musicTitle;
    public string soundTitle;
    public string MarketPanelTitle;
    public string Get;
    public string GetFreeCoins;
    public string Market;
    public string Level;
    public string Continue;
    public string Restart;
    public string[] Congrats;
    
    public string YourReward;
    public string ExtraReward;
    public string ChestRewardTitle;
    public string LeaderboardTitle;
    public string LoginBtnTe;
    public string AuthInfo;
    [FormerlySerializedAs("InstructTitle")] public string LeaderboardInfoTitle;
    public string LeaderboardInfoStory;
    public string LeaderboardLoadError;
    public string AuthFailedError;
    public string LeaderboardServiceNotAvailable;
}
