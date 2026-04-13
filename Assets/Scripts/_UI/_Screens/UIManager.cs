using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace _UI {
    public class UIManager : MonoBehaviour {
        [SerializeField] private List<BaseScreen> screens; // assign all screens in inspector

        private BaseScreen _currentScreen;

        [Inject]
        public void Construct() {
            // hide all screens on start
            foreach (var screen in screens) {
                screen.HideImmediate();
            }
        }

        // generic method to switch screens
        public T ShowScreen<T>(Action onComplete = null) where T : BaseScreen {
            var nextScreen = screens.FirstOrDefault(s => s is T);
            if (nextScreen == null) {
                Debug.LogError($"// screen {typeof(T)} not found in UIManager list!");
                return null;
            }

            if (_currentScreen != null && _currentScreen != nextScreen) {
                // hide current, then show next
                _currentScreen.Hide(() => {
                    _currentScreen = nextScreen;
                    _currentScreen.Show(onComplete);
                });
            } else {
                // just show if no screen is active
                _currentScreen = nextScreen;
                _currentScreen.Show(onComplete);
            }

            return nextScreen as T;
        }

        // get screen reference if we need to pass data before showing
        public T GetScreen<T>() where T : BaseScreen {
            return screens.FirstOrDefault(s => s is T) as T;
        }
    }
}