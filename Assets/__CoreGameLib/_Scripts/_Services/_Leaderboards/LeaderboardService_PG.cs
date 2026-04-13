using System;
using System.Collections.Generic;
using __CoreGameLib._Scripts._Services._Leaderboards;
using Playgama;
using Playgama.Modules.Leaderboards;
using UnityEngine;
using Zenject;

namespace _Infrastructure.Services._Leaderboards {
    public class LeaderboardService_PG : ILeaderboardService, IInitializable {
        //private const string LeaderboardName = "leaders";

        public bool IsInitialized { get; private set; }
        public bool IsPlayerAuthorized => Bridge.player.isAuthorized;
        public event Action OnInitialized;

        public void Initialize() {
            if (IsInitialized) return;

            if (Bridge.leaderboards.type == LeaderboardType.NotAvailable) {
                Debug.LogWarning("[LeaderboardService] Leaderboards are not available on this platform.");
                return;
            }

            IsInitialized = true;
            OnInitialized?.Invoke();
            Debug.Log("[LeaderboardService] Service initialized successfully.");
        }
        
        public void AuthorizePlayer(Action<bool> onComplete) {
            if (IsPlayerAuthorized) {
                //Debug.Log("[LeaderboardService] Player is already authorized.");
                onComplete?.Invoke(true);
                return;
            }

            if (Bridge.player.isAuthorizationSupported) {
                Bridge.player.Authorize(null, success => {
                    /*if (success) {
                        Debug.Log("[LeaderboardService] Player authorized successfully.");
                    } else {
                        Debug.LogError("[LeaderboardService] Player authorization failed.");
                    }*/
                    onComplete?.Invoke(success);
                });
            } else {
                //Debug.LogWarning("[LeaderboardService] Authorization is not supported on this platform.");
                onComplete?.Invoke(false);
            }
        }

        public void SetPlayerScore(string leaderboardName, int score, Action<bool> onComplete) {
            if (!IsInitialized) {
                //Debug.LogError("[LeaderboardService] Cannot set score: service is not initialized.");
                onComplete?.Invoke(false);
                return;
            }
            
            if (!IsPlayerAuthorized) {
                //Debug.LogWarning("[LeaderboardService] Cannot set score: player is not authorized.");
                onComplete?.Invoke(false);
                return;
            }

            Bridge.leaderboards.SetScore(leaderboardName, score, (success) => {
                onComplete?.Invoke(success);
            });
        }

        public void ShowNativeLeaderboard(string leaderboardName, Action<bool> onComplete) {
            if (!IsInitialized) {
                //Debug.LogError("[LeaderboardService] Cannot show native leaderboard: service is not initialized.");
                onComplete?.Invoke(false);
                return;
            }

            if (Bridge.leaderboards.type != LeaderboardType.Native) {
                //Debug.LogWarning("[LeaderboardService] Native leaderboard popups are not supported on this platform.");
                onComplete?.Invoke(false);
                return;
            }

            Bridge.leaderboards.ShowNativePopup(leaderboardName, (success) => {
                /*if (success) {
                    Debug.Log("[LeaderboardService] Native leaderboard shown.");
                }
                else {
                    Debug.LogError("[LeaderboardService] Failed to show native leaderboard.");
                }*/
                onComplete?.Invoke(success);
            });
        }

        public void GetLeaderboardEntries(string leaderboardName, Action<LeaderboardData> onSuccess, Action<string> onError) {
            if (!IsInitialized) {
                onError?.Invoke("Service is not initialized.");
                return;
            }

            if (Bridge.leaderboards.type != LeaderboardType.InGame) {
                onError?.Invoke("In-game leaderboards are not supported on this platform.");
                return;
            }

            Bridge.leaderboards.GetEntries(leaderboardName, (success, entries) => {
                if (success) {
                    var data = ParseEntries(entries);
                    onSuccess?.Invoke(data);
                }
                else {
                    onError?.Invoke("Failed to retrieve leaderboard entries from SDK.");
                }
            });
        }

        private LeaderboardData ParseEntries(List<Dictionary<string, string>> sdkEntries) {
            var data = new LeaderboardData();
            foreach (var entry in sdkEntries) {
                var newEntry = new LeaderboardEntry {
                    Rank = int.Parse(entry["rank"]),
                    PlayerName = entry["name"],
                    Score = int.Parse(entry["score"]),
                    PlayerId = entry["id"],
                    PlayerPhotoUrl = entry["photo"]
                };
                data.Entries.Add(newEntry);
            }
            
            data.PlayerEntry = data.Entries.Find(e => e.PlayerId == Bridge.player.id);
            return data;
        }
    }
}