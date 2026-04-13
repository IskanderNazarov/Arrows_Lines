using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public static class Utils {
    private const string Format_Full = @"hh\:mm\:ss";
    private const string Format_NoHour = @"mm\:ss";

    //-----------------------------------------------------------------------------
    public static float GetDeterminedRandom(Vector2 st) {
        var v = Mathf.Sin(Vector2.Dot(st, new Vector2(12.9898f, 78.233f))) * 43758.5453123f;
        var fract = v - Mathf.Floor(v);
        return fract;
    }

    public static bool IsPointerOverUIRaycastTarget() {
        var eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    public static string FormatTime(int secondsCount) {
        var time = TimeSpan.FromSeconds(secondsCount);
        var formatString = time.Hours == 0 ? Format_NoHour : Format_Full;
        var timerStr = time.ToString(formatString);

        return timerStr;
    }

    public static int ConvertSecondsToMinutes(int seconds) {
        return seconds / 60;
    }
}

internal static class Extensions {
    public static void Shuffle<T>(this IList<T> list) {
        var n = list.Count;
        while (n != 0) {
            var randInd = Random.Range(0, n - 1);
            var t = list[n - 1];
            list[n - 1] = list[randInd];
            list[randInd] = t;

            n--;
        }
    }

    public static void ShuffleDetermined<T>(this IList<T> list, float x, float y) {
        var n = list.Count;
        while (n != 0) {
            //var randInd = Random.Range(0, n - 1);
            var randInd = (int) (Utils.GetDeterminedRandom(new Vector2(x, y)) * (n - 1));
            var t = list[n - 1];
            list[n - 1] = list[randInd];
            list[randInd] = t;

            n--;
        }
    }

    public static void ResetLocal(this Transform tr, Transform parent) {
        tr.SetParent(parent);
        tr.localPosition = Vector3.zero;
        tr.localScale = Vector3.one;
        tr.localRotation = Quaternion.identity;
    }
}