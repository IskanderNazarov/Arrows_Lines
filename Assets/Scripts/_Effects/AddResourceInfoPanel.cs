using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Effects {
    public class AddResourceInfoPanel : MonoBehaviour {
        [SerializeField] private TextMeshPro[] resourceCountTMP;
        [SerializeField] private SpriteRenderer[] icon;
        [SerializeField] private SpriteRenderer darkBg;

        private float _bgAlpha = 0.7f;
        private bool _isClicked;

        /*public void Show(int count, Transform trToShake, Action onCompletedShow = null) {
        }*/

        public void Show(Action onCompletedShow = null, params ResourceAddingInfo[] info) {
            StartCoroutine(ShowScreen(info, onCompletedShow));
        }

        /*public void Show(int count, Transform trToShake, Action onCompletedShow = null) {
            StartCoroutine(ShowScreen(count, trToShake, onCompletedShow));
        }*/

        private IEnumerator ShowScreen(ResourceAddingInfo[] infoArr, Action onCompletedShow = null) {
            darkBg.color = new Color(0, 0, 0, 0);
            yield return new WaitForSeconds(0.3f);
            darkBg.DOFade(_bgAlpha, 0.4f).From(0).SetEase(Ease.Linear);
            
            transform.position = new Vector3(0, 0, -25);

            for (var i = 0; i < infoArr.Length; i++) {
                var info = infoArr[i];
                var resourceIndex = (int) info.CurrencyType;
                var curIcon = icon[resourceIndex];
                var curTMP = resourceCountTMP[resourceIndex];
                var originPos = curIcon.transform.position;


                Debug.Log($"icon[i]: {icon[i]}");
                
                
                curIcon.sprite = info.iconSprite;
                curTMP.gameObject.SetActive(false);
                curTMP.text = $"+{info.resourceCount}";

                Debug.Log($"info: {info}");
                var showDur = 0.75f;
                curIcon.gameObject.SetActive(true);
                curIcon.DOFade(1, showDur * 0.5f).SetEase(Ease.Linear);
                yield return curIcon.transform.DOMoveY(originPos.y, showDur).From(originPos.y - 3.5f).SetEase(Ease.OutBack).WaitForCompletion();
                curTMP.gameObject.SetActive(true);

                yield return new WaitForSeconds(0.3f * i);
            }

            yield return new WaitUntil(() => _isClicked);

            foreach (var info in infoArr) {
                var startScale = info.transformToShake.localScale;
                info.transformToShake.DOPunchScale(startScale * 0.2f, 1, 5);
            }

            onCompletedShow?.Invoke();

            //gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private void OnMouseDown() {
            _isClicked = true;
        }
    }
}