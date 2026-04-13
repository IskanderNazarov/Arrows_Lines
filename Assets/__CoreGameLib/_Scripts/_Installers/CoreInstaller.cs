using __CoreGameLib._Scripts._ScriptableObjects;
using __CoreGameLib._Scripts._Services._Lang;
using __CoreGameLib._Scripts._Services._Leaderboards;
using __CoreGameLib._Scripts._Services._Purchasing;
using __CoreGameLib._Scripts._Services._RemoteConfig;
using __CoreGameLib._Scripts._Services._Saving;
using _Infrastructure;
using _Infrastructure.Services._Leaderboards;
using _Services;
using _Services._Saving;
using Core._Purchasing;
using Core._Services;
using Core._Services._Saving;
using core.ads;
using core.purchasing;
using core.rewards;
using UnityEngine;
using Zenject;

namespace __CoreGameLib._Scripts._Installers {
    public class CoreServicesInstaller : MonoInstaller {
        [SerializeField] private ProjectSettings _projectSettings;

        public override void InstallBindings() {
            if (_projectSettings.SDKType == SDK_Type.Playgama) {
                InstallFor_Playgama();
            } else if (_projectSettings.SDKType == SDK_Type.GamePush) {
                InstallFor_GamePush();
            }

            Container.Bind<ProjectSettings>().FromScriptableObject(_projectSettings).AsSingle();
            Container.Bind<SoundManager>().FromNew().AsSingle().NonLazy();
            Container.Bind<CurrencyManager>().FromNew().AsSingle().NonLazy();
            Container.Bind<RewardHandler>().FromNew().AsSingle().NonLazy();
        }

        private void InstallFor_Playgama() {
            Container.BindInterfacesAndSelfTo<AdsService_PG>().FromNew().AsSingle();
            Container.BindInterfacesAndSelfTo<Purchaser_PG>().FromNew().AsSingle();
            Container.Bind<IDataSaver>().To<DataSaver_PG>().FromNew().AsSingle().NonLazy();
            Container.Bind<IRemoteConfig>().To<RemoteConfig_PG>().FromNew().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LeaderboardService_PG>().AsSingle().NonLazy();
            Container.Bind<IPlatformActionProvider>().To<PlatformActionProvider_PG>().AsSingle().NonLazy();
        }

        private void InstallFor_GamePush() {
            Container.BindInterfacesAndSelfTo<AdsService_GP>().FromNew().AsSingle().WithArguments(_projectSettings);
            Container.BindInterfacesAndSelfTo<Purchaser_GP>().FromNew().AsSingle();
            Container.Bind<IDataSaver>().To<DataSaver_GP>().FromNew().AsSingle().WithArguments(_projectSettings.PublicKeysFor_GP).NonLazy();
            Container.Bind<IRemoteConfig>().To<RemoteConfig_GP>().FromNew().AsSingle().WithArguments(true).NonLazy();
            Container.BindInterfacesAndSelfTo<LeaderboardService_GP>().AsSingle().NonLazy();
            Container.Bind<IPlatformActionProvider>().To<PlatformActionProvider_GP>().AsSingle().NonLazy();
        }
    }
}