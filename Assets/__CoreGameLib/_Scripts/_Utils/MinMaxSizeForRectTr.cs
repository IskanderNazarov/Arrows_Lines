using System;
using UnityEngine;

public class MinMaxSizeForRectTr : MonoBehaviour {
    [SerializeField] private RectTransform canvas;
    [SerializeField] private float maxWidth;
    [SerializeField] private float minWidth;

    private float curWidth;
    private bool isInit;
    private RectTransform tr;

    private void Start() {
        if (!isInit) Init();
        Recalculate();
    }

    private void Update() {
        var dif = Math.Abs(curWidth - tr.rect.width);
        if (dif > 1) UpdateSize("DD__ Update");
    }

    private void OnEnable() {
        if (!isInit) Init();
        Recalculate();
    }

    private void Init() {
        isInit = true;
        tr = (RectTransform) transform;
    }

    public void Recalculate() {
        UpdateSize("Recalculate");

        curWidth = tr.rect.width;
        tr = (RectTransform) transform;
        curWidth = tr.rect.width;
    }

    private void UpdateSize(string from) {
        curWidth = tr.rect.width;

        if (canvas.rect.width > maxWidth) {
            /*tr.anchorMax = new Vector2(0, tr.anchorMax.y);
            tr.anchorMin = new Vector2(0, tr.anchorMin.y);*/
            tr.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
        }
        else if (canvas.rect.width < minWidth) {
            /*tr.anchorMax = new Vector2(0, tr.anchorMax.y);
            tr.anchorMin = new Vector2(0, tr.anchorMin.y);*/
            tr.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minWidth);
        }
        else {
            //if between min and max
            tr.anchorMin = new Vector2(0, tr.anchorMin.y);
            tr.anchorMax = new Vector2(1, tr.anchorMax.y);
            tr.sizeDelta = new Vector2(0, tr.sizeDelta.y);
        }
    }
}