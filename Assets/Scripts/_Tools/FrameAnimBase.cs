using UnityEngine;

namespace LocationsData.CommonAsssets.Scripts._MainUtilItems {
    public abstract class FrameAnimBase : MonoBehaviour {
        //public Sprite[] frames;
        [SerializeField] private float timeInterval;
        [SerializeField] private bool isPlayingOnStart = true;
        [SerializeField] private bool isLooping;

        public bool IsPlaying => !isStopped;
        
        private float timer;
        private int ind;
        private bool isStopped;
        private bool blockStartMethodWorkStopped; //when game object was activated and the Start is not called yet

        protected virtual void Start() {
            if (!isPlayingOnStart && !blockStartMethodWorkStopped) {
                StopAnim();
            }
        }

        protected abstract int ItemsCount { get; }
        protected abstract void ChangeState(int itemIndex);

        private void Update() {
            if (isStopped) return;

            if (timer >= timeInterval) {
                ChangeState(ind);
                ind++;
                if (ind >= ItemsCount) {
                    if (isLooping) {
                        ind = 0;
                    }

                    isStopped = !isLooping;
                }

                timer = 0;
            }


            timer += Time.deltaTime;
        }

        public void StopAnim() {
            blockStartMethodWorkStopped = true;
            isStopped = true;
        }

        public virtual void StartAnim() {
            blockStartMethodWorkStopped = true;
            isStopped = false;
        }

        /*public void ResetTimer() {
            timer = 0;
        }*/

        public void Reset() {
            ind = 0;
            timer = 0;
        }
    }
}