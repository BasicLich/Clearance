using UnityEngine;
using UnityEngine.SceneManagement;

namespace Helpers {
    public class DontDestroyOnLoad : MonoBehaviour {
        public int unlessOnScene = -1;

        private static DontDestroyOnLoad _me;
        
        private void Awake() {
            if (_me == null) {
                DontDestroyOnLoad(gameObject);
                _me = this;
            } else if (_me != this) {
                Destroy(gameObject);
            }

            if (unlessOnScene >= 0) {
                if (SceneManager.GetActiveScene().buildIndex == unlessOnScene)
                    Destroy(gameObject);
            }
        }
    }
}