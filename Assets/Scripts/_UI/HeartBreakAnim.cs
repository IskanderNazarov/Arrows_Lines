using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _UI {
    public class HeartBreakAnim : MonoBehaviour {
        [SerializeField] private RectTransform partLeft;
        [SerializeField] private RectTransform partRight;
        [SerializeField] private float partLeftPosY;
        [SerializeField] private float partRightPosY;
        [SerializeField] private float angle;
        [SerializeField] private float shift;
        [SerializeField] private float duration;
        [SerializeField] private float delay;


        public void PlayBreakAnim() {
            gameObject.SetActive(true);
            StartCoroutine(PlayBreakAnimRoutine());
        }

        private IEnumerator PlayBreakAnimRoutine() {
            
            partLeft.DOKill();
            partRight.DOKill();
            partLeft.eulerAngles = Vector3.zero;
            partRight.eulerAngles = Vector3.zero;
            partLeft.anchoredPosition = new Vector2(partLeft.anchoredPosition.x, partLeftPosY);
            partRight.anchoredPosition = new Vector2(partRight.anchoredPosition.x, partRightPosY);
            
            
            var d1 = PlayPartAnim(partLeft, partLeftPosY, -angle);
            var d2 = PlayPartAnim(partRight, partRightPosY, angle);
            
            yield return new WaitForSeconds(Mathf.Max(d1, d2) + 0.5f);
        }

        private float PlayPartAnim(RectTransform tr, float partPos, float rotation) {
            var randDelay = Random.value * 0.1f + 0.1f;
            var leftImage = tr.GetComponent<Image>();
            leftImage.color = new Color(1, 1, 1, 1);

            var seq = DOTween.Sequence();
            seq.Append(tr.DOLocalRotate(Vector3.forward * rotation, duration).SetEase(Ease.InOutSine));
            seq.Insert(duration * 0.2f, tr.DOAnchorPosY(partPos - shift, duration * 0.8f).SetEase(Ease.InOutSine));
            seq.Insert(duration * 0.75f, leftImage.DOFade(0, duration * 0.25f).SetEase(Ease.Linear));
            seq.Play().SetDelay(delay + randDelay);

            return seq.Duration() + seq.Delay();
        }
    }
}