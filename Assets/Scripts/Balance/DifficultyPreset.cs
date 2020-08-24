using UnityEngine;

namespace DefaultNamespace.Balance {
    [CreateAssetMenu(fileName = "difficulty.asset", menuName = "Difficulty Preset", order = 0)]
    public class DifficultyPreset : ScriptableObject {
        public float costScale = 1f;
        public float healthScale = 1f;
        public float bulletScale = 1f;
        public float timeoutScale = 1f;
        public float reloadScale = 1f;
    }
}