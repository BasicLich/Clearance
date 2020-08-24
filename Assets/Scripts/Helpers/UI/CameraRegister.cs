using UnityEngine;

namespace Helpers.UI {
    public class CameraRegister : MonoBehaviour {
        public Camera uiCamera;
        private void Awake() {
            GameManager.Instance.uiCamera = uiCamera;
        }
    }
}