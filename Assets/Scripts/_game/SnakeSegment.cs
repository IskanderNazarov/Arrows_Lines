using UnityEngine;

namespace _game {
    public class SnakeSegment : MonoBehaviour {
        [SerializeField] private SpriteRenderer _renderer;
        // Collider should be added to the prefab in Editor
    
        public void SetColor(Color color) {
            if (_renderer != null) _renderer.color = color;
        }
    }
}