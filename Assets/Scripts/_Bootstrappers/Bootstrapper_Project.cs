using System;
using System.Collections;
using __CoreGameLib._Scripts._ScriptableObjects;
using __CoreGameLib._Scripts._Services._Lang;
using __CoreGameLib._Scripts._Services._RemoteConfig;
using _Infrastructure;
using _Services._Saving;
using Core._Purchasing;
using Core._Services;
using Core._Services._Saving;
using GamePush;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Services {
    public class Bootstrapper_Project : MonoBehaviour {
        [Inject] private CurrencyManager _currencyManager;
        [Inject] private IPurchaser _purchaser;
        [Inject] private IDataSaver _dataSaver;
        [Inject] private Localizer _localizer;
        [Inject] private IRemoteConfig _remoteConfig;
        [Inject] private IPlatformActionProvider _langProvider;
        [Inject] private ProjectSettings _projectSettings;
        [Inject] private PlayerProgressService _playerProgressService;


        private IEnumerator Start() {
            var iapSupport = true; //Bridge.platform.id == "yandex";
#if UNITY_EDITOR
            iapSupport = false;
#endif

            var start = DateTime.Now;
            var b = DateTime.Now;
            yield return InitSDK();
            PrintTime("GP_Init.isReady", b);
//#if !UNITY_EDITOR

            b = DateTime.Now;
            var rc = new RCKeysStorage();
            Debug.Log($"1212 _remoteConfig.LoadConfigs(, rc == null: {rc == null}, _remoteConfig type: {_remoteConfig.GetType().FullName}");
            yield return _remoteConfig.LoadConfigs(new RCKeysStorage());
            PrintTime("_remoteConfig.LoadConfigs()", b);

            b = DateTime.Now;
            yield return _dataSaver.LoadData(new DataKeysStorage());
            PrintTime("_dataSaver.LoadData", b);

//#endif
            b = DateTime.Now;
            yield return _currencyManager.Initialize();
            PrintTime("_currencyManager.Initialize()", b);

            b = DateTime.Now;
            yield return _purchaser.Initialize(false);
            PrintTime("_purchaser.Initialize()", b);
            //#endif

            //RCH.Initialize(_remoteConfig);

            b = DateTime.Now;
            _localizer.Initialize();
            PrintTime("_localizer.Initialize()", b);
            //-------------------------------
            
            _playerProgressService.Initialize();
            //-------------------------------


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