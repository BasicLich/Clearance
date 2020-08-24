using UnityEngine;
using UnityEngine.Tilemaps;

namespace Helpers {
    public class DisableTilemapRenderer : MonoBehaviour {
        private void Start() {
            GetComponent<TilemapRenderer>().enabled = false;
        }
    }
}