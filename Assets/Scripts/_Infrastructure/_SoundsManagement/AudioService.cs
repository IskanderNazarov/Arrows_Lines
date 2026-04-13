using System;
using System.Collections.Generic;
using Core._Services;
using Core._Services.SoundManagement;
using core.purchasing._SoundsManagement;
using UnityEngine;
using Zenject;

namespace Game.SoundManagement {
    public class AudioService : IInitializable, IDisposable {
        
        private readonly SoundManager _soundManager;
        private readonly SoundsDatabase _database;
        private readonly Dictionary<GameSoundId, SoundInfo> _soundsMap;
        
        private Hellmade.Sound.Audio _bgAudio;

        [Inject]
        public AudioService(SoundManager soundManager, SoundsDatabase database) {
            _soundManager = soundManager;
            _database = database;
            
            // Собираем быстрый словарь
            _soundsMap = new Dictionary<GameSoundId, SoundInfo>();
            foreach (var entry in _database.Entries) {
                _soundsMap.TryAdd(entry.Id, entry.Info);
            }
        }

        public void Initialize() {
            _soundManager.OnMusicStateChanged += OnMusicStateChanged;
            PlayBgMusic();
        }

        public void Dispose() {
            _soundManager.OnMusicStateChanged -= OnMusicStateChanged;
        }

        private void OnMusicStateChanged(bool isOn) {
            if (isOn) PlayBgMusic();
            else StopBgMusic();
        }

        public void PlayBgMusic() {
            StopBgMusic();
            if (_database.MainBgMusic != null) {
                _bgAudio = _soundManager.PlayMusic(_database.MainBgMusic);
            }
        }

        public void StopBgMusic() => _bgAudio?.Stop();

        // --- ГЛАВНЫЙ МЕТОД ---
        public void Play(GameSoundId soundId) {
            if (_soundsMap.TryGetValue(soundId, out var info)) {
                _soundManager.PlaySound(info);
            } else {
                Debug.LogWarning($"[AudioService] Sound {soundId} is missing in SoundsDatabase!");
            }
        }

        // --- МЕТОД ДЛЯ РАКЕТ (С ИЗМЕНЕНИЕМ ПИТЧА) ---
        // Использование: _audioService.PlayWithPitch(GameSoundId.RocketMove, 1.5f);
        public void PlayWithPitch(GameSoundId soundId, float customPitch) {
            if (_soundsMap.TryGetValue(soundId, out var info)) {
                _soundManager.PlaySound(info, customPitch); // Безопасно передаем питч
            }
        }
    }
}