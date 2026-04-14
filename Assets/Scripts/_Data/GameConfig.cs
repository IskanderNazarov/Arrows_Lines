using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "config", menuName = "Scriptables/BoxConfig", order = 5)]
public class GameConfig : ScriptableObject {
    public Color marketValueCountColor;

    public int ReviveCountCap;
    [FormerlySerializedAs("LivesCountCap")] public int HintsCountCap;
}