using System.Collections.Generic;
using UnityEngine;

namespace _ScriptableObjects._LevelData {
    [CreateAssetMenu(fileName = "LevelsDatabase", menuName = "Game/LevelsDatabase")]
    public class LevelsDatabase : ScriptableObject {
        public List<LevelData> levelDatas;
    }
}