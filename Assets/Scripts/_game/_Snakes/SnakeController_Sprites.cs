using System;
using System.Collections;
using System.Collections.Generic;
using _ExtensionsHelpers;
using _ScriptableObjects;
using DG.Tweening;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class SnakeController_Sprites : MonoBehaviour, ISnakeController {
    [Header("Visuals - Dots & Links")] [SerializeField]
    private SpriteRenderer _headSprite;
    [SerializeField] private Transform _headTransform;

    [SerializeField] private SpriteRenderer _segmentPrefab; // ОСТРЫЙ БЕЛЫЙ КВАДРАТ
    [SerializeField] private SpriteRenderer _jointPrefab; // ИДЕАЛЬНЫЙ БЕЛЫЙ КРУГ

    [Header("Movement Settings")] [SerializeField]
    private float _startSpeed = 2f;

    [SerializeField] private float _exitSpeed = 15f;
    [SerializeField] private float _accelerationTime = 0.8f;
    [SerializeField] private float _lineWidth = 0.45f;

    [SerializeField] private Vector3 _fireIdleScale = new Vector3(0.5f, 0.5f, 1f);

    private List<Vector2Int> _logicalPositions;
    private List<Vector3> _visualPoints = new List<Vector3>();
    [Header("Hint Line")] [SerializeField] private LineRenderer _indicatorLine;

    // Пулы для суставов и костей
    private List<SpriteRenderer> _activeSegments = new List<SpriteRenderer>();
    private List<SpriteRenderer> _activeJoints = new List<SpriteRenderer>();

    public Vector2Int HeadPosition => _logicalPositions[0];
    public Vector2Int Direction { get; private set; }
    public IReadOnlyList<Vector2Int> BodyPositions => _logicalPositions;
    public bool IsMoving { get; private set; }
    public Color SnakeColor => _headSprite.color;

    private bool _isBumping = false;
    private Color _bumpColor;
    private Color _tempSnakeColor;

    [Inject] private SnakesGlobalConfig _snakesGlobalConfig;
    [Inject] private SoundHelper _soundHelper;

    public void Initialize(int index, SnakeConfig config, float cellSize) {
        _logicalPositions = new List<Vector2Int>(config.BodyPositions);

        if (_logicalPositions.Count > 1) Direction = _logicalPositions[0] - _logicalPositions[1];
        else Direction = Vector2Int.up;

        _bumpColor = _snakesGlobalConfig.bumpColor;

        if (_snakesGlobalConfig.snakeColors != null && _snakesGlobalConfig.snakeColors.Length > 0) {
            _tempSnakeColor = _snakesGlobalConfig.snakeColors[index % _snakesGlobalConfig.snakeColors.Length];
        } else {
            _tempSnakeColor = Color.white;
        }

        _headSprite.color = _tempSnakeColor;

        // ==========================================
        // ГЕНЕРАЦИЯ ОПТИМИЗИРОВАННЫХ ТОЧЕК
        // ==========================================
        _visualPoints = new List<Vector3>();

        // 1. Всегда добавляем голову
        if (_logicalPositions.Count > 0) {
            _visualPoints.Add((Vector3)(Vector2)_logicalPositions[0] * cellSize);
        }

        // 2. Добавляем промежуточные точки ТОЛЬКО если это угол
        for (int i = 1; i < _logicalPositions.Count - 1; i++) {
            Vector2Int prev = _logicalPositions[i - 1];
            Vector2Int curr = _logicalPositions[i];
            Vector2Int next = _logicalPositions[i + 1];

            if ((curr - prev) != (next - curr)) {
                _visualPoints.Add((Vector3)(Vector2)curr * cellSize);
            }
        }

        // 3. Всегда добавляем кончик хвоста
        if (_logicalPositions.Count > 1) {
            _visualPoints.Add((Vector3)(Vector2)_logicalPositions[_logicalPositions.Count - 1] * cellSize);
        }

        _headTransform.localPosition = _visualPoints[0];
        UpdateSpriteVisuals(_tempSnakeColor);
        RotateHeadImmediate(Direction);
    }

    public void MoveToExit(List<Vector2Int> fullPath, float cellSize, Action onComplete, Action<Vector2Int> onTailLeaveCell = null) {
        IsMoving = true;

        float totalDist = fullPath.Count * cellSize;
        List<Vector3> initialPoints = new List<Vector3>(_visualPoints);
        Vector3 headDir = new Vector3(Direction.x, Direction.y, 0);

        List<Vector2Int> tailPath = new List<Vector2Int>();
        for (int i = _logicalPositions.Count - 1; i >= 0; i--) tailPath.Add(_logicalPositions[i]);
        tailPath.AddRange(fullPath);

        StartCoroutine(MovementRoutine(initialPoints, headDir, totalDist, cellSize, onComplete, onTailLeaveCell, tailPath));
    }

    private IEnumerator MovementRoutine(List<Vector3> initialPoints, Vector3 headDir, float totalDist, float cellSize, Action onComplete,
        Action<Vector2Int> onTailLeaveCell, List<Vector2Int> tailPath) {
        bool isAnticipating = true;
        float pullBackDist = cellSize * 0.4f;
        float pullBackDuration = 0.32f;

        //a little anticipating. Move back before start
        /*yield return DOVirtual.Float(0f, -pullBackDist, pullBackDuration, dist => {
            CalculateVisualPoints(initialPoints, headDir, dist, cellSize);
            _headSprite.transform.localPosition = _visualPoints[0];
            UpdateSpriteVisuals(_tempSnakeColor);
        }).SetEase(Ease.OutSine).OnComplete(() => { isAnticipating = false; }).WaitForCompletion();*/

        float currentDist = -pullBackDist;
        float timeMoving = 0f;
        int nextTailCellIndex = 0;

        _soundHelper.PlayRocketMove(Random.Range(0.7f, 1.2f));

        while (currentDist < totalDist) {
            timeMoving += Time.deltaTime;

            float currentSpeed;
            if (timeMoving < _accelerationTime) {
                var t = timeMoving / _accelerationTime;
                currentSpeed = Mathf.Lerp(_startSpeed, _exitSpeed, t * t * t);
            } else {
                currentSpeed = _exitSpeed;
            }

            currentDist += currentSpeed * Time.deltaTime;
            if (currentDist > totalDist) currentDist = totalDist;

            if (currentDist > 0) {
                while (nextTailCellIndex < tailPath.Count && currentDist >= nextTailCellIndex * cellSize + cellSize * 0.7f) {
                    onTailLeaveCell?.Invoke(tailPath[nextTailCellIndex]);
                    nextTailCellIndex++;
                }
            }

            CalculateVisualPoints(initialPoints, headDir, currentDist, cellSize);
            _headTransform.localPosition = _visualPoints[0];
            UpdateSpriteVisuals(_tempSnakeColor);

            yield return null;
        }

        IsMoving = false;
        onComplete?.Invoke();
    }

    private void CalculateVisualPoints(List<Vector3> initialPoints, Vector3 headDir, float dist, float cellSize) {
        _visualPoints.Clear();

        Vector3 currentHead = initialPoints[0] + headDir * dist;
        _visualPoints.Add(currentHead);

        float tailDistRemaining = dist;
        bool tailFound = false;

        for (int i = initialPoints.Count - 1; i > 0; i--) {
            float segLen = Vector3.Distance(initialPoints[i], initialPoints[i - 1]);

            if (tailDistRemaining >= segLen) {
                tailDistRemaining -= segLen;
            } else {
                Vector3 dir = (initialPoints[i - 1] - initialPoints[i]).normalized;
                Vector3 currentTail = initialPoints[i] + dir * tailDistRemaining;

                for (int j = 1; j <= i - 1; j++) _visualPoints.Add(initialPoints[j]);
                _visualPoints.Add(currentTail);
                tailFound = true;
                break;
            }
        }

        if (!tailFound) _visualPoints.Add(initialPoints[0] + headDir * tailDistRemaining);
    }

    // ==========================================
    // СИСТЕМА DOTS & LINKS (СУСТАВЫ И КОСТИ)
    // ==========================================
    private void UpdateSpriteVisuals(Color color) {
        int neededSegments = _visualPoints.Count - 1;
        int neededJoints = _visualPoints.Count;

        if (neededSegments < 0) neededSegments = 0;

        // Пополняем пулы
        while (_activeSegments.Count < neededSegments) {
            var seg = Instantiate(_segmentPrefab, transform);
            seg.drawMode = SpriteDrawMode.Simple; // Гарантируем обычный режим
            _activeSegments.Add(seg);
        }

        while (_activeJoints.Count < neededJoints) {
            var joint = Instantiate(_jointPrefab, transform);
            joint.drawMode = SpriteDrawMode.Simple;
            _activeJoints.Add(joint);
        }

        // Включаем нужные и красим
        for (int i = 0; i < _activeSegments.Count; i++) {
            _activeSegments[i].gameObject.SetActive(i < neededSegments);
            if (i < neededSegments) _activeSegments[i].color = color;
        }

        for (int i = 0; i < _activeJoints.Count; i++) {
            _activeJoints[i].gameObject.SetActive(i < neededJoints);
            if (i < neededJoints) _activeJoints[i].color = color;
        }

        // Расставляем суставы (Круги)
        for (int i = 0; i < neededJoints; i++) {
            _activeJoints[i].transform.localPosition = _visualPoints[i];
            _activeJoints[i].transform.localScale = new Vector3(_lineWidth, _lineWidth, 1f);
        }

        // Расставляем кости (Квадраты) от центра до центра
        for (int i = 0; i < neededSegments; i++) {
            Vector3 start = _visualPoints[i];
            Vector3 end = _visualPoints[i + 1];
            Vector3 dir = end - start;
            float dist = dir.magnitude;

            if (dist < 0.001f) {
                _activeSegments[i].gameObject.SetActive(false);
                continue;
            }

            Vector3 center = start + dir * 0.5f;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            _activeSegments[i].transform.localPosition = center;
            _activeSegments[i].transform.localRotation = Quaternion.Euler(0, 0, angle);

            // Идеально растягиваем квадрат
            _activeSegments[i].transform.localScale = new Vector3(dist, _lineWidth, 1f);
        }

        // Убеждаемся, что стрелка головы рисуется поверх всех суставов
        _headTransform.SetAsLastSibling();
    }

    private void RotateHeadImmediate(Vector2Int dir) {
        float angle = dir == Vector2Int.up ? 0 : dir == Vector2Int.down ? 180 : dir == Vector2Int.left ? 90 : -90;
        _headTransform.localRotation = Quaternion.Euler(0, 0, angle);
    }

    public void PlayBumpAnimation(float bumpDistance, float cellSize, Action onComplete) {
        if (_isBumping || IsMoving) return;
        _isBumping = true;

        List<Vector3> initialPoints = new List<Vector3>(_visualPoints);
        Vector3 headDir = new Vector3(Direction.x, Direction.y, 0f);

        Color originalBodyColor = _tempSnakeColor;
        Color originalHeadColor = _headSprite.color;

        float duration = 0.15f + (bumpDistance / cellSize) * 0.04f;

        Sequence seq = DOTween.Sequence();

        seq.Append(DOVirtual.Float(0f, bumpDistance, duration, dist => {
            CalculateVisualPoints(initialPoints, headDir, dist, cellSize);
            _headTransform.localPosition = _visualPoints[0];
            float t = dist / bumpDistance;
            _tempSnakeColor = Color.Lerp(originalBodyColor, _bumpColor, t);
            _headSprite.color = Color.Lerp(originalHeadColor, _bumpColor, t);
            UpdateSpriteVisuals(_tempSnakeColor);
        }).SetEase(Ease.InSine));

        seq.Append(DOVirtual.Float(bumpDistance, 0f, duration, dist => {
            CalculateVisualPoints(initialPoints, headDir, dist, cellSize);
            _headTransform.localPosition = _visualPoints[0];
            float t = dist / bumpDistance;
            _tempSnakeColor = Color.Lerp(originalBodyColor, _bumpColor, t);
            _headSprite.color = Color.Lerp(originalHeadColor, _bumpColor, t);
            UpdateSpriteVisuals(_tempSnakeColor);
        }).SetEase(Ease.OutSine));

        seq.OnComplete(() => {
            _isBumping = false;
            _visualPoints = new List<Vector3>(initialPoints);
            _headTransform.localPosition = _visualPoints[0];
            _tempSnakeColor = originalBodyColor;
            _headSprite.color = originalHeadColor;
            UpdateSpriteVisuals(_tempSnakeColor);
            onComplete?.Invoke();
        });
    }

    public void ShowExitLine() {
        if (_indicatorLine == null) return;

        _indicatorLine.gameObject.SetActive(true);

        // Рисуем линию от головы далеко вперед (например, на 30 клеток)
        Vector3 startPos = _visualPoints[0];
        Vector3 endPos = startPos + new Vector3(Direction.x, Direction.y, 0) * 30f;

        _indicatorLine.positionCount = 2;
        _indicatorLine.SetPosition(0, startPos);
        _indicatorLine.SetPosition(1, endPos);

        // Анимация: линия быстро вытягивается
        float currentDist = 0f;
        DOTween.To(() => currentDist, x => {
            currentDist = x;
            _indicatorLine.SetPosition(1, Vector3.Lerp(startPos, endPos, currentDist));
        }, 1f, 0.2f).SetEase(Ease.OutSine);
    }

    public void HideExitLine() {
        if (_indicatorLine != null) {
            _indicatorLine.gameObject.SetActive(false);
        }
    }

    public void PlayAppearAnimation(AppearAnimType type, float duration, int i, Action onComplete) => onComplete?.Invoke();
}