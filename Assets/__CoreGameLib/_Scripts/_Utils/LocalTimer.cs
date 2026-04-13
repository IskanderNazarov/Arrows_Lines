using System;
using UnityEngine;

namespace Scripts._UI {
    public class LocalTimer : MonoBehaviour {
        public int Time => (int) _timer;
        
        private bool _isRunning;
        private float _timer;
        private float _startTime;
        private float _endTime;
        private Action _onCompleted;

        private int prevTimerValue;
        public Action OnTimerTicked;

        public static LocalTimer CreateTime() {
            var go = new GameObject("Timer");
            var timer = go.AddComponent<LocalTimer>();

            return timer;
        }

        public void StartCountDownTimer(int startTime, Action onCompleted = null) {
            StartTimer(startTime, 0, onCompleted);
        }
        
        public void StartStopwatch() {
            StartTimer(0, Int32.MaxValue, null);
        }

        private void StartTimer(int startTime, int endTime, Action onCompleted = null) {
            _onCompleted = onCompleted;
            _startTime = startTime;
            _endTime = endTime;
            _timer = startTime;
            
            _isRunning = true;
        }

        private void Update() {
            if (!_isRunning) return;

            _timer += _startTime < _endTime ? UnityEngine.Time.deltaTime : -UnityEngine.Time.deltaTime;
            if (_startTime < _endTime && _timer > _endTime) {
                _isRunning = false;
                _onCompleted?.Invoke();
            }
            
            if (_startTime > _endTime && _timer < _endTime) {
                _isRunning = false;
                _onCompleted?.Invoke();
            }


            if (prevTimerValue != (int) _timer && _isRunning) {
                prevTimerValue = (int)_timer;
                OnTimerTicked?.Invoke();
            }

        }
    }
}