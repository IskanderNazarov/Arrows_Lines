using System;
using UnityEngine;
using _ExtensionsHelpers;
using DG.Tweening;
using UnityEngine.UI;
using Zenject;

public class CameraController : MonoBehaviour, ICameraService {
    [SerializeField] private Slider _zoomSlider;

    [Header("Zoom Settings")] [SerializeField]
    private float _minOrthoSize = 3f;

    [SerializeField] private float _zoomSpeedMouse = 1.5f;
    [SerializeField] private float _zoomSpeedTouch = 0.01f;

    [Header("Pan Settings")] [SerializeField]
    private float _dragThreshold = 15f;

    public event Action<float> OnZoomChanged;

    private Camera _cam;
    private float _maxOrthoSize;
    private Vector3 _baseCamPos;

    private bool _canPan;
    private bool _isDragging;
    private Vector3 _dragStartScreen;
    private Vector3 _lastPanScreenPos;
    private int _lastScreenWidth;
    private int _lastScreenHeight;
    
    [Inject] private SignalBus _signalBus;

    private void Awake() {
        _cam = GetComponent<Camera>();
        _maxOrthoSize = _cam.orthographicSize;
        _baseCamPos = transform.position;
    }

    private void Start() {
        if (_zoomSlider != null) {
            _zoomSlider.onValueChanged.AddListener(ZooChanged);
        }
        
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
    }
    
    public void ShakeCamera(float duration, float strength) {
        _cam.transform.DOKill();
        _cam.transform.position = _baseCamPos;
        // Эффект тряски: параметры можно вынести в конфиг
        _cam.transform.DOShakePosition(duration, strength, vibrato: 10, randomness: 90)
            .OnComplete(ResetCamera);
    }

    // --- ИЗМЕНЕНИЕ 2: Метод сброса для старта/рестарта ---
    public void ResetCamera() {
        _cam.transform.DOKill();
        _cam.orthographicSize = _maxOrthoSize;
        transform.position = _baseCamPos;

        if (_zoomSlider != null) {
            // SetValueWithoutNotify меняет UI, но не вызывает событие (нет зацикливания)
            _zoomSlider.SetValueWithoutNotify(0f);
        }
    }

    private void OnDestroy() {
        _zoomSlider.onValueChanged.RemoveListener(ZooChanged);
    }


    private void ZooChanged(float value) {
        SetZoomFromSlider(value);
    }


    private void Update() {
        HandleZoom();
        HandlePan();

        // --- МОНИТОРИНГ РАЗМЕРА ЭКРАНА ---
        if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight) {
            Debug.Log($"Screen.width: {Screen.width}, Screen.height: {Screen.height}");
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;

            float currentAspect = (float)_lastScreenWidth / _lastScreenHeight;
            _signalBus.Fire(new ScreenSizeChangedSignal { AspectRatio = currentAspect });
        }
    }

    private void HandleZoom() {
        // --- ДЕСКТОП: Колесико мыши ---
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f) {
            float targetSize = _cam.orthographicSize - (scroll * _zoomSpeedMouse);
            // Зумим в точку, где сейчас находится курсор
            ApplyZoom(targetSize, Input.mousePosition);
        }

        // --- МОБАЙЛ: Щипок (Pinch) двумя пальцами ---
        if (Input.touchCount == 2) {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            Vector2 t1Prev = t1.position - t1.deltaPosition;
            Vector2 t2Prev = t2.position - t2.deltaPosition;

            float prevDist = (t1Prev - t2Prev).magnitude;
            float currentDist = (t1.position - t2.position).magnitude;
            float diff = currentDist - prevDist;

            float targetSize = _cam.orthographicSize - (diff * _zoomSpeedTouch);

            // Зумим в центр между двумя пальцами
            Vector2 pinchCenter = (t1.position + t2.position) / 2f;
            ApplyZoom(targetSize, pinchCenter);
        }
    }

    private void HandlePan() {
        // Блокируем панорамирование, если зумим двумя пальцами
        if (Input.touchCount >= 2) {
            _canPan = false;
            _isDragging = false;
            return;
        }

        if (Input.GetMouseButtonDown(0)) {
            if (!Utils.IsPointerOverUIRaycastTarget()) {
                _dragStartScreen = Input.mousePosition;
                _lastPanScreenPos = Input.mousePosition;
                _isDragging = false;
                _canPan = true;
            } else {
                _canPan = false;
            }
        }

        if (Input.GetMouseButton(0) && _canPan) {
            if (!_isDragging && Vector3.Distance(_dragStartScreen, Input.mousePosition) > _dragThreshold) {
                _isDragging = true;
                _lastPanScreenPos = Input.mousePosition;
            }

            if (_isDragging) {
                Vector3 lastWorld = _cam.ScreenToWorldPoint(new Vector3(_lastPanScreenPos.x, _lastPanScreenPos.y, _cam.nearClipPlane));
                Vector3 currentWorld = _cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _cam.nearClipPlane));

                Vector3 delta = lastWorld - currentWorld;

                transform.position = ClampCameraPosition(transform.position + delta);
                _lastPanScreenPos = Input.mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0)) {
            _canPan = false;
            _isDragging = false;
        }
    }

    // ==========================================
    // ЛОГИКА ОГРАНИЧЕНИЙ И ЗУМА К ТОЧКЕ
    // ==========================================

    private void ApplyZoom(float targetSize, Vector2 zoomCenterScreen) {
        targetSize = Mathf.Clamp(targetSize, _minOrthoSize, _maxOrthoSize);
        if (Mathf.Approximately(_cam.orthographicSize, targetSize)) return;

        // 1. Запоминаем мировую позицию под курсором ДО зума
        Vector3 worldPosBeforeZoom = _cam.ScreenToWorldPoint(new Vector3(zoomCenterScreen.x, zoomCenterScreen.y, _cam.nearClipPlane));

        // 2. Применяем зум
        _cam.orthographicSize = targetSize;

        // 3. Смотрим, куда уехала эта точка ПОСЛЕ изменения размера камеры
        Vector3 worldPosAfterZoom = _cam.ScreenToWorldPoint(new Vector3(zoomCenterScreen.x, zoomCenterScreen.y, _cam.nearClipPlane));

        // 4. Сдвигаем камеру на разницу, чтобы курсор остался над той же точкой мира
        transform.position += (worldPosBeforeZoom - worldPosAfterZoom);

        // 5. Зажимаем камеру в границах (если зум к краю пытается выкинуть камеру за пределы доски)
        transform.position = ClampCameraPosition(transform.position);

        // --- ИЗМЕНЕНИЕ 3: Синхронизируем UI ---
        float normalizedZoom = 1f - ((_cam.orthographicSize - _minOrthoSize) / (_maxOrthoSize - _minOrthoSize));

        if (_zoomSlider != null) {
            _zoomSlider.SetValueWithoutNotify(normalizedZoom);
        }

        OnZoomChanged?.Invoke(normalizedZoom);
    }

    private Vector3 ClampCameraPosition(Vector3 targetPos) {
        float zoomDelta = _maxOrthoSize - _cam.orthographicSize;

        float maxX = zoomDelta * _cam.aspect;
        float maxY = zoomDelta;

        float clampedX = Mathf.Clamp(targetPos.x, _baseCamPos.x - maxX, _baseCamPos.x + maxX);
        float clampedY = Mathf.Clamp(targetPos.y, _baseCamPos.y - maxY, _baseCamPos.y + maxY);

        return new Vector3(clampedX, clampedY, targetPos.z);
    }

    // --- ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ КНОПОК И ПОЛЗУНКА ---
    // Для кнопок UI логичнее всего зумить просто в центр экрана

    public void ZoomInDiscrete() {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        ApplyZoom(_cam.orthographicSize - 1f, screenCenter);
    }

    public void ZoomOutDiscrete() {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        ApplyZoom(_cam.orthographicSize + 1f, screenCenter);
    }

    public void SetZoomFromSlider(float value) {
        float targetOrtho = Mathf.Lerp(_maxOrthoSize, _minOrthoSize, value);
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        ApplyZoom(targetOrtho, screenCenter);
    }
}