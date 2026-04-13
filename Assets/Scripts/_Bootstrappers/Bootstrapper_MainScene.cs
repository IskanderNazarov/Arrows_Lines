using __CoreGameLib._Scripts._Services._Lang;
using GamePush;
using UnityEngine;
using Zenject;

namespace _Infrastructure.Services {
    public class Bootstrapper_MainScene : MonoBehaviour {
        [Inject] private GameManager _gameManager;
        [Inject] private IPlatformActionProvider _actionProvider;


        private void Start() {
            _actionProvider.CallGameReadyAPI();
        }
    }
}