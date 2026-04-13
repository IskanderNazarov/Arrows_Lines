using System;
using System.Collections.Generic;
using UnityEngine;

// Using your namespace

// 1. Data Structure
[CreateAssetMenu(fileName = "Level_00", menuName = "Game/LevelData")]
public class LevelData : ScriptableObject {
    public int LevelIndex;
    public Vector2Int GridSize = new Vector2Int(6, 6);
    public List<SnakeConfig> Snakes;
}

[Serializable]
public class SnakeConfig {
    // 0 is Head, Last is Tail
    public List<Vector2Int> BodyPositions;
    public Color Color = Color.green;
}