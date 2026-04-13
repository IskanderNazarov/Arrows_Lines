using UnityEngine;

namespace _ScriptableObjects {
    [CreateAssetMenu(fileName = "config", menuName = "Scriptable/SnakesConfig", order = 5)]
    public class SnakesGlobalConfig : ScriptableObject {
        [Header("Palette")]
        public Color[] snakeColors;
        public Color bumpColor = Color.red;
        public Material snakeNeckMaterial;
        public Material snakeTailMaterial;
        public Sprite[] rocketsSprites;
    }
}