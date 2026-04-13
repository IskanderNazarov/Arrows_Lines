using System;
using System.Collections.Generic;
using _Infrastructure.Services._Leaderboards;
using GamePush;
using UnityEngine;
using UnityEngine.Events;
using Zenject;
// Важно для UnityAction

namespace __CoreGameLib._Scripts._Services._Leaderboards {
    public class LeaderboardService_GP : ILeaderboardService, IInitializable {
        // Тэг лидерборда. 
        // Если вы используете основное поле "score" в GamePush, можно оставить пустым "" 
        // или использовать "score", в зависимости от настроек вашей админки.

        public bool IsInitialized => true; // GP инициализируется автоматически через префаб
        public bool IsPlayerAuthorized => GP_Player.IsLoggedIn();

        public event Action OnInitialized;

        public void Initialize() {
            // Сообщаем игре, что сервис готов (так как SDK GP инициализируется в Bootstrapper)
            OnInitialized?.Invoke();
        }

        // --- AUTHORIZATION ---

        public void AuthorizePlayer(Action<bool> onComplete) {
            if (IsPlayerAuthorized) {
                onComplete?.Invoke(true);
                return;
            }

            // Определяем колбэки для подписки
            UnityAction onLogin = null;
            UnityAction onLoginError = null;

            onLogin = () => {
                GP_Player.OnLoginComplete -= onLogin;
                GP_Player.OnLoginError -= onLoginError;
                onComplete?.Invoke(true);
            };

            onLoginError = () => {
                GP_Player.OnLoginComplete -= onLogin;
                GP_Player.OnLoginError -= onLoginError;
                // Логин отменен или ошибка
                onComplete?.Invoke(false);
            };

            // Подписываемся
            GP_Player.OnLoginComplete += onLogin;
            GP_Player.OnLoginError += onLoginError;

            // Вызываем окно авторизации
            GP_Player.Login();
        }

        // --- SCORE SETTING ---

        //here we use parameter "leaderboardName" as "scoreKey"
        public void SetPlayerScore(string scoreKey, int score, Action<bool> onComplete) {
            // В GamePush очки (score) являются частью профиля игрока.
            // Мы сохраняем их через DataSaver_GP (который вызывает GP_Player.Sync).
            // Поэтому здесь мы просто возвращаем успех, чтобы не блокировать поток игры.
            UnityAction onCompleteAction = null;
            onCompleteAction = () => {
                GP_Player.OnSyncComplete -= onCompleteAction;
                onComplete?.Invoke(true);
            };
            GP_Player.OnSyncComplete += onCompleteAction;


            GP_Player.Set(scoreKey, score);
            GP_Player.Sync();
        }

        // --- NATIVE UI ---
        public void ShowNativeLeaderboard(string orderBy, Action<bool> onComplete) {
            // Подписываемся на закрытие окна
            UnityAction onClose = null;

            onClose = () => {
                GP_Leaderboard.OnLeaderboardClose -= onClose;
                onComplete?.Invoke(true);
            };

            GP_Leaderboard.OnLeaderboardClose += onClose;

            // Открываем лидерборд
            // order: DESC (от большего к меньшему)
            // limit: 10 игроков
            GP_Leaderboard.Open(
                orderBy: orderBy, //orderBy: "score",
                order: Order.DESC,
                limit: 10,
                withMe: WithMe.last // Показывать меня, даже если я не в топе
            );
        }

        // --- FETCH DATA ---

        public void GetLeaderboardEntries(string leaderboardName, Action<LeaderboardData> onSuccess, Action<string> onError) {
            UnityAction<string, GP_Data> onFetchSuccess = null;
            UnityAction onFetchError = null;

            onFetchSuccess = (tag, data) => {
                Debug.Log($"LB__ tag: {tag}");
                Debug.Log($"LB__ data: {data}");
                // Проверяем, что ответ пришел именно для нашего запроса (тега)
                // Если MainLeaderboardTag пустой, GP может вернуть null или "", учитываем это
                if (!string.IsNullOrEmpty(leaderboardName) && tag != leaderboardName) return;
                Debug.Log($"LB__ 111");

                // Отписываемся
                GP_Leaderboard.OnFetchSuccess -= onFetchSuccess;
                GP_Leaderboard.OnFetchError -= onFetchError;

                try {
                    // Парсим сырой JSON от GamePush
                    var leaderboardData = ParseLeaderboardData(data);
                    Debug.Log($"LB__ 222");
                    onSuccess?.Invoke(leaderboardData);
                } catch (Exception e) {
                    Debug.LogError($"[LeaderboardService_GP] Parse error: {e.Message}");
                    onError?.Invoke("Parse Error");
                }
            };

            onFetchError = () => {
                GP_Leaderboard.OnFetchSuccess -= onFetchSuccess;
                GP_Leaderboard.OnFetchError -= onFetchError;
                onError?.Invoke("Fetch Error");
            };

            GP_Leaderboard.OnFetchSuccess += onFetchSuccess;
            GP_Leaderboard.OnFetchError += onFetchError;

            // Запрашиваем данные
            GP_Leaderboard.Fetch(
                tag: leaderboardName,
                orderBy: "score",
                order: Order.DESC,
                limit: 10,
                withMe: WithMe.first, // Подгрузить игрока первым, если нужно знать его ранк
                includeFields: "name,avatar,score,id"
            );
        }

        // --- PARSING LOGIC ---

        private LeaderboardData ParseLeaderboardData(GP_Data gpData) {
            var result = new LeaderboardData();

            // Получаем JSON строку. Обычно это массив: [{"id":1...}, {"id":2...}]
            //string jsonArray = gpData.GetString();
            Debug.Log($"gpData.Data: {gpData.Data}");
            //string jsonArray = gpData.ToString();
            string jsonArray = gpData.Data;
            //string jsonArray = gpData.Get<string>();
            Debug.Log($"LB__ jsonArray: {jsonArray}");


            Debug.Log($"LB__ 555 string.IsNullOrEmpty(jsonArray): {string.IsNullOrEmpty(jsonArray)}");
            Debug.Log($"LB__ 555 jsonArray == \"[]\": {jsonArray == "[]"}");
            if (string.IsNullOrEmpty(jsonArray) || jsonArray == "[]") {
                return result;
            }

            Debug.Log($"LB__ 666");

            // Оборачиваем массив в объект, чтобы JsonUtility смог его съесть
            string wrappedJson = "{\"items\":" + jsonArray + "}";

            GPLeaderboardWrapper wrapper = JsonUtility.FromJson<GPLeaderboardWrapper>(wrappedJson);

            if (wrapper != null && wrapper.items != null) {
                Debug.Log($"LB__ 777б wrapper.items.Count: {wrapper.items.Count}");
                int fallbackRank = 1;

                foreach (var item in wrapper.items) {
                    var entry = new LeaderboardEntry();

                    // GamePush может не всегда возвращать rank/position в Fetch,
                    // иногда приходится полагаться на порядок в массиве.
                    // Проверяем, пришло ли поле position (иногда rank)
                    if (item.position > 0) entry.Rank = item.position;
                    else if (item.rank > 0) entry.Rank = item.rank;
                    else entry.Rank = fallbackRank;

                    entry.PlayerName = string.IsNullOrEmpty(item.name) ? "Anonymous" : item.name;
                    entry.Score = (int)item.score; // GP использует float, приводим к int
                    entry.PlayerId = item.id.ToString();
                    entry.PlayerPhotoUrl = item.avatar;

                    result.Entries.Add(entry);
                    fallbackRank++;

                    // Пытаемся найти текущего игрока в списке
                    int currentPlayerId = GP_Player.GetID();
                    if (item.id == currentPlayerId) {
                        result.PlayerEntry = entry;
                    }
                }
            }

            // Если игрок не попал в топ-10, но нам нужен его Entry (а мы делали fetch withMe=first/last),
            // GamePush иногда может присылать его отдельно или в том же списке.
            // Если PlayerEntry все еще null, можно попробовать создать его из локальных данных GP_Player
            if (result.PlayerEntry == null && IsPlayerAuthorized) {
                result.PlayerEntry = new LeaderboardEntry {
                    PlayerId = GP_Player.GetID().ToString(),
                    PlayerName = GP_Player.GetName(),
                    Score = (int)GP_Player.GetScore(),
                    PlayerPhotoUrl = GP_Player.GetAvatarUrl(),
                    Rank = 0 // Ранк неизвестен, если не пришел с сервера
                };
            }

            return result;
        }

        // --- DTO CLASSES FOR PARSING ---
        // Приватные классы для маппинга JSON от GamePush

        [Serializable]
        private class GPLeaderboardWrapper {
            public List<GPPlayerItem> items;
        }

        [Serializable]
        private class GPPlayerItem {
            public int id;
            public string name;
            public float score;
            public string avatar;
            public int position; // Часто используется в GP
            public int rank; // Альтернативное имя поля
        }
    }
}