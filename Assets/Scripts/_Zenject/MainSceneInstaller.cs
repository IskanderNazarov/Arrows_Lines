using _ExtensionsHelpers;
using _game;
using _game._LevelsProviding;
using _Infrastructure;
using _ScriptableObjects;
using _UI;
using SoundManaging;
using UnityEngine;
using Zenject;

namespace _Zenject {
    public class MainSceneInstaller : MonoInstaller {
        [SerializeField] private GameplayController _gameplayController;
        [SerializeField] private FXCanvas _fxCanvas;
        [SerializeField] private GeneralSoundData _soundsContainer;
        [SerializeField] private UIManager _uiManager;


        [SerializeField] private SnakesGlobalConfig _snakesGlobalConfig;

        public override void InstallBindings() {
            // Controllers
            Container.BindInstance(_gameplayController).AsSingle();
            Container.BindInterfacesAndSelfTo<GameManager>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SoundHelper>().FromNew().AsSingle().WithArguments(_soundsContainer).NonLazy();
            Container.BindInstance(_uiManager).AsSingle();
            //Container.Bind<Camera>().FromComponentOn(mainCamera.gameObject).AsSingle().NonLazy();

            // Services
            Container.Bind<ILevelProvider>().To<LevelProvider>().AsSingle();
            Container.Bind<SnakesGlobalConfig>().FromScriptableObject(_snakesGlobalConfig).AsSingle().NonLazy();
            Container.Bind<AnimationsService>().FromNew().AsSingle().WithArguments(_fxCanvas).NonLazy();

            Container.Bind<ICameraService>().To<CameraController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<BoosterVisualService>().FromComponentInHierarchy().AsSingle();
        }
    }
}