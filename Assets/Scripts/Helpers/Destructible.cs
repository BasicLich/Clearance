using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Helpers {
    public class Destructible : MonoBehaviour {
        public bool hookDestroy = true;
        private Vector3 _firstPos;
        private bool _destroyed;

        protected void Start() {
            _firstPos = transform.position;
            if (hookDestroy && PlayerState.Instance.destroyedOverall.Contains(_firstPos)) {
                _destroyed = true;
                Destroy(gameObject);
            }
        }
        
        private void OnDestroy() {
            if (_destroyed || !hookDestroy) return;
            PlayerState.Instance.destroyedThisLife.Add(_firstPos);
            _destroyed = true;
        }
    }
}