using System;
using System.Collections.Generic;
using UnityEngine;
using _ScriptableObjects; // Убедитесь, что этот using совпадает с вашим пространством имен

public enum AppearAnimType {
    Instant, // Мгновенное появление (как сейчас)
    FastFlyAndFade, // Ракета прилетает, дым плавно появляется
    PullSmoke // Ракета прилетает и "вытягивает" за собой дым
}

public interface ISnakeController {
    // --- Unity Properties ---
    // Добавляем эти свойства, чтобы GameplayController мог брать объект и двигать его
    GameObject gameObject { get; }
    Transform transform { get; }

    // --- Logical Properties ---
    Vector2Int HeadPosition { get; }
    Vector2Int Direction { get; }
    IReadOnlyList<Vector2Int> BodyPositions { get; }
    bool IsMoving { get; }
    Color SnakeColor { get; }

    // --- Methods ---
    void Initialize(int index, SnakeConfig config, float cellSize);
    void MoveToExit(List<Vector2Int> fullPath, float cellSize, Action onComplete, Action<Vector2Int> onTailLeaveCell = null);
    void PlayBumpAnimation(float bumpDistance, float cellSize, Action onComplete);
    void PlayAppearAnimation(AppearAnimType type, float duration, int index, Action onComplete);
    
    void ShowExitLine();
    void HideExitLine();
}