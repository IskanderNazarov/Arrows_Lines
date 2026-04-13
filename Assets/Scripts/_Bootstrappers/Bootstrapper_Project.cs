using System;
using System.Collections;
using __CoreGameLib._Scripts._Services._RemoteConfig;
using _Data;
using _Infrastructure;
using _Services._Localization;
using _Services._PlatformActions;
using Core._Purchasing;
using Core._Services._Saving;
using GamePush;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Services {
    public class Bootstrapper_Project : MonoBehaviour {
        [Inject] private IPurchaser _purchaser;
        [Inject] private IDataSaver _dataSaver;
        [Inject] private Localizer _localizer;
        [Inject] private IRemoteConfig _remoteConfig;
        [Inject] private IPlatformActionProvider _langProvider;
        [Inject] private ProjectSettings _projectSettings;
        [Inject] private PlayerProgressService _playerProgressService;


        private IEnumerator Start() {
            var iapSupport = true; 
#if UNITY_EDITOR
            iapSupport = false;
#endif

            var start = DateTime.Now;
            var b = DateTime.Now;
            yield return InitSDK();
            PrintTime("GP_Init.isReady", b);

            b = DateTime.Now;
            var rc = new RCKeysStorage();
            Debug.Log($"1212 _remoteConfig.LoadConfigs(, rc == null: {rc == null}, _remoteConfig type: {_remoteConfig.GetType().FullName}");
            yield return _remoteConfig.LoadConfigs(rc);
            PrintTime("_remoteConfig.LoadConfigs()", b);

            b = DateTime.Now;
            // yield async progress loading directly
            yield return _playerProgressService.Initialize();
            PrintTime("_playerProgressService.Initialize()", b);

            b = DateTime.Now;
            yield return _purchaser.Initialize(false);
            PrintTime("_purchaser.Initialize()", b);

            b = DateTime.Now;
            PrintTime("GP_Game.GameReady()", b);
            PrintTime("Total time", start);
            
            SceneManager.LoadScene("MainScene");
        }

        private IEnumerator InitSDK() {
            if (_projectSettings.SDKType == SDK_Type.GamePush) {
                yield return new WaitUntil(() => GP_Init.isReady);
            }
        }

        private void PrintTime(string placement, DateTime startTime) {
            var a = DateTime.Now;
            var t = (a - startTime).TotalSeconds;
            print($"Bootstrapper_. Time for {placement}: {t}");
        }
    }
}