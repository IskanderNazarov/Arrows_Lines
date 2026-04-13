using System;
using System.Collections;
using System.Collections.Generic;
using _Infrastructure;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Core._RewardPresenter {
    // handles the visual representation of the reward sequence.
    public class RewardSequencePanel : MonoBehaviour {
        [Header("Root Panels")] [SerializeField]
        private GameObject mainRoot;

        [SerializeField] private TextMeshProUGUI rewardTitleTMP;
        [SerializeField] private TextMeshProUGUI extraRewardTitleTMP;
        [SerializeField] private GameObject chestPanelRoot;
        [SerializeField] private GameObject rewardsPanelRoot;

        [Header("Chest Components")] [SerializeField]
        private Image closedChestImage;

        [SerializeField] private Image openChestImage;
        [SerializeField] private TextMeshProUGUI chestTitleTMP;
        [SerializeField] private CanvasGroup extraBoosterPanel;
        [SerializeField] private RewardItemView extraRewItem;

        [Header("Reward Components")] [SerializeField]
        private List<RewardItemView> rewardItemViews;

        [SerializeField] private LayoutGroup itemsLayout;
        [SerializeField] private float rewardAppearanceDelay = 0.2f;
        [SerializeField] private Button tapCatcher;

        public event Action OnScreenTapped;

        private CommonResources _commonAssetsContainer;
        private Tween _chestIdleAnimation;
        private Vector3 _chestClosedOriginPos;


        [Inject]
        private void Constr(CommonResources commonAssetsContainer) {
            _commonAssetsContainer = commonAssetsContainer;
        }

        private void Awake() {
            tapCatcher.onClick.AddListener(() => OnScreenTapped?.Invoke());
            mainRoot.SetActive(false);

            _chestClosedOriginPos = openChestImage.transform.localPosition;
            rewardTitleTMP.text =      Localizer.YourReward;
            extraRewardTitleTMP.text = Localizer.ExtraReward;
            chestTitleTMP.text =       Localizer.ChestRewardTitle;
        }

        public void Initialize() {
            mainRoot.SetActive(true);
            chestPanelRoot.SetActive(false);
            rewardsPanelRoot.SetActive(false);
            chestTitleTMP.gameObject.SetActive(true);
            extraBoosterPanel.gameObject.SetActive(false);
            foreach (var item in rewardItemViews) {
                item.Hide();
            }
        }

        public IEnumerator AnimateChestAppearance() {
            chestPanelRoot.SetActive(true);
            closedChestImage.gameObject.SetActive(true);
            openChestImage.gameObject.SetActive(false);

            // your custom appearance effect
            var tr = closedChestImage.transform;
            tr.localScale = Vector3.zero;
            _chestIdleAnimation?.Kill();

            yield return tr.DOScale(1f, 0.5f).SetEase(Ease.OutBack).WaitForCompletion();

            // start idle animation after appearing
            _chestIdleAnimation?.Kill();
            _chestIdleAnimation = tr.DOLocalMoveY(tr.localPosition.y + 20f, 1f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        public IEnumerator AnimateChestOpening() {
            _chestIdleAnimation?.Kill();
            //todo jump closed chest and then open it

            closedChestImage.transform.localPosition = _chestClosedOriginPos;
            var downPos = _chestClosedOriginPos - Vector3.up * 90f;
            //var jumpPos = _chestOpenedOriginPos + Vector3.up * 30f;
            yield return closedChestImage.transform.DOLocalMove(downPos, 0.5f).SetEase(Ease.OutSine).WaitForCompletion();
            yield return closedChestImage.transform.DOLocalJump(_chestClosedOriginPos, 100, 1, 0.7f).SetEase(Ease.InSine).WaitForCompletion();

            closedChestImage.gameObject.SetActive(false);
            chestTitleTMP.gameObject.SetActive(false);
            openChestImage.gameObject.SetActive(true);
        }


        public IEnumerator AnimateRewardAppearance(Reward reward) {
            rewardsPanelRoot.SetActive(true);
            var viewIndex = 0;

            var itemsToShow = new List<RewardItemView>();
            if (reward.Gems > 0 && viewIndex < rewardItemViews.Count) {
                itemsToShow.Add(rewardItemViews[viewIndex]);

                SetupCoinIcon(rewardItemViews[viewIndex], reward.Gems);
                viewIndex++;
            }

            /*if (reward.Boosters != null) {
                foreach (var br in reward.Boosters) {
                    if (viewIndex >= rewardItemViews.Count) break;
                    var item = rewardItemViews[viewIndex];
                    itemsToShow.Add(item);

                    var boosterIcon = _commonAssetsContainer.GetBoosterIcon(br.BoosterId);
                    item.Setup(boosterIcon, $"x{br.Amount}");
                    viewIndex++;
                }
            }*/

            //todo make items appearance depend on chest active or not
            yield return ShowItemsAppearanceDefault(itemsToShow, false/*reward.IsChest*/);
        }

        private void SetupCoinIcon(RewardItemView coinsItem, int amount) {
            var coinIcon = _commonAssetsContainer.gemIcon;
            coinsItem.Setup(coinIcon, $"+{amount}");
        }

        private IEnumerator ShowItemsAppearanceDefault(List<RewardItemView> itemsToShow, bool isFromChest) {
            rewardTitleTMP.DOFade(1, 1f).From(0);

            //first make the layout work and then disable it
            foreach (var item in itemsToShow) item.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemsLayout.GetComponent<RectTransform>());
            itemsLayout.enabled = false;
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemsLayout.GetComponent<RectTransform>());
            foreach (var item in itemsToShow) item.gameObject.SetActive(false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemsLayout.GetComponent<RectTransform>());


            Tween lastTween = null;
            foreach (var item in itemsToShow) {
                lastTween = isFromChest ? item.AnimateAppearanceChest(chestPanelRoot.transform) : item.AnimateAppearanceDefault();
                yield return new WaitForSeconds(0.3f);
            }

            if (lastTween != null) yield return lastTween.WaitForCompletion();
            itemsLayout.enabled = true;
            yield return null;
        }

        public void Hide() {
            mainRoot.SetActive(false);
        }

        public void Run(IEnumerator coroutine) {
            StartCoroutine(coroutine);
        }

        public IEnumerator ShowExtraBoostersConversion(int coinsAmount) {
            extraBoosterPanel.gameObject.SetActive(true);
            SetupCoinIcon(extraRewItem, coinsAmount);
            yield return extraBoosterPanel.DOFade(1, 0.7f).From(0).WaitForCompletion();
        }
    }
}