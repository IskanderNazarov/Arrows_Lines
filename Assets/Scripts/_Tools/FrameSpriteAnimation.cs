using LocationsData.CommonAsssets.Scripts._MainUtilItems;
using UnityEngine;

public class FrameSpriteAnimation : FrameAnimBase {
    
    public Sprite[] frames;
    
    protected override int ItemsCount => frames.Length;
    private SpriteRenderer sr;

    protected override void Start() {
        base.Start();
        sr = GetComponent<SpriteRenderer>();
    }

    protected override void ChangeState(int itemIndex) {
        sr.sprite = frames[itemIndex];
    }
}