using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core._RewardPresenter {
    // component for a single reward item UI (icon and count).
    public class RewardItemView : MonoBehaviour {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI countTMP;
        [SerializeField] private CanvasGroup canvasGroup;

        public void Setup(Sprite icon, string countText) {
            iconImage.sprite = icon;
            countTMP.text = countText;
        }

        //-----------------------------------------------------------
        // added a little animation effect for appearance
        private const float moveDur = 1.2f;

        public Tween AnimateAppearanceDefault() {
            var tr = transform;
            var originPos = transform.localPosition;

            gameObject.SetActive(true);
            canvasGroup.alpha = 0;

            //make anim
            canvasGroup.DOFade(1, moveDur * 0.3f);
            countTMP.gameObject.SetActive(true);
            countTMP.DOFade(1, moveDur * 0.3f).From(0).SetDelay(moveDur * 0.7f);
            var tween = tr.DOLocalMoveY(originPos.y, moveDur).From(originPos.y - 100).SetEase(Ease.OutBack);

            return tween;
        }

        //-----------------------------------------------------------
        public Tween AnimateAppearanceChest(Transform chestItemsPos) {
            var tr = transform;
            var originPos = transform.position;

            gameObject.SetActive(true);
            canvasGroup.alpha = 0;
            countTMP.gameObject.SetActive(false);

            //make anim
            canvasGroup.DOFade(1, moveDur * 0.3f);
            tr.position = chestItemsPos.position;
            var tween = tr.DOJump(originPos, 1, 1, moveDur).SetEase(Ease.OutSine).OnComplete(() => { countTMP.gameObject.SetActive(true); });

            return tween;
        }

        public void Hide() => gameObject.SetActive(false);
    }
}