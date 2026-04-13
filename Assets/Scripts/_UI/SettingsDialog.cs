using System.Collections;
using Core._Services;
using Core._Services._Saving;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _UI {
    public class SettingsDialog : Dialog {
        [SerializeField] private Button musicBtn;
        [SerializeField] private Button soundBtn;
        [SerializeField] private TextMeshProUGUI musicTMP;
        [SerializeField] private TextMeshProUGUI soundTMP;
        [SerializeField] private Image soundTop;
        [SerializeField] private Image musicTop;
        [SerializeField] private RectTransform soundGrayTop;
        [SerializeField] private RectTransform musicGrayTop;
        [SerializeField] private float musicTopLeftPos;
        [SerializeField] private float musicTopRightPos;
        [SerializeField] private float soundTopLeftPos;
        [SerializeField] private float soundTopRightPos;


        [Inject] private SoundManager _soundManager;
        [Inject] private IDataSaver _dataSaver;
        private RectTransform _rectTransform;

        private void Start() {
            soundBtn.onClick.AddListener(OnSoundClicked);
            musicBtn.onClick.AddListener(OnMusicClicked);

            // musicTMP.text = Localizer.musicTitle;
            // soundTMP.text = Localizer.soundTitle;
        }

        private void OnEnable() {
            Debug.Log($"DS_ _soundManager.IsMusicOn(): {_soundManager.IsMusicOn}");
            Debug.Log($"DS_ _soundManager.IsSoundOn(): {_soundManager.IsSoundOn}");
            musicGrayTop.anchoredPosition =
                new Vector2(_soundManager.IsMusicOn ? musicTopRightPos : musicTopLeftPos, musicGrayTop.anchoredPosition.y);
            soundGrayTop.anchoredPosition =
                new Vector2(_soundManager.IsSoundOn ? soundTopRightPos : soundTopLeftPos, soundGrayTop.anchoredPosition.y);

            musicTop.color = new Color(1, 1, 1, _soundManager.IsMusicOn ? 1 : 0);
            soundTop.color = new Color(1, 1, 1, _soundManager.IsSoundOn ? 1 : 0);
        }

        private void OnDestroy() {
            soundBtn.onClick.RemoveListener(OnSoundClicked);
            musicBtn.onClick.RemoveListener(OnMusicClicked);
        }

        private void OnMusicClicked() {
            var isMusicOn = _soundManager.IsMusicOn;
            _soundManager.SetMusicOn(!isMusicOn);
            MoveSettingsBtn(musicTop, musicGrayTop, isMusicOn ? musicTopLeftPos : musicTopRightPos, isMusicOn ? 0 : 1);
        }

        private void OnSoundClicked() {
            var isSfxOn = _soundManager.IsSoundOn;
            _soundManager.SetSFXOn(!isSfxOn);
            MoveSettingsBtn(soundTop, soundGrayTop, isSfxOn ? soundTopLeftPos : soundTopRightPos, isSfxOn ? 0 : 1);
        }

        public void MoveSettingsBtn(Image btn, RectTransform tr, float targetPos, float targetFade) {
            btn.DOKill();
            tr.DOKill();

            tr.DOAnchorPosX(targetPos, 0.5f).SetEase(Ease.InOutSine);
            btn.DOFade(targetFade, 0.5f).SetEase(Ease.InOutSine);
        }

        public void DeleteAll() {
            //_dataSaver.DeleteAll();
        }

        [SerializeField] private GameObject debugPanel;

        private int counter;

        public void EnableSolveButton() {
            counter++;
#if UNITY_EDITOR
            debugPanel.gameObject.SetActive(counter % 2 == 0);
#else
            debugPanel.gameObject.SetActive(counter % 10 == 0);
#endif
        }
    }
}