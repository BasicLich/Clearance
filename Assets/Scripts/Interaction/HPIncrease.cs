using UnityEngine;

namespace Interaction {
    public class HPIncrease : Interactible {
        private bool _used;
        private void Start() {
            hookDestroy = true;
            base.Start();
        }
        
        public override bool OnInteract() {
            if (_used) {
                Destroy(gameObject);
                return true;
            }
            base.OnInteract();
            var state = PlayerState.Instance;
            state.currentState.maxHP = Mathf.Clamp(state.currentState.maxHP + 2, 0, PlayerState.AbsoluteMaxHP);
            state.hitpoints = state.currentState.maxHP;
            _used = true;
            PlayerController.Instance.UpdateHealthUI();
            
            Destroy(gameObject);
            return true;
        }

        public override string GetInteractText() {
            return "increase max hp";
        }
    }
}