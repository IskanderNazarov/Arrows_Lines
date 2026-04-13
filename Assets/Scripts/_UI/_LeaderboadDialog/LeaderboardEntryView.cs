    using _Infrastructure.Services._Leaderboards;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

namespace _UI {
// this component manages the visuals for a single entry in the leaderboard list.
    public class LeaderboardEntryView : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI rankTMP;
        [SerializeField] private TextMeshProUGUI nameTMP;
        [SerializeField] private TextMeshProUGUI scoreTMP;
        [SerializeField] private Image first3PlaceIcon;
        [SerializeField] private Image playerHighlight; // background to highlight the current player

        // populates the UI elements with data from a LeaderboardEntry.
        public void Populate(LeaderboardEntry data, bool isCurrentPlayer) {
            first3PlaceIcon.gameObject.SetActive(false);
            if (rankTMP) rankTMP.text = data.Rank.ToString();
            if (nameTMP) nameTMP.text = data.PlayerName;
            if (scoreTMP) scoreTMP.text = data.Score.ToString("N0"); // formats score with commas

            // show a special highlight if this entry is for the current player
            if (playerHighlight) playerHighlight.gameObject.SetActive(isCurrentPlayer);

            if (data.Rank <= 3) {
                rankTMP.gameObject.SetActive(false);
            }
        }

        public void UpdateFirst3View(Sprite first3RankSprite) {
            first3PlaceIcon.gameObject.SetActive(true);
            first3PlaceIcon.sprite = first3RankSprite;
            rankTMP.gameObject.SetActive(false);
        }
    }
}