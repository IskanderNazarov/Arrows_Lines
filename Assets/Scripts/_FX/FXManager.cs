using System;
using System.Collections.Generic;
using _Infrastructure;
using Core._Services;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace _game {
    public class FXManager : MonoBehaviour {
        //[SerializeField] private TextMeshPro _addScoreFXSample;
        [SerializeField] private AddResourceFX _addScoreFX;
        [SerializeField] private Transform _defaultAddFxParent;
        [SerializeField] private Material spriteDefault;
        [SerializeField] private Material grayscale;
        [SerializeField] private Sprite coinsIcon;
        [SerializeField] private Sprite gemIcon;
        [SerializeField] private Sprite nectarIcon;
        [SerializeField] private Sprite scoreIcon;

        [SerializeField] private Transform coinsTextTr;
        [SerializeField] private Transform gemsTextTr;

        private static readonly int C1 = Shader.PropertyToID("_C1");
        private static readonly int C0 = Shader.PropertyToID("_C0");

        private CommonResources _commonResources;
        private Dictionary<CurrencyType, Sprite> iconsMap;
        private Dictionary<CurrencyType, Transform> shakeTrMap;


        [Inject]
        private void Constr(CommonResources commonResources) {
            _commonResources = commonResources;
        }

        private void Start() {
            iconsMap = new Dictionary<CurrencyType, Sprite> {
                { CurrencyType.Coins, coinsIcon },
                { CurrencyType.Gems, gemIcon },
                { CurrencyType.Score, scoreIcon }
            };

            shakeTrMap = new Dictionary<CurrencyType, Transform> {
                { CurrencyType.Coins, coinsTextTr },
                { CurrencyType.Gems, gemsTextTr }
            };
        }

        private Sprite GetResourceIcon(CurrencyType CurrencyType) {
            return !iconsMap.ContainsKey(CurrencyType) ? null : iconsMap[CurrencyType];
        }

        private Transform GetTextTrs(CurrencyType CurrencyType) {
            return !shakeTrMap.ContainsKey(CurrencyType) ? null : shakeTrMap[CurrencyType];
        }

        public void ShowAddResourceFX(int addedResource, CurrencyType CurrencyType, Color color_0, Color color_1,
            Transform startPos = null) {
            var fx = Instantiate(_addScoreFX, transform.parent);
            fx.ResourceIcon.material = new Material(grayscale);
            fx.ResourceIcon.material.SetColor(C0, color_0);
            fx.ResourceIcon.material.SetColor(C1, color_1);

            ShowAddResourceFX(addedResource, CurrencyType, fx, startPos);
        }

        public void ShowAddResourceFX(int addedResource, CurrencyType CurrencyType, Transform startPos = null) {
            var fx = Instantiate(_addScoreFX, transform.parent);
            fx.ResourceIcon.material = spriteDefault;

            ShowAddResourceFX(addedResource, CurrencyType, fx, startPos);
        }

        private void ShowAddResourceFX(int addedResource, CurrencyType CurrencyType, AddResourceFX instantiatedGO,
            Transform startPos = null) {
            if (startPos == null) startPos = _defaultAddFxParent;
            _addScoreFX.ResourceIcon.sprite = GetResourceIcon(CurrencyType);

            const float dur = 1.2f;
            const float fadeDelay = dur * 0.2f;
            const float fadeDur = dur - fadeDelay;
            var fx = instantiatedGO;
            var tr = fx.transform;

            fx.ResourceCountTMP.text = $"+{addedResource}";
            fx.ResourceCountTMP.DOFade(0, fadeDur).SetDelay(fadeDelay);
            fx.ResourceIcon.DOFade(0, fadeDur).SetDelay(fadeDelay);

            tr.gameObject.gameObject.SetActive(true);
            tr.position = startPos.position;
            tr.DOLocalMoveY(tr.localPosition.y + 1.5f, dur).SetEase(Ease.OutCubic).OnComplete(delegate {
                tr.DOKill();
                fx.DOKill();
                Destroy(fx.gameObject);
            });
        }

        public void ShowAddResourceScope(int resCount, CurrencyType CurrencyType, Action onCompletedShow = null) {
            ShowAddResourceScope(new[] { resCount }, new[] { CurrencyType });
        }

        public void ShowAddResourceScope(int[] resCount, CurrencyType[] CurrencyTypes, Action onCompletedShow = null) {
            //panel.Show(count, coinsTextTr, onCompletedShow);
            var infos = new ResourceAddingInfo[CurrencyTypes.Length];
            for (var i = 0; i < CurrencyTypes.Length; i++) {
                var info = new ResourceAddingInfo {
                    iconSprite = GetResourceIcon(CurrencyTypes[i]),
                    resourceCount = resCount[i],
                    CurrencyType = CurrencyTypes[i],
                    transformToShake = GetTextTrs(CurrencyTypes[i])
                };
                infos[i] = info;
            }

            var panel = Instantiate(_commonResources.addResourceInfoPanel);
            panel.Show(onCompletedShow, infos);
        }
    }
}

public class ResourceAddingInfo {
    public CurrencyType CurrencyType;
    public int resourceCount;
    public Sprite iconSprite;
    public Transform transformToShake;

    public override string ToString() {
        return base.ToString();
    }
}