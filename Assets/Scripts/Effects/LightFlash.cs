using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace Fog {
    public class LightFlash : MonoBehaviour {
        public float minIntensity;
        public float maxIntensity;
        
        /// <summary>
        /// For non-erratic, higher = faster
        /// for erratic, lower = faster
        /// </summary>
        public float changeSpeed;

        public bool erratic;
        
        // TODO: More believable flicker effect.

        private float _currentLuminance;
        private bool _min;

        private float _timer;

        private Light2D _light;

        private void Start() {
            _light = GetComponent<Light2D>();
        }

        private void Update() {
            if (erratic) {
                if (_timer >= changeSpeed) {
                    _timer = 0;
                    _light.intensity = Random.Range(minIntensity, maxIntensity);
                } else _timer += Time.deltaTime;
            } else {
                if (_min) {
                    _light.intensity += Time.deltaTime * changeSpeed;

                    if (_light.intensity >= maxIntensity)
                        _min = false;
                }
                else {
                    _light.intensity -= Time.deltaTime * changeSpeed;

                    if (_light.intensity <= minIntensity)
                        _min = true;
                }
            }
        }
    }
}