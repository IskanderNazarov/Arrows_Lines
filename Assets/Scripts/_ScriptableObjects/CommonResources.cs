using _Effects;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Infrastructure {
    [CreateAssetMenu(fileName = "config", menuName = "Scriptables/Common resources", order = 5)]
    public class CommonResources : ScriptableObject {
        [FormerlySerializedAs("coinIcon")] public Sprite gemIcon;
        public AddResourceInfoPanel addResourceInfoPanel;
        public Sprite itemsCatalogDefIcon;
    }
}