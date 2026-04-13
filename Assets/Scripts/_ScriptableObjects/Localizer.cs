using System;
using System.Collections.Generic;
using System.Linq;
using __CoreGameLib._Scripts._Services._Lang;
using Playgama;
using UnityEngine;
using Random = UnityEngine.Random;

public class Localizer {
    private static Localizer shared; // { get; private set; }
    private List<LocaleData> _allData;

    private LocaleData _currentLoc;
    private LocaleData _defaultLoc;
    private IPlatformActionProvider _platformActionProvider;
    public static bool IsInitialized => shared != null;

    //--------------------------------------------------------------------------
    private Localizer(List<LocaleData> allData, IPlatformActionProvider  platformActionProvider) {
        _allData = allData;
        _platformActionProvider = platformActionProvider;
    }

    //--------------------------------------------------------------------------
    public void Initialize() {
        shared = this;
        var langCode = _platformActionProvider.GetISO();
        Debug.Log($"langCode: {langCode} from bridge in INIT");

#if UNITY_EDITOR

        langCode = "en"; //GetLang();//
#endif


        var isParsed = Enum.TryParse(langCode.ToLower(), true, out LangCode parsedLocaled);
        if (isParsed) {
            _currentLoc = _allData.Find(l => l.language == parsedLocaled); //
        }

        _defaultLoc = _allData.Find(l => l.language == LangCode.en);
        if (_currentLoc == null) {
            _currentLoc = _defaultLoc;
        }
    }

    //--------------------------------------------------------------------------

    public static string musicTitle => shared._currentLoc.musicTitle;
    public static string soundTitle => shared._currentLoc.soundTitle;
    public static string MarketPanelTitle => shared._currentLoc.MarketPanelTitle;
    public static string Get => shared._currentLoc.Get;
    public static string GetFreeCoins => shared._currentLoc.GetFreeCoins;
    public static string Market => shared._currentLoc.Market;
    public static string LevelTitle => shared._currentLoc.Level;
    public static string Continue => shared._currentLoc.Continue;
    public static string Restart => shared._currentLoc.Restart;
    public static string[] Congrats => shared._currentLoc.Congrats;

    //--------------------------------------------------------------------------
    public static string YourReward => shared._currentLoc.YourReward;
    public static string ExtraReward => shared._currentLoc.ExtraReward;
    public static string ChestRewardTitle => shared._currentLoc.ChestRewardTitle;

    //--------------------------------------------------------------------------
    public static string LeaderboardTitle => shared._currentLoc.LeaderboardTitle;
    public static string LoginBtnText => shared._currentLoc.LoginBtnTe;
    public static string AuthInfo => shared._currentLoc.AuthInfo;
    public static string LeaderboardInfoTitle => shared._currentLoc.LeaderboardInfoTitle;
    public static string LeaderboardInfoStory => shared._currentLoc.LeaderboardInfoStory;
    public static string LeaderboardLoadError => shared._currentLoc.LeaderboardLoadError;
    public static string AuthFailedError => shared._currentLoc.AuthFailedError;
    public static string LeaderboardServiceNotAvailable => shared._currentLoc.LeaderboardServiceNotAvailable;
    //--------------------------------------------------------------------------
}


public enum LangCode {
    en,
    ru,
    tr
}