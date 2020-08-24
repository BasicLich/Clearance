using UnityEngine;

namespace Items {
    [CreateAssetMenu(fileName = "food.asset", menuName = "Items/Food")]
    public class Food : ScriptableObject {
        public string displayName;
        public Sprite sprite;
        public int health;
    }
}