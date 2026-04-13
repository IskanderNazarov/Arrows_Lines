using TMPro;
using UnityEngine;

namespace _game {
    public class AddResourceFX :MonoBehaviour {
        [SerializeField] private SpriteRenderer resourceIcon;
        [SerializeField] private TextMeshPro resourceCountTMP;
        
        public  SpriteRenderer ResourceIcon => resourceIcon;
        public  TextMeshPro ResourceCountTMP => resourceCountTMP;
    }
}