using System;
using System.Collections;
using _ExtensionsHelpers;
using _UI;
using _Zenject;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

// File: Assets/Game/Scripts/UI/Screens/WinScreen.cs
namespace _UI.Screens {
    public class WinScreen : BaseScreen {
        [SerializeField] private Button _continueButton;
        [SerializeField] private TextMeshProUGUI levelTitle_Next;
        [SerializeField] private TextMeshProUGUI levelTitle_Completed;
        [SerializeField] private TextMeshProUGUI levelNumber_Next;
        [SerializeField] private TextMeshProUGUI levelNumber_Completed;
        [SerializeField] private TextMeshProUGUI continueTMP;
        [SerializeField] private TextMeshProUGUI scoreTMP;
        [SerializeField] private RectTransform rocket;
        [SerializeField] private RectTransform ufoTr;
        [SerializeField] private RectTransform energyTr;
        [SerializeField] private Image rocketImage;
        [SerializeField] private Vector2 rocketStartPos;
        [SerializeField] private float rocketIdleMoveShift;
        [SerializeField] private float rocketMoveAwayShift;
        [SerializeField] private Vector2 congratsTextStartPos;

        [Inject] private SignalBus _signalBus;
        [Inject] private PlayerProgressService _playerProgress;
        [Inject] private SoundHelper _soundHelper;

        private bool _isContinueClicked;
        private RectTransform _congratsTMPTr;
        private int _addedScore;


        public void Show(int score, Action onShowFinished = null) {
            _addedScore = score;
            _isContinueClicked = false;
            energyTr.anchoredPosition = energyOriginPos;
            rocket.DOKill();
            rocket.anchoredPosition = rocketStartPos;

            base.Show(onShowFinished);
            // Подписываемся на стандартный клик Unity UI
            _continueButton.onClick.AddListener(OnContinueClicked);

            levelNumber_Completed.text = _playerProgress.CurrentLevelIndex.ToString();
            levelNumber_Next.text = (_playerProgress.CurrentLevelIndex + 1).ToString();

            /*levelTitle_Next.text = Localizer.LevelTitle;
            levelTitle_Completed.text = Localizer.LevelTitle;
            continueTMP.text = Localizer.Continue;*/

            scoreTMP.text = score.ToString(); //todo make anima for score
        }

        [SerializeField] private RectTransform ufoCap;
        [SerializeField] private float ufoCapOpenAngle;
        [SerializeField] private float energyJumpShift;

        protected IEnumerator PlayShowAnim(Action onShowFinished = null) {
            energyTr.gameObject.SetActive(true);
            rocket.gameObject.SetActive(true);
            rocketImage.color = new Color(1, 1, 1, 1);
            //yield return base.PlayShowAnim(onShowFinished);

            //todo ufo fly

            yield return ufoCap.DORotate(Vector3.forward * ufoCapOpenAngle, 0.4f).From(Vector3.zero).SetEase(Ease.OutBack).WaitForCompletion();
            var jumpPos = energyTr.anchoredPosition.y + /*Vector2.up * */energyJumpShift;
            //yield return energyTr.DOJumpAnchorPos(jumpPos, 50, 1, 0.5f).WaitForCompletion();
            _soundHelper.PlayEnergyFromUFO();
            yield return energyTr.DOAnchorPosY(jumpPos, 0.75f).SetEase(Ease.OutBack).WaitForCompletion();
            energyTr.DOAnchorPosY(energyTr.anchoredPosition.y + 15, 0.7f).SetDelay(0.1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

            /*_congratsTMPTr.gameObject.SetActive(true);
            yield return _congratsTMPTr.DOAnchorPosY(_congratsTMPTr.anchoredPosition.y, 1).From(congratsTextStartPos).WaitForCompletion();*/
            //MakeCurveTextAnim();


            rocket.DOAnchorPosX(rocketStartPos.x + rocketIdleMoveShift, 1).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
//        yield return new WaitUntil(() => _isContinueClicked);
        }

        /*
        private void MakeCurveTextAnim() {
            var c = _congratsTMPTr.GetComponent<CurvedText>();
            DOTween.Punch(() => new Vector3(c.curve, c.curve), t => c.curve = t.x, Vector3.right, 2, 5);
            DOVirtual.Float(4, 0, 2, delegate(float v) {
                //c.curve = v;
                c.SetCurve(v);
            }).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutBack);
        }
        */

        [SerializeField] private Vector2 energyOriginPos;

        [FormerlySerializedAs("gameUI")] [SerializeField]
        private GameplayScreen gameplayScreen;

        protected IEnumerator HideAnim(Action onCompleted) {
            var moveAwayDur = 0.5f;
            energyTr.DOKill();

            gameplayScreen.MoveEnergy(_addedScore, energyTr.position, moveAwayDur);
            energyTr.anchoredPosition = energyOriginPos;
            energyTr.gameObject.SetActive(false);


            rocket.DOKill();

            var sequence = DOTween.Sequence();
            sequence.Append(rocket.DOAnchorPosX(rocketMoveAwayShift, moveAwayDur).SetEase(Ease.InSine));
            sequence.Insert(moveAwayDur * 0.5f, rocketImage.DOFade(0, moveAwayDur * 0.5f).SetEase(Ease.Linear));
            yield return sequence.Play().WaitForCompletion();

            /*yield return base.HideAnim(delegate {
                DOVirtual.DelayedCall(1.5f, delegate {
                   // yield return new WaitForSeconds(1.5f);
                    onCompleted?.Invoke();
                });
            });*/
        }

        public override void Hide(Action onCompleted = null) {
            rocket.DOKill();
            rocketImage.DOKill();
            rocketImage.color = new Color(1, 1, 1, 1);
            rocket.anchoredPosition = rocketStartPos;

            base.Hide(onCompleted);
            _continueButton.onClick.RemoveListener(OnContinueClicked);

            //ufoCap.eulerAngles = new Vector3(0, 0, 0);
            ufoCap.DORotate(Vector3.zero, 0.5f);
        }

        private void OnContinueClicked() {
            _isContinueClicked = true;
            // Просто кричим в шину, что кнопка нажата
            _signalBus.Fire<NextLevelClickedSignal>();
        }
    }
}