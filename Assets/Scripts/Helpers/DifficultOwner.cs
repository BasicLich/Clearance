using DefaultNamespace.Balance;
using UnityEngine;

namespace Helpers {
    public class DifficultOwner : MonoBehaviour {
        public DifficultyPreset currentPreset;

        public static DifficultOwner Instance;

        private void Awake() {
            if (Instance == null) {
                DontDestroyOnLoad(gameObject);
                Instance = this;
            } else if (Instance != this)
                Destroy(gameObject);
        }
    }
}