using UnityEngine;
using UnityEngine.UI;

namespace Helpers.UI {
    public class AttachUIToCamera : MonoBehaviour {
        private void Start() {
            GetComponent<Canvas>().worldCamera = GameManager.Instance.uiCamera;
        }
    }
}