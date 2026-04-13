using System.Collections.Generic;
using __Gameplay;
using __Gameplay._Services._Boosters;
using _ScriptableObjects._IAP_Data;
using _ScriptableObjects._LevelData;
using _Services._Localization;
using Core._RewardPresenter;
using Core._Services.SoundManagement;
using core.rewards;
using Game.SoundManagement;
using Services;
using UnityEngine;
using Zenject;

namespace _Zenject {
    public class ProjectContextInstaller : MonoInstaller {
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private IAP_Config iapConfig;
        [SerializeField] private LocalesSettings localesSettings;
        [SerializeField] private RewardSequencePanel rewardPanelPrefab; //link to prefab
        [SerializeField] private LevelsDatabase _levelsDatabase;

        public override void InstallBindings() {
            DeclareSignals();

            Container.Bind<Localizer>().FromNew().AsSingle().WithArguments(localesSettings).NonLazy();
            //Container.BindInterfacesAndSelfTo<SoundHelper>().FromNew().AsSingle().WithArguments(_soundsContainer).NonLazy();
            Container.Bind<GameConfig>().FromScriptableObject(gameConfig).AsSingle().NonLazy();
            Container.Bind<IAP_Config>().FromScriptableObject(iapConfig).AsSingle().NonLazy();
            Container.Bind<LevelsDatabase>().FromScriptableObject(_levelsDatabase).AsSingle().NonLazy();
            
            Container.Bind<PlayerProgressService>().FromNew().AsSingle().NonLazy();
            Container.Bind<ISoundStateProvider>().To<GameSoundStateProvider>().AsSingle().NonLazy();


            Container.Bind<IRewardApplier>().To<GameRewardApplier>().FromNew().AsSingle().NonLazy();
            Container.Bind<IRewardPresenter>().To<RewardPresenter>().AsSingle().WithArguments(rewardPanelPrefab, Container).NonLazy();
            InstallBoostersServices();
        }

        private void DeclareSignals() {
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<LevelCompletedSignal>();
            Container.DeclareSignal<NextLevelClickedSignal>();
            Container.DeclareSignal<RestartLevelClickedSignal>();
            Container.DeclareSignal<BoosterBtnClickSignal>();
            Container.DeclareSignal<LivesChangedSignal>();
            Container.DeclareSignal<GameOverSignal>();
            Container.DeclareSignal<ReviveSignal>();
            Container.DeclareSignal<ProcessAdForReviveSignal>();
            Container.DeclareSignal<OnSnakeClickedSignal>();
            Container.DeclareSignal<ScreenSizeChangedSignal>();
        }

        private void InstallBoostersServices() {
            //todo default values are set in the keys storage
            var caps = new Dictionary<BoosterId, int> {
                { BoosterId.Hint, 2 },
                { BoosterId.Hammer, 2 },
                { BoosterId.Ruler, 2 },
            };
            Container.BindInterfacesAndSelfTo<GameBoosterInventory>().AsSingle().WithArguments( /*defaultValues, */caps);
            Container.BindInterfacesAndSelfTo<BoosterConfig>().FromNew().AsSingle();
        }
    }
}