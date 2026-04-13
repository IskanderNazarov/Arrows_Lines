using System.Collections.Generic;
using System.Linq;
using __CoreGameLib._Scripts._Services._Leaderboards;
using _Infrastructure.Services._Leaderboards;
using _Services._Localization;
using _UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LeaderboardDialog : MonoBehaviour {
    public const string LeaderboardName = "lb_name";
    [Header("UI References")] [SerializeField]
    private Transform contentRoot;

    [SerializeField] private LeaderboardEntryView entryPrefab;
    [SerializeField] private GameObject separatorPrefab;

    [Header("Content Panels")] [SerializeField]
    private GameObject authorizationPanel;

    [SerializeField] private Button authorizeButton;

    [Header("Display Logic")] [Tooltip("The number of top players to show (N).")] [SerializeField]
    private int maxTopEntriesToShow = 10;

    [Header("UI States")] [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private GameObject errorIndicator;
    [SerializeField] private TextMeshProUGUI errorTMP;
    [SerializeField] private Sprite[] first3RankSprites;

    [Header("Localization")] [SerializeField]
    private TextMeshProUGUI titleTMP;

    [SerializeField] private TextMeshProUGUI instructionsTitleTMP;
    [SerializeField] private TextMeshProUGUI instructionsTMP;
    [SerializeField] private TextMeshProUGUI loadErrorTMP;

    private ILeaderboardService _leaderboardService;
    private GameConfig _gameConfig;
    private readonly List<LeaderboardEntryView> _entryViewPool = new List<LeaderboardEntryView>();
    private readonly List<GameObject> _separatorPool = new List<GameObject>();

    [Inject]
    public void Construct(ILeaderboardService leaderboardService, GameConfig gameConfig) {
        _leaderboardService = leaderboardService;
        _gameConfig = gameConfig;
    }

    private void Awake() {
        if (authorizeButton) authorizeButton.onClick.AddListener(OnAuthorizeClicked);
    }

    public void Show() {
        gameObject.SetActive(true);
        ClearContent();
        SetupLocalization();

        if (!_leaderboardService.IsInitialized) {
            //ShowError(Localizer.LeaderboardServiceNotAvailable); //"Leaderboard service is not available."
            return;
        }

        if (_leaderboardService.IsPlayerAuthorized) {
            ShowLeaderboardView();
        } else {
            ShowAuthorizationView();
        }
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    private void ShowLeaderboardView() {
        authorizationPanel.SetActive(false);
        ShowLoading();
        _leaderboardService.GetLeaderboardEntries(LeaderboardName, OnDataLoaded, OnDataError);
    }

    private void ShowAuthorizationView() {
        authorizationPanel.SetActive(true);
        loadingIndicator.SetActive(false);
        errorIndicator.SetActive(false);
    }

    private void OnAuthorizeClicked() {
        ShowLoading();
        authorizationPanel.SetActive(false);
        _leaderboardService.AuthorizePlayer(OnAuthorizationCompleted);
    }


    private void OnAuthorizationCompleted(bool success) {
        if (success) {
            ShowLeaderboardView();
        } else {
            ShowAuthorizationView();
            //ShowError(Localizer.AuthFailedError); //"Authorization Failed. Please try again."
        }
    }

    private void OnDataLoaded(LeaderboardData data) {
        loadingIndicator.SetActive(false);
        errorIndicator.SetActive(false);

        if (data.Entries == null || data.Entries.Count == 0) {
            return;
        }

        var playerIndex = -1;
        if (data.PlayerEntry != null) {
            playerIndex = data.Entries.FindIndex(e => e.PlayerId == data.PlayerEntry.PlayerId);
        }

        var isPlayerInTopList = playerIndex != -1 && playerIndex < maxTopEntriesToShow;

        if (isPlayerInTopList || playerIndex == -1) {
            PopulateEntries(data, data.Entries.Take(maxTopEntriesToShow));
        } else {
            PopulateEntries(data, data.Entries.Take(maxTopEntriesToShow));

            if (separatorPrefab != null) {
                GetSeparatorFromPool();
            }

            var playerNeighborhood = new List<LeaderboardEntry>();

            if (playerIndex - 1 >= maxTopEntriesToShow) {
                playerNeighborhood.Add(data.Entries[playerIndex - 1]);
            }

            playerNeighborhood.Add(data.Entries[playerIndex]);
            if (playerIndex + 1 < data.Entries.Count) {
                playerNeighborhood.Add(data.Entries[playerIndex + 1]);
            }

            PopulateEntries(data, playerNeighborhood);
        }
    }

    private void PopulateEntries(LeaderboardData fullData, IEnumerable<LeaderboardEntry> entriesToShow) {
        foreach (var entryData in entriesToShow) {
            var entryView = GetEntryFromPool();
            ///Debug.Log($"Leader__ name: {entryData.PlayerName}, id: {entryData.PlayerId}, fullData.PlayerEntry.PlayerId: {fullData.PlayerEntry.PlayerId}");
            var isPlayer = fullData.PlayerEntry != null && fullData.PlayerEntry.PlayerId == entryData.PlayerId;
            entryView.Populate(entryData, isPlayer);

            if (entryData.Rank > 0 && entryData.Rank <= 3) {
                entryView.UpdateFirst3View(first3RankSprites[entryData.Rank - 1]);
            }
        }
    }

    private void OnDataError(string errorMessage) {
        //Debug.LogError($"[LeaderboardDialog] Failed to load data: {errorMessage}");
        //ShowError(Localizer.LeaderboardLoadError);
    }

    [SerializeField] private TextMeshProUGUI loginTitle;
    [SerializeField] private TextMeshProUGUI authInfo;


    private void SetupLocalization() {
        /*titleTMP.text = Localizer.LeaderboardTitle;
        loginTitle.text = Localizer.LoginBtnText;
        authInfo.text = Localizer.AuthInfo;


        instructionsTitleTMP.text = Localizer.LeaderboardInfoTitle;
        instructionsTMP.text = Localizer.LeaderboardInfoStory;*/
    }

    /*private string ApplyHoneyColor(string text) {
        //var f = $"<color=#{c}>{Localizer.Remove3BoosterName}</color>";
        var c = _gameConfig.scoreInstructColor.ToHexString();
        return $"<color=#{c}>{text}</color>";
    }*/

    private LeaderboardEntryView GetEntryFromPool() {
        foreach (var entry in _entryViewPool) {
            if (!entry.gameObject.activeSelf) {
                entry.gameObject.SetActive(true);
                return entry;
            }
        }

        var newEntry = Instantiate(entryPrefab, contentRoot);
        _entryViewPool.Add(newEntry);
        return newEntry;
    }

    private void GetSeparatorFromPool() {
        foreach (var separator in _separatorPool) {
            if (!separator.activeSelf) {
                separator.SetActive(true);
                return;
            }
        }

        var newSeparator = Instantiate(separatorPrefab, contentRoot);
        _separatorPool.Add(newSeparator);
    }

    private void ClearContent() {
        foreach (var entry in _entryViewPool) entry.gameObject.SetActive(false);
        foreach (var separator in _separatorPool) separator.SetActive(false);
    }

    private void ShowLoading() {
        loadingIndicator.SetActive(true);
        errorIndicator.SetActive(false);
    }

    private void ShowError(string message) {
        loadingIndicator.SetActive(false);
        errorIndicator.SetActive(true);
        // if you have a Text component for the error message, set it here
        loadErrorTMP.text = message;
    }
}