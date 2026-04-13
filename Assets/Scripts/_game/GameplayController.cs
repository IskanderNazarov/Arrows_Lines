using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using __CoreGameLib._Scripts._Services._RemoteConfig;
using __Gameplay;
using _ExtensionsHelpers;
using _game;
using _game._LevelsProviding;
using _ScriptableObjects;
using core.boosters; // Добавлен using для BoosterId
using DG.Tweening;
using Sripts._Services._KeysStorage;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class GameplayController : MonoBehaviour {
    [Inject] private ILevelProvider _levelProvider;
    [Inject] private SoundHelper _soundHelper;
    [Inject] private SignalBus _signalBus;
    [Inject] private DiContainer _container;
    [Inject] private SnakesGlobalConfig _snakesGlobalConfig;
    [Inject] private GameConfig _gameConfig;
    [Inject] private AnimationsService _animationsService;
    [Inject] private IRemoteConfig _remoteConfig;
    [Inject] private ICameraService _cameraService;
    [Inject] private BoosterVisualService _boosterVisual;

    [SerializeField] private CameraController _cameraController;

    [SerializeField] private GameObject _snakePrefab;
    [SerializeField] private GameObject _dotPrefab;
    [SerializeField] private ParticleSystem _clickParticlePrefab;

    [Header("Settings")] 
    [SerializeField] private Transform _boardRoot;
    [SerializeField] private float _cellSize = 1.0f;
    [SerializeField] private float _dotInitScale = 0.05f;
    
    [Header("Board Framing")]
    [Tooltip("Полупрозрачный SpriteRenderer на сцене, внутри которого будет отрисовываться уровень")]
    [SerializeField] private SpriteRenderer _playAreaBounds; 
    [Tooltip("Дополнительный отступ внутри рамки (чтобы змейки не терлись об края)")]
    [SerializeField] private float _boundsPadding = 0.5f; 
    [SerializeField] private float _maxBoardScale = 3.0f;
    
    [Header("Input")]
    [SerializeField] private float _dragThreshold = 40f;
    [SerializeField] private int curLevel; // Оставил, если ты используешь это для дебага

    [FormerlySerializedAs("_startAnimDuration")] [SerializeField]
    private float _startAnimSpeed = 2f;

    public int ReviveCount => _reviveCount;
    public int SnakesCountForLevel => _startSnakesCount;

    // --- State ---
    private List<ISnakeController> _activeSnakes = new List<ISnakeController>();
    private Dictionary<Vector2Int, BoardDot> _activeDots = new Dictionary<Vector2Int, BoardDot>();
    private HashSet<Vector2Int> _occupiedCells = new HashSet<Vector2Int>();
    private Vector2Int _gridSize;
    private bool _levelActive;
    private Camera _cam;
    private const int MAX_LIVES = 3;
    private int _currentLives = MAX_LIVES;
    private Vector3 _mousePressPos;
    private bool _isValidClickStart;
    private int _reviveCount;

    private ParticleSystem _clickParticleInstance; 

    // --- State: Boosters ---
    private bool _isBoosterAnimating = false;
    private List<ISnakeController> _hintedSnakesList = new List<ISnakeController>();
    
    // --- Object Pools ---
    private Queue<ISnakeController> _snakePool = new Queue<ISnakeController>();
    private Queue<BoardDot> _dotPool = new Queue<BoardDot>();
    private int _movingSnakesCount = 0;
    private int _startSnakesCount;

    private void Start() {
        _cam = Camera.main;
        LoadLevel();
        _signalBus.Subscribe<ReviveSignal>(OnRevive);
    }

    private void OnDestroy() {
        _signalBus.Unsubscribe<ReviveSignal>(OnRevive);
    }

    private void Update() {
        if (!_levelActive || _isBoosterAnimating) return; // Блокируем клики во время анимации бустеров

        if (Input.GetMouseButtonDown(0)) {
            if (!Utils.IsPointerOverUIRaycastTarget()) {
                _mousePressPos = Input.mousePosition;
                _isValidClickStart = true;
            } else {
                _isValidClickStart = false;
            }
        }

        if (Input.GetMouseButtonUp(0) && _isValidClickStart) {
            _isValidClickStart = false;

            if (Vector3.Distance(_mousePressPos, Input.mousePosition) <= _dragThreshold) {
                HandleClick();
            }
        }
    }

    // ==========================================
    // ЛОГИКА УРОВНЕЙ И РУТИНЫ (GAME FLOW)
    // ==========================================

    public void ClearLevel() {
        StopAllCoroutines();
        _levelActive = false;
        _occupiedCells.Clear();
        _movingSnakesCount = 0;
        
        ClearAllHints();
        _isBoosterAnimating = false;

        foreach (var snake in _activeSnakes) {
            ReturnSnakeToPool(snake);
        }
        _activeSnakes.Clear();

        foreach (var dot in _activeDots.Values) {
            dot.ResetDot();
            ReturnDotToPool(dot);
        }
        _activeDots.Clear();
    }

    public void LoadLevel(int levelNumber = -1) {
        ClearLevel();
        _reviveCount = _gameConfig.ReviveCountCap;

        if (_cameraController != null) {
            _cameraController.ResetCamera();
        }

        LevelData data = levelNumber >= 0 ? _levelProvider.GetLevel(levelNumber) : _levelProvider.GetCurrentLevel();

        _startSnakesCount = data.Snakes.Count;
        _gridSize = data.GridSize;

        CenterAndScaleBoard(data);

        foreach (var sConf in data.Snakes) {
            foreach (var pos in sConf.BodyPositions) {
                _occupiedCells.Add(pos);
            }
        }

        CreateGridDots();

        var i = 0;
        foreach (var sConf in data.Snakes) {
            var snake = GetSnakeFromPool();
            snake.Initialize(i, sConf, _cellSize);
            _activeSnakes.Add(snake);
            i++;
        }

        StartCoroutine(StartLevelRoutine());
    }

    private IEnumerator StartLevelRoutine() {
        _levelActive = false;
        AppearAnimType currentAnimMode = AppearAnimType.PullSmoke;

        int pendingAnims = _activeSnakes.Count;
        if (pendingAnims == 0) {
            _levelActive = true;
            yield break;
        }

        var n = 0;
        foreach (var snake in _activeSnakes) {
            snake.PlayAppearAnimation(currentAnimMode, _startAnimSpeed, n, () => { pendingAnims--; });
            n++;
        }

        yield return new WaitUntil(() => pendingAnims == 0);

        _levelActive = true;
        _currentLives = MAX_LIVES;
        _signalBus.Fire(new LivesChangedSignal { CurrentLives = _currentLives });
    }

    private IEnumerator WinRoutine() {
        _levelActive = false;
        yield return new WaitForSeconds(0.3f);

        if (_cameraController != null) {
            _cameraController.ResetCamera();
        }

        yield return _animationsService.AnimateDotsAfterLevel(_activeDots);
        yield return new WaitForSeconds(0.2f);

        Debug.Log("Level Complete!");
        _signalBus.Fire<LevelCompletedSignal>();
    }

    private void CheckWin() {
        if (_activeSnakes.Count == 0 && _movingSnakesCount == 0) {
            StartCoroutine(WinRoutine());
        }
    }

    // ==========================================
    // POOLING ENGINE
    // ==========================================

    private ISnakeController GetSnakeFromPool() {
        if (_snakePool.Count > 0) {
            var snake = _snakePool.Dequeue();
            snake.gameObject.SetActive(true);
            return snake;
        }
        return _container.InstantiatePrefabForComponent<ISnakeController>(_snakePrefab, _boardRoot);
    }

    private void ReturnSnakeToPool(ISnakeController snake) {
        snake.transform.DOKill();
        snake.gameObject.SetActive(false);
        _snakePool.Enqueue(snake);
    }

    private BoardDot GetDotFromPool() {
        if (_dotPool.Count > 0) {
            return _dotPool.Dequeue();
        }
        return Instantiate(_dotPrefab, _boardRoot).GetComponent<BoardDot>();
    }

    private void ReturnDotToPool(BoardDot dot) {
        dot.transform.DOKill();
        dot.gameObject.SetActive(false);
        _dotPool.Enqueue(dot);
    }

    // ==========================================
    // ОСТАЛЬНАЯ ЛОГИКА ГЕЙМПЛЕЯ
    // ==========================================

    private void OnRevive() {
        _currentLives = MAX_LIVES;
        _reviveCount = Mathf.Max(0, _reviveCount - 1);
        _signalBus.Fire(new LivesChangedSignal { CurrentLives = _currentLives });
        _levelActive = true;
    }

    private void CreateGridDots() {
        foreach (var pos in _occupiedCells) {
            var dot = GetDotFromPool();
            dot.transform.localPosition = new Vector3(pos.x * _cellSize, pos.y * _cellSize, 0.5f);
            dot.transform.localScale = Vector3.one * _dotInitScale;
            dot.gameObject.SetActive(false);
            _activeDots[pos] = dot; 
        }
    }

    private void CenterAndScaleBoard(LevelData data) {
        if (_playAreaBounds == null) {
            Debug.LogError("[GameplayController] Не назначен _playAreaBounds! Добавь SpriteRenderer на сцену.");
            return;
        }

        // При старте игры отключаем рендер рамки, чтобы игрок ее не видел 
        // (закомментируй эту строку, если хочешь видеть рамку во время игры)
        _playAreaBounds.enabled = false;

        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        // Находим крайние точки уровня
        foreach (var snake in data.Snakes) {
            foreach (var pos in snake.BodyPositions) {
                if (pos.x < minX) minX = pos.x;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y < minY) minY = pos.y;
                if (pos.y > maxY) maxY = pos.y;
            }
        }

        // Защита, если уровень пустой
        if (minX > maxX) {
            minX = 0; maxX = _gridSize.x - 1;
            minY = 0; maxY = _gridSize.y - 1;
        }

        // Фактический размер уровня в мировых координатах (без учета скейла)
        // +1 клетка, так как нам нужны габариты от левого края первой клетки до правого края последней
        float boardUnscaledWidth = ((maxX - minX) + 1) * _cellSize;
        float boardUnscaledHeight = ((maxY - minY) + 1) * _cellSize;

        // Берем мировые размеры нашей целевой рамки-SpriteRenderer минус внутренний отступ
        Bounds targetBounds = _playAreaBounds.bounds;
        float availableWidth = targetBounds.size.x - (_boundsPadding * 2f);
        float availableHeight = targetBounds.size.y - (_boundsPadding * 2f);

        // Вычисляем масштаб, чтобы вписать уровень в рамку
        float scaleX = availableWidth / boardUnscaledWidth;
        float scaleY = availableHeight / boardUnscaledHeight;
        float finalScale = Mathf.Min(scaleX, scaleY);

        // Ограничиваем максимальный масштаб
        finalScale = Mathf.Min(finalScale, _maxBoardScale);

        // Применяем масштаб
        _boardRoot.localScale = new Vector3(finalScale, finalScale, 1f);

        // Находим локальный геометрический центр уровня (середина между крайними клетками)
        float levelLocalCenterX = (minX + maxX) * _cellSize / 2f;
        float levelLocalCenterY = (minY + maxY) * _cellSize / 2f;

        // Чтобы выровнять центр доски по центру нашей рамки, двигаем _boardRoot.
        // Позиция = Центр Рамки Минус (Локальный Центр Уровня * Масштаб)
        Vector3 targetPos = targetBounds.center;
        targetPos.x -= levelLocalCenterX * finalScale;
        targetPos.y -= levelLocalCenterY * finalScale;
        targetPos.z = _boardRoot.position.z; // Сохраняем оригинальную глубину по Z

        _boardRoot.position = targetPos;
    }

    private void HandleClick() {
        Vector2 mouseWorldPos = _cam.ScreenToWorldPoint(Input.mousePosition);
        var hitCollider = Physics2D.OverlapPoint(mouseWorldPos);

        PlayClickParticle(mouseWorldPos);

        if (hitCollider != null) {
            var snake = hitCollider.GetComponentInParent<ISnakeController>();

            if (snake != null && !snake.IsMoving) {
                // Как только кликнули по любой змейке — убираем ВСЕ подсказки (линии)
                ClearAllHints();

                _signalBus.Fire(new OnSnakeClickedSignal());
                TryMoveSnake(snake);
            }
        } else {
            // Если игрок просто кликнул "в молоко" по пустой доске, тоже убираем подсказки
            ClearAllHints();
        }
    }

    private void PlayClickParticle(Vector3 position) {
        if (_clickParticlePrefab == null) return;

        if (_clickParticleInstance == null) {
            _clickParticleInstance = Instantiate(_clickParticlePrefab, _boardRoot);
            _clickParticleInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        gameObject.SetActive(true);
        position.z = _boardRoot.position.z - 2f;
        _clickParticleInstance.transform.position = position;

        _clickParticleInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _clickParticleInstance.Play(true);
    }

    private void TryMoveSnake(ISnakeController snake) {
        if (snake.IsMoving) return;

        var direction = snake.Direction;
        var currentCheckPos = snake.HeadPosition;
        var movePath = new List<Vector2Int>();
        var isBlocked = false;
        var safetyLimit = 100;

        while (safetyLimit-- > 0) {
            currentCheckPos += direction;

            if (!IsInsideGrid(currentCheckPos)) {
                while (IsPositionVisibleOnScreen(currentCheckPos)) {
                    movePath.Add(currentCheckPos);
                    currentCheckPos += direction;
                }

                var tailLength = snake.BodyPositions.Count;
                for (var k = 0; k < tailLength + 1; k++) {
                    movePath.Add(currentCheckPos);
                    currentCheckPos += direction;
                }
                break;
            }

            if (_occupiedCells.Contains(currentCheckPos) && !snake.BodyPositions.Contains(currentCheckPos)) {
                isBlocked = true;
                break;
            }

            movePath.Add(currentCheckPos);
        }

        if (isBlocked) {
            var bumpDistance = (movePath.Count * _cellSize) + (_cellSize * 0.35f);

            _currentLives--;
            _signalBus.Fire(new LivesChangedSignal {
                CurrentLives = _currentLives,
                IsLose = true
            });

            if (_currentLives <= 0) {
                _levelActive = false;
                _signalBus.Fire<GameOverSignal>();
            }

            snake.PlayBumpAnimation(bumpDistance, _cellSize, () => { });
            _soundHelper.PlayBumpSound();
        } else {
            var N = snake.BodyPositions.Count;
            for (var i = 0; i < N; i++) {
                _occupiedCells.Remove(snake.BodyPositions[i]);
            }

            _activeSnakes.Remove(snake);
            _movingSnakesCount++;

            snake.MoveToExit(movePath, _cellSize,
                onComplete: () => {
                    _movingSnakesCount--;
                    ReturnSnakeToPool(snake);
                    CheckWin();
                },
                onTailLeaveCell: (Vector2Int cellPos) => {
                    if (_activeDots.TryGetValue(cellPos, out var dot)) {
                        dot.gameObject.SetActive(true);
                        dot.PLayAppearAnimFromUnderSnake(0f, snake.SnakeColor);
                    }
                }
            );
        }
    }

    private bool IsPositionVisibleOnScreen(Vector2Int gridPos) {
        var worldPos = _boardRoot.TransformPoint(new Vector3(gridPos.x * _cellSize, gridPos.y * _cellSize, 0));
        var viewportPos = _cam.WorldToViewportPoint(worldPos);
        return viewportPos.x > -0.1f && viewportPos.x < 1.1f &&
               viewportPos.y > -0.1f && viewportPos.y < 1.1f;
    }

    private bool IsInsideGrid(Vector2Int pos) {
        return pos.x >= 0 && pos.x < _gridSize.x &&
               pos.y >= 0 && pos.y < _gridSize.y;
    }

    private bool CanSnakeExit(ISnakeController snake) {
        var direction = snake.Direction;
        var currentCheckPos = snake.HeadPosition;
        var safetyLimit = 100;

        while (safetyLimit-- > 0) {
            currentCheckPos += direction;

            if (!IsInsideGrid(currentCheckPos)) return true;

            if (_occupiedCells.Contains(currentCheckPos) && !snake.BodyPositions.Contains(currentCheckPos)) {
                return false;
            }
        }
        return false;
    }

    // ==========================================
    // ЛОГИКА БУСТЕРОВ (ОБНОВЛЕННАЯ АСИНХРОННАЯ)
    // ==========================================

    // Универсальный метод для вызова из UI
    public void TryUseBooster(BoosterId boosterId, Vector3 buttonScreenPos, Action onConsumed) {
        switch (boosterId) {
            case BoosterId.Hint: TryUseHint(buttonScreenPos, onConsumed); break;
            case BoosterId.Hammer: TryUseHammer(buttonScreenPos, onConsumed); break;
            case BoosterId.Ruler: TryUseRuler(buttonScreenPos, onConsumed); break;
        }
    }

    public void TryUseHint(Vector3 buttonScreenPos, Action onConsumed) {
        if (_isBoosterAnimating || !_levelActive) return;

        var validSnakes = _activeSnakes.Where(s => !s.IsMoving && CanSnakeExit(s) && !_hintedSnakesList.Contains(s)).ToList();
        if (validSnakes.Count == 0) return;

        _isBoosterAnimating = true;
        var targetSnake = validSnakes.OrderByDescending(s => s.BodyPositions.Count).First();

        _boosterVisual.PlayHintProjectile(buttonScreenPos, targetSnake.transform.position, () => {
            targetSnake.ShowExitLine();
            _hintedSnakesList.Add(targetSnake);
            _isBoosterAnimating = false;
            onConsumed?.Invoke(); 
        });
    }

    public void TryUseHammer(Vector3 buttonScreenPos, Action onConsumed) {
        if (_isBoosterAnimating || !_levelActive) return;

        var validSnakes = _activeSnakes.Where(s => !s.IsMoving && CanSnakeExit(s)).ToList();
        if (validSnakes.Count == 0) return;

        _isBoosterAnimating = true;
        
        _boosterVisual.PlayHammerSequence(buttonScreenPos, _boardRoot.position, () => {
            _cameraService.ShakeCamera(0.4f, 0.5f);
            
            // Запускаем змеек с маленьким интервалом
            StartCoroutine(LaunchSnakesRoutine(validSnakes.Take(3).ToList()));
            
            _isBoosterAnimating = false;
            onConsumed?.Invoke();
        });
    }

    public void TryUseRuler(Vector3 buttonScreenPos, Action onConsumed) {
        if (_isBoosterAnimating || !_levelActive) return;

        var validSnakes = _activeSnakes.Where(s => !s.IsMoving && CanSnakeExit(s) && !_hintedSnakesList.Contains(s)).ToList();
        if (validSnakes.Count == 0) return;

        _isBoosterAnimating = true;
        int completedAnimations = 0;

        foreach (var snake in validSnakes) {
            _boosterVisual.PlayHintProjectile(buttonScreenPos, snake.transform.position, () => {
                snake.ShowExitLine();
                _hintedSnakesList.Add(snake);

                completedAnimations++;
                if (completedAnimations == validSnakes.Count) {
                    _isBoosterAnimating = false;
                    onConsumed?.Invoke();
                }
            });
        }
    }

    private IEnumerator LaunchSnakesRoutine(List<ISnakeController> snakesToLaunch) {
        foreach (var snake in snakesToLaunch) {
            if (_activeSnakes.Contains(snake)) {
                snake.HideExitLine();
                _hintedSnakesList.Remove(snake);
                TryMoveSnake(snake);
            }
            yield return new WaitForSeconds(0.15f);
        }
    }

    private void ClearAllHints() {
        foreach (var s in _hintedSnakesList) {
            if (s != null && s.gameObject.activeInHierarchy) {
                s.HideExitLine();
            }
        }
        _hintedSnakesList.Clear();
    }

    // ==========================================
    // АВТО-РЕШЕНИЕ (Чит / Дебаг)
    // ==========================================
    
    public void Solve() {
        if (!_levelActive || _activeSnakes.Count <= 1) return;
        StartCoroutine(SolveRoutine());
    }

    private IEnumerator SolveRoutine() {
        _levelActive = false;

        while (_activeSnakes.Count > 1) {
            var snakeToMove = _activeSnakes.FirstOrDefault(s => CanSnakeExit(s));

            if (snakeToMove != null) {
                ClearAllHints();
                TryMoveSnake(snakeToMove);
                yield return new WaitForSeconds(0.05f);
            } else {
                yield return null;
            }
        }
        _levelActive = true;
    }
}