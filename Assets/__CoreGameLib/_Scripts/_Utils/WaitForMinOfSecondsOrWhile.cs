using System;
using UnityEngine;

public class WaitForMinOfSecondsOrWhile : CustomYieldInstruction {
    private readonly float delayTime;
    private readonly Func<bool> predicate;

    private readonly float startTime;


    public WaitForMinOfSecondsOrWhile(float seconds, Func<bool> predicate) {
        delayTime = seconds;
        startTime = Time.time;
        this.predicate = predicate;
    }

    public override bool keepWaiting => Time.time - startTime < delayTime && (predicate?.Invoke() ?? false);


    public override void Reset() {
    }
}