using System.Collections.Generic;
using UnityEngine;

namespace __CoreGameLib._Scripts._ScriptableObjects {
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ProjectSettings", order = 10)]
    public class ProjectSettings : ScriptableObject {
        
        public SDK_Type SDKType = SDK_Type.Playgama;
        public string[] PublicKeysFor_GP;
        public int FirstInterstitialTime = 60;
        public int minimumDelayBetweenInterstitial = 60;
        
    }
    
    public enum SDK_Type {
        Playgama, 
        GamePush
    }
}