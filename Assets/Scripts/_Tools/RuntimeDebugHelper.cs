using _UI;
using UnityEngine;

namespace LocationsData.CommonAsssets.Scripts._MainUtilItems {
    public class RuntimeDebugHelper : MonoBehaviour {
        [SerializeField] private SettingsDialog settingsDialog;
        private int clickCounter = 1;

        private void Update() {
            /*if (Input.GetKeyDown(KeyCode.A)) {
                var p = gameOverLine.transform.localPosition;
                p.y = -0.4f;
                gameOverLine.transform.localPosition = p;
            }*/

            /*if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) &&
                Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Space)) {
                YandexGame.savesData = new SavesYG();
            }*/
        }

        private void PrintLeaderboard(string leaderboardName) {
            /*YandexGame.onGetLeaderboard += UserCoinsLeaderLoaded;
            YandexGame.GetLeaderboard(leaderboardName, 100, 90, 100, "nonePhoto");*/
        }

        /*private void UserCoinsLeaderLoaded(LBData data) {
            YandexGame.onGetLeaderboard -= UserCoinsLeaderLoaded;
            print(" -----------------------");
            print(" ------- PLAYERS ------");
            Debug.Log($"data.entries:\n {data.entries}\n");

            /*foreach (var player in data.players) {
                var info = $"{player.rank}. {player.name}: {player.score.ToString()}";
                Debug.Log($"{info}");
            }
            #1#


            print(" -----------------------");
        }*/

        public void PrintOldLeaderboard() {
            PrintLeaderboard("scores");
        }

        public void PrintNewLeaderboard() {
            PrintLeaderboard("scoresbasenew");
        }

        public void PrintCoinsLeaderboard() {
            PrintLeaderboard("usercoins");
        }
        
        public void PrintGemsLeaderboard() {
            PrintLeaderboard("usergems");
        }
        
        public void ResetProgress() {
            /*YandexGame.savesData = new SavesYG();
            YandexGame.SaveProgress();*/
        }


        public void OnOpenClicked() {
            gameObject.SetActive(clickCounter % 10 == 0);
            settingsDialog.gameObject.SetActive(!gameObject.activeSelf);
            clickCounter++;
        }

        public void ReduceGems() {
            /*YandexGame.savesData.gemsCount = 0;
            YandexGame.SaveProgress();*/
        }
    }
}