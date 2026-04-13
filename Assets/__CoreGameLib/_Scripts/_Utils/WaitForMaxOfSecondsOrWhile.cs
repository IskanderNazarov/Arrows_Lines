using System;
using UnityEngine;

public class WaitForMaxOfSecondsOrWhile : CustomYieldInstruction {
    private readonly float delayTime;
    private readonly Func<bool> predicate;

    private readonly float startTime;


    public WaitForMaxOfSecondsOrWhile(float seconds, Func<bool> predicate) {
        delayTime = seconds;
        startTime = Time.time;
        this.predicate = predicate;
    }
    /*public override bool keepWaiting {
        get{
            var b = Time.time - startTime < delayTime;
            if (b) {
                startTime = Time.time;
                return false;
            }
            //Debug.Log("B: " + b);
            return b/* && (predicate?.Invoke() ?? true)#1#;
        }
    }*/

    public override bool keepWaiting => Time.time - startTime < delayTime || (predicate?.Invoke() ?? false);


    public override void Reset() {
    }
}