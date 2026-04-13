using System;
using __CoreGameLib._Scripts._Services._Saving;
using Core._Services._Saving;
using Hellmade.Sound;
using Scriptable.SoundManaging;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Core._Services {
    public class SoundManager {
        /*private bool isMusicEnable;
        private bool isSoundEnable;*/

        private bool isMusicInitialized;
        private bool isSFXInitialized;
        private IDataSaver _dataSaver;

        public static SoundManager shared { get; private set; }
        public Action<bool> OnMusicStateChanged;
        public Action<bool> OnSoundStateChanged;

        //------------------------------------------------------------------

        [Inject]
        private void Construct(IDataSaver dataSaver) {
            _dataSaver = dataSaver;
        }

        private SoundManager() {
            shared = this;

            EazySoundManager.StopAll();
            Initialize();
        }

        public void MuteSounds(bool mute) {
            EazySoundManager.GlobalVolume = mute ? 0 : 1.0f;
        }

        private void Initialize() {
        }

        /*private Audio mainBgAudio;
        private Audio secondBgAudio;

        public static void PlayBgMusic() {
            shared.mainBgAudio = PlayMusic(shared._generalSoundData.mainBgMusic);
            //PlayMusic(shared._generalSoundData.secondaryBgMusic);
        }

        public static void StopBgMusic() {
            shared.mainBgAudio?.Stop();
            shared.secondBgAudio?.Stop();
        }*/

        private Audio PlayGameAudio(bool canPlay, AudioClip clip, bool isLooped, float pitch, float volume = 1) {
            //if (!shared.IsSFXOn() || clip == null) return null;
            if (!canPlay || clip == null) return null;

            var id = EazySoundManager.PlaySound(clip, volume);
            var audio = EazySoundManager.GetAudio(id);
            audio.Loop = isLooped;
            audio.Pitch = pitch;
            return audio;
        }

        public Audio PlayMusic(SoundInfo soundInfo) {
            return PlayGameAudio(shared.IsMusicOn(), soundInfo.clip, soundInfo.loop, soundInfo.Pitch, soundInfo.volume);
        }

        public Audio PlaySound(SoundInfo soundInfo) {
            return PlayGameAudio(shared.IsSoundOn(), soundInfo.clip, soundInfo.loop, soundInfo.Pitch, soundInfo.volume);
        }

        //------------------------------------------------------------------
        public void StopAllSounds() {
            EazySoundManager.StopAllSounds();
        }

        //------------------------------------------------------------------
        public bool IsSoundOn() {
            return _dataSaver.GetDataBool(CoreKeys.SoundOnKey);
            
            /*if (!isSFXInitialized) {
                isSFXInitialized = true;
                isSoundEnable = _dataSaver.GetDataBool(CoreKeys.SoundOnKey);
            }

            return isSoundEnable;*/
        }

        //------------------------------------------------------------------
        public bool IsMusicOn() {
            return _dataSaver.GetDataBool(CoreKeys.MusicOnKey);
            Debug.Log($"IsMusicOn 1, isMusicInitialized: {isMusicInitialized}");
            /*if (!isMusicInitialized) {
                Debug.Log("IsMusicOn 2");
                isMusicInitialized = true;
                isMusicEnable = _dataSaver.GetDataBool(CoreKeys.MusicOnKey);
            }

            Debug.Log("IsMusicOn 3");


            return isMusicEnable;*/
        }

        //–––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––
        public void SetSFXOn(bool isOn) {
            //
            //PlayerPrefs.SetInt(IS_SFX_ON_KEY, isOn ? 1 : 0);
            _dataSaver.SetData(CoreKeys.SoundOnKey, isOn.ToString());
            OnSoundStateChanged?.Invoke(isOn);
        }

        //–––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––
        public void SetMusicOn(bool isOn) {
            _dataSaver.SetData(CoreKeys.MusicOnKey, isOn.ToString());
            OnMusicStateChanged?.Invoke(isOn);
        }
    }
}