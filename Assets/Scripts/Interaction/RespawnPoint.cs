using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace Interaction {
    public class RespawnPoint : Interactible {
        public GameObject fire;

        private Light2D _light;

        private void Start() {
            hookDestroy = false;
            base.Start();
            _light = GetComponent<Light2D>();
        }
        
        public override bool OnInteract() {
            base.OnInteract();
            var state = PlayerState.Instance;
            state.lastSaveState = new CurrentPlayerState(state.currentState);
            state.lastCheckpoint = transform.position;
            state.hasCheckpoint = true;
            _light.enabled = true;
            fire.SetActive(true);

            state.destroyedOverall.AddRange(state.destroyedThisLife);
            state.destroyedThisLife.Clear();
            state.killedOverall.AddRange(state.killedThisLife);
            state.killedThisLife.Clear();
            state.savedOverall.AddRange(state.savedThisLife);
            state.savedThisLife.Clear();
            state.companionsDiedOverall.AddRange(state.companionsDiedThisLife);
            state.companionsDiedThisLife.Clear();
            state.lootedOverall.AddRange(state.lootedThisLife);
            state.lootedThisLife.Clear();
            state.interactedOverall.AddRange(state.interactedThisLife);
            state.interactedThisLife.Clear();

            return true;
        }

        public override string GetInteractText() {
            return "checkpoint";
        }
    }
}