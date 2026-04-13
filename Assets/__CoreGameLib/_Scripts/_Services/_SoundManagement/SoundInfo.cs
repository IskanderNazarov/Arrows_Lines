using System;
using UnityEngine;

namespace Scriptable {
    namespace SoundManaging {
        [Serializable]
        public class SoundInfo {
            public AudioClip clip;
            [Range(0, 1)] public float volume = 1f;
            public bool loop;
            public float Delay;
            public float Pitch = 1;

            public SoundInfo() {
                volume = 1;
            }
        }
    }
}