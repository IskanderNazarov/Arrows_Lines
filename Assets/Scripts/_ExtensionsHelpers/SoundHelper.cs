using System;
using System.Collections.Generic;
using Core._Services;
using Core._Services.SoundManagement;
using Hellmade.Sound;
using SoundManaging;
using UnityEngine;
using Zenject;

namespace _ExtensionsHelpers {
    public class SoundHelper : IDisposable, IInitializable {
        private SignalBus _signalBus;
        private SoundManager _soundManager;
        private GeneralSoundData _generalSoundData;
        private Audio _bgAudio;

        private Dictionary<string, SoundInfo> _clickMelodyMap = new();

        /*private List<string> _clickMelody = new(new[] {
            // Ваша изначальная часть
            "C6", "B5", "G5", "C6", "B5", "A5", "G5", "A5", "B5", "A5",

            // Моё первое продолжение
            "G5", "E5", "G5", "A5", "B5", "C6", "B5", "G5", "A5" , "E5", "G5", "A5",

            // Новое продолжение на 30 нот
            "C6", "B5", "G5", "E5", "G5", "A5", "B5", "G5", "C6", "D6",
            "C6", "B5", "A5", "G5", "E5", "G5", "A5", "B5", "C6", "A5",
            "G5", "E5", "D5", "E5", "G5", "A5", "C6", "B5", "G5", "A5"
        });*/
        List<string> _clickMelody = new List<string> {
            // Восходящее движение
            "C5", "E5", "G5", "C6", "E6", "D6", "C6", "B5", "A5", "G5",
            // Развитие с переходом в верхний регистр
            "F5", "A5", "C6", "F6", "E6", "D6", "C6", "B5", "G5", "E5",
            // Кульминация на высоких нотах
            "A5", "C6", "E6", "A6", "G6", "F6", "E6", "D6", "B5", "G5",
            // Плавный спуск
            "C6", "E6", "G6", "C7", "B6", "A6", "G6", "F6", "E6", "D6",
            // Завершение и подготовка к зацикливанию
            "C6", "G5", "E5", "C5", "G5", "B5", "C6", "E6", "G6", "C7"
        };

        private SoundHelper(SoundManager soundManager, GeneralSoundData soundData, SignalBus signalBus) {
            _soundManager = soundManager;
            _generalSoundData = soundData;
            _signalBus = signalBus;

            _soundManager.OnMusicStateChanged += OnMusicStateChanged;

            _clickMelodyMap = new Dictionary<string, SoundInfo>();
            foreach (var rocketSound in _generalSoundData.rocketSounds) {
                var name = rocketSound.clip.name;
                _clickMelodyMap.TryAdd(name, rocketSound);
            }
        }

        public void Initialize() {
            PlayBgMusic();

            _signalBus.Subscribe<GameOverSignal>(OnGameOverCallback);
        }

        public void Dispose() {
            _signalBus.Unsubscribe<GameOverSignal>(OnGameOverCallback);
            _soundManager.OnMusicStateChanged -= OnMusicStateChanged;
        }

        private void OnMusicStateChanged(bool isOn) {
            Debug.Log($"GM: OnMusicStateChanged, isOn: {isOn}");
            if (isOn) {
                PlayBgMusic();
            }
            else {
                StopBgMusic();
            }
        }

        public Audio PlayBgMusic() {
            StopBgMusic();
            _bgAudio = _soundManager.PlayMusic(_generalSoundData.mainBgMusic);
            return _bgAudio;
        }

        public void StopBgMusic() => _bgAudio?.Stop();

        private int i = 0;

        public void PlayRocketMove(float pitch) {
            pitch = 1;
            //var i = Random.Range(0, _generalSoundData.rocketSounds.Length);
            //var sound = _generalSoundData.rocketSounds[i % _generalSoundData.rocketSounds.Length];
            var name = _clickMelody[i % _clickMelody.Count];
            var sound = _clickMelodyMap[name];
            i++;
            sound.volume = 0.45f;
            _soundManager.PlaySound(sound, pitch);
        }

        public void PlayBumpSound() {
            _soundManager.PlaySound(_generalSoundData.BumpSound);
        }

        private void OnGameOverCallback(GameOverSignal sig) {
            _soundManager.PlaySound(_generalSoundData.LoseSound);
        }

        public void EnergyStartMoveSound() {
            _soundManager.PlaySound(_generalSoundData.EnergyStartMoveSound);
        }

        public void PlayEnergyHitSound() {
            _soundManager.PlaySound(_generalSoundData.EnergyHitMoveSound);
        }

        public void PlayEnergyFromUFO() {
            _soundManager.PlaySound(_generalSoundData.EnergyFromUFO);
        }

        public void PlayCongratsText() {
            _soundManager.PlaySound(_generalSoundData.WinSound);
        }

        public void DotsCollectMoveSound() {
            _soundManager.PlaySound(_generalSoundData.DotsCollectSound);
        }

        public void DotsWaveSound() {
            _soundManager.PlaySound(_generalSoundData.DotsWaveSound);
        }
    }
}