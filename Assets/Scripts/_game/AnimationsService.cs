using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _ExtensionsHelpers;
using _UI;
using DG.Tweening;
using UnityEngine;

namespace _game {
    public class AnimationsService {
        private FXCanvas _fxCanvas;
        private SoundHelper _soundHelper;

        private AnimationsService(FXCanvas fxCanvas, SoundHelper soundHelper) {
            _fxCanvas = fxCanvas;
            _soundHelper = soundHelper;
        }

        //from center
        public IEnumerator AnimateDotsAfterLevel(Dictionary<Vector2Int, BoardDot> activeDots) {
            var rightTop = new Vector2(-1000, -1000);
            var leftBottom = new Vector2(1000, 1000);

            foreach (var kv in activeDots) {
                var dot = kv.Value.transform.position;
                if (dot.x > rightTop.x) rightTop.x = dot.x;
                if (dot.x < leftBottom.x) leftBottom.x = dot.x;
                if (dot.y > rightTop.y) rightTop.y = dot.y;
                if (dot.y < leftBottom.y) leftBottom.y = dot.y;
            }

            var center = Vector2.Lerp(rightTop, leftBottom, 0.5f);
            var dots = new Dictionary<Vector2Int, BoardDot>(activeDots);

            var dur = 0.63f;
            var originScale = activeDots.First().Value.transform.localScale.x;
            var punchValue = 0.08f;
            var waitTime = 0f;
            var maxDist_R = Mathf.Max(Vector3.Distance(center, leftBottom), Vector3.Distance(center, rightTop));
            //var maxDist_R = Mathf.Min(Mathf.Abs(center.x - leftBottom.x), Mathf.Abs(center.y - rightTop.y));

            _soundHelper.DotsWaveSound();
            //DOVirtual.DelayedCall(dur, () => { _soundHelper.DotsCollectMoveSound(); });

            foreach (var kv in dots) {
                var L = Vector2.Distance(kv.Value.transform.position, center);
                var t = L / maxDist_R;
                //t = Mathf.Clamp01(t);

                var tr = kv.Value.transform;
                //var delay = kv.Key.y * 0.02f;
                var delay = t * 0.65f;
                /*var seq = DOTween.Sequence();
                seq.Append(tr.DOScale(originScale + punchValue, dur / 2)).SetEase(Ease.OutSine);
                seq.Append(tr.DOScale(originScale, dur / 2));
                seq.AppendCallback(delegate { kv.Value.EnableTrails(true); });
                seq.Append(tr.DOMove(center, dur / 2).SetEase(Ease.InBack));
                seq.AppendCallback(delegate { kv.Value.gameObject.SetActive(false); });
                seq.Play().SetDelay(delay);

                waitTime = Mathf.Max(seq.Duration() + seq.Delay(), waitTime);*/

                var tween = tr.DOScale(Vector3.zero, dur).SetDelay(delay).SetEase(Ease.InBack, 12);
                waitTime = Mathf.Max(tween.Duration() + tween.Delay(), waitTime);
            }

            yield return new WaitForSeconds(waitTime);
            foreach (var kv in dots) {
                kv.Value.gameObject.SetActive(false);
            }

            //now play stars anim particles
            DOVirtual.DelayedCall(0.5f, () => { _soundHelper.DotsCollectMoveSound(); });
            yield return _fxCanvas.PlayStarsCollectParticles();

            //after this lets move them together
            yield return _fxCanvas.AnimateCongratsText(center);
        }
    }
}