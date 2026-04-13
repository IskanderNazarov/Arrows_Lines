using UnityEngine;
using UnityEngine.UI;

public class AspectRatioFitterExtension : MonoBehaviour {
    [SerializeField] private RectTransform canvasRect;
    private AspectRatioFitter aspectRatioFitter;
    private float canvasAspectRatio;

    private void Start() {
        aspectRatioFitter = GetComponent<AspectRatioFitter>();

        canvasAspectRatio = canvasRect.rect.width / canvasRect.rect.height;
    }

    private void Update() {
        var currentCanvasAspectRatio = canvasRect.rect.width / canvasRect.rect.height;
        if (Mathf.Abs(canvasAspectRatio - currentCanvasAspectRatio) <= float.Epsilon) return;

        //if canvas size changed
        canvasAspectRatio = canvasRect.rect.width / canvasRect.rect.height;


        //if canvas is less wide than aspectFitter then let the width control the height
        aspectRatioFitter.aspectMode = canvasAspectRatio > aspectRatioFitter.aspectRatio
            ? AspectRatioFitter.AspectMode.HeightControlsWidth
            : AspectRatioFitter.AspectMode.WidthControlsHeight;
    }
}