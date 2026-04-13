using System;
using UnityEngine;

namespace _UI {
    public abstract class BaseScreen : MonoBehaviour {
        
        // instant show
        public virtual void Show(Action onComplete = null) {
            gameObject.SetActive(true);
            onComplete?.Invoke();
        }

        // instant hide
        public virtual void Hide(Action onComplete = null) {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        }

        // useful for initial setup
        public virtual void HideImmediate() {
            gameObject.SetActive(false);
        }
    }
}