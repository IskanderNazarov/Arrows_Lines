using Core._Services.SoundManagement;
using UnityEngine;

namespace SoundManaging {
    [CreateAssetMenu(fileName = "LocData", menuName = "ScriptableObjects/LocationsSoundsData", order = 1)]
    public class GeneralSoundData : ScriptableObject {
        public SoundInfo mainBgMusic;
        public SoundInfo BumpSound;
        public SoundInfo[] rocketSounds;
        public SoundInfo EnergyStartMoveSound;
        public SoundInfo EnergyHitMoveSound;
        public SoundInfo EnergyFromUFO;
        public SoundInfo WinSound;
        public SoundInfo LoseSound;
        public SoundInfo DotsCollectSound;
        public SoundInfo DotsWaveSound;
    }
}