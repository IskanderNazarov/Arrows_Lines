using System;
using System.Collections;
using _Services;
using _UI;
using core.ads;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Gameplay._UI {
    public class WinScreen : BaseScreen {
        [Inject] private IAdsService _adsService;
        [Inject] private PlayerProgressService _progressService;

        [Header("Panels")]
        [SerializeField] private GameObject _winScreen;
        [SerializeField] private GameObject _unlockScreen;
        [SerializeField] private GameObject _unlockPanel;

        [Header("Progress Bar")]
        [SerializeField] private ProgressBar _progressBar;

        [Header("Star Containers")]
        [SerializeField] private RectTransform _bucket_1;
        [SerializeField] private RectTransform _bucket_2;
        [SerializeField] private TextMeshProUGUI _bucketStarsText_1;
        [SerializeField] private TextMeshProUGUI _bucketStarsText_2;

        [Header("Bucket Positions")]
        [SerializeField] private RectTransform _bucket_1_pos_1;
        [SerializeField] private RectTransform _bucket_1_pos_2;
        [SerializeField] private RectTransform _bucket_2_pos_2;

        [Header("Coins UI")]
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private Image _coinsImage;
        [SerializeField] private Sprite _largeCoinsSprite;

        [Header("Buttons")]
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _claimX2Button;
        [SerializeField] private Button _claimX5CoinsButton;
        [SerializeField] private Button _unlockContinueButton;

        [Header("Unlock Assets")]
        [SerializeField] private Image _unlockedPlanetImage;
        [SerializeField] private TextMeshProUGUI _unlockedPlanetName;

        // logic completion event for GameManager
        public event Action OnClosed;
        public bool DidUnlock => _updateResult.DidUnlock;

        private int _starsForLevel;
        private int _coinsForLevel;
        private bool _isX2StarsActive;
        private ProgressUpdateResult _updateResult;
        private Action _resumeProgressBarCallback;

        public void Setup(int stars, int coins) {
            _starsForLevel = stars;
            _coinsForLevel = coins;
            _isX2StarsActive = false;

            _bucketStarsText_1.text = stars.ToString();
            _bucketStarsText_2.text = stars.ToString();
            _coinsText.text = coins.ToString();

            _bucket_2.gameObject.SetActive(false);
            _bucket_1.position = _bucket_1_pos_1.position;

            UpdateButtonsVisibility();
        }

        public override void Show(Action onComplete = null) {
            base.Show(onComplete);
            _winScreen.SetActive(true);
            _unlockScreen.SetActive(false);
            _continueButton.interactable = true;

            // subscribe to listeners in code
            _continueButton.onClick.AddListener(OnContinueClicked);
            _claimX2Button.onClick.AddListener(OnClaimX2StarsClicked);
            _claimX5CoinsButton.onClick.AddListener(OnClaimX5CoinsClicked);
            _unlockContinueButton.onClick.AddListener(OnUnlockContinueClicked);
        }

        public override void Hide(Action onComplete = null) {
            base.Hide(onComplete);
            
            // unsubscribe to prevent leaks
            _continueButton.onClick.RemoveListener(OnContinueClicked);
            _claimX2Button.onClick.RemoveListener(OnClaimX2StarsClicked);
            _claimX5CoinsButton.onClick.RemoveListener(OnClaimX5CoinsClicked);
            _unlockContinueButton.onClick.RemoveListener(OnUnlockContinueClicked);
        }

        private void UpdateButtonsVisibility() {
            var anyUnlocked = _progressService.IsAnyPlanetUnlocked;
            _claimX2Button.gameObject.SetActive(anyUnlocked);
            _claimX5CoinsButton.gameObject.SetActive(true);
        }

        private void OnClaimX2StarsClicked() {
            _adsService.ShowRewarded(
                onRewardGranted: () => {
                    _isX2StarsActive = true;
                    _starsForLevel *= 2;
                    _bucketStarsText_1.text = _starsForLevel.ToString();
                    _bucketStarsText_2.text = _starsForLevel.ToString();

                    _bucket_2.gameObject.SetActive(true);
                    _bucket_1.DOMove(_bucket_1_pos_2.position, 0.5f).SetEase(Ease.OutBack);
                    _bucket_2.DOMove(_bucket_2_pos_2.position, 0.5f).SetEase(Ease.OutBack).From(_bucket_1_pos_1.position);
                },
                onAdClosed: null
            );
        }

        private void OnClaimX5CoinsClicked() {
            _adsService.ShowRewarded(
                onRewardGranted: () => {
                    _coinsForLevel *= 5;
                    _coinsText.text = _coinsForLevel.ToString();
                    _coinsImage.sprite = _largeCoinsSprite;
                    _coinsImage.transform.DOScale(1.2f, 0.3f).SetLoops(2, LoopType.Yoyo);
                },
                onAdClosed: null
            );
        }

        private void OnContinueClicked() {
            _continueButton.interactable = false;
            _claimX2Button.interactable = false;

            _updateResult = _progressService.AddStarsAndGetResult(_starsForLevel);
            _progressService.AddCoins(_coinsForLevel);

            StartCoroutine(ProcessWinFlow());
        }

        private IEnumerator ProcessWinFlow() {
            // execute progress bar routine with unlock callback
            yield return _progressBar.AnimateProgressRoutine(_updateResult, (resumeCallback) => {
                _resumeProgressBarCallback = resumeCallback;
                ShowUnlockScreen();
            });

            // if no unlock happened, complete screen immediately
            if (!_updateResult.DidUnlock) {
                OnClosed?.Invoke();
            }
        }

        private void ShowUnlockScreen() {
            _winScreen.SetActive(false);
            _unlockScreen.SetActive(true);

            var reward = _updateResult.UnlockedReward;
                _unlockedPlanetImage.sprite = reward.Sprite;
                _unlockedPlanetName.text = $"\"{reward.DisplayName}\"";

            _unlockPanel.SetActive(true);
            _unlockPanel.transform.localScale = Vector3.zero;
            _unlockPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }

        private void OnUnlockContinueClicked() {
            _unlockScreen.SetActive(false);
            _winScreen.SetActive(true);

            // resume rocket animation
            _resumeProgressBarCallback?.Invoke();

            // notify manager after a delay to allow rocket to return
            DOVirtual.DelayedCall(1.5f, () => OnClosed?.Invoke());
        }
    }
}