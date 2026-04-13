using System;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class BoosterVisualService : MonoBehaviour {
    [Header("Hint FX")]
    [SerializeField] private ParticleSystem _hintTrailPrefab;
    [SerializeField] private ParticleSystem _hintExplosionPrefab;

    [Header("Hammer FX")]
    [SerializeField] private Transform _hammerPrefab;
    [SerializeField] private ParticleSystem _hammerImpactPrefab;

    private Camera _cam;

    [Inject]
    private void Construct() {
        _cam = Camera.main;
    }

    // 1. Анимация полета "Искры" от кнопки к голове змейки (Hint / Ruler)
    public void PlayHintProjectile(Vector3 buttonScreenPos, Vector3 targetWorldPos, Action onArrived) {
        Vector3 startWorldPos = _cam.ScreenToWorldPoint(new Vector3(buttonScreenPos.x, buttonScreenPos.y, 10f));
        
        var trail = Instantiate(_hintTrailPrefab, startWorldPos, Quaternion.identity, transform);
        
        // Летим по дуге для красоты
        trail.transform.DOJump(targetWorldPos, jumpPower: 2f, numJumps: 1, duration: 0.4f)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => {
                // Взрыв по прибытии
                if (_hintExplosionPrefab != null) {
                    var explosion = Instantiate(_hintExplosionPrefab, targetWorldPos, Quaternion.identity);
                    Destroy(explosion.gameObject, 1.5f);
                }
                
                Destroy(trail.gameObject);
                onArrived?.Invoke(); // Говорим контроллеру: "Включай линию!"
            });
    }

    // 2. Анимация удара Молотком
    public void PlayHammerSequence(Vector3 buttonScreenPos, Vector3 targetWorldPos, Action onImpact) {
        Vector3 startWorldPos = _cam.ScreenToWorldPoint(new Vector3(buttonScreenPos.x, buttonScreenPos.y, 10f));
        
        var hammer = Instantiate(_hammerPrefab, startWorldPos, Quaternion.identity, transform);
        hammer.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();
        
        // Молоток появляется и летит к центру доски
        seq.Append(hammer.DOScale(Vector3.one * 1.5f, 0.3f));
        seq.Join(hammer.DOJump(targetWorldPos + Vector3.up * 2f, 3f, 1, 0.5f));
        
        // Замах и удар
        seq.Append(hammer.DORotate(new Vector3(0, 0, -45f), 0.2f).SetEase(Ease.OutQuad)); // Замах
        seq.Append(hammer.DORotate(new Vector3(0, 0, 45f), 0.1f).SetEase(Ease.InFlash));  // Удар вниз
        seq.Append(hammer.DOMoveY(targetWorldPos.y, 0.1f).SetEase(Ease.InFlash));         // Бьем в точку
        
        seq.OnComplete(() => {
            if (_hammerImpactPrefab != null) {
                var impact = Instantiate(_hammerImpactPrefab, targetWorldPos, Quaternion.identity);
                Destroy(impact.gameObject, 2f);
            }
            
            // Заставляем молоток исчезнуть
            hammer.DOScale(Vector3.zero, 0.2f).OnComplete(() => Destroy(hammer.gameObject));
            
            onImpact?.Invoke(); // Говорим контроллеру: "Тряси камеру и запускай змеек!"
        });
    }
}