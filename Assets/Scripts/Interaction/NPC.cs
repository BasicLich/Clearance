using System.Collections.Generic;
using Helpers;
using UnityEngine;

namespace Interaction {
    public class NPC : Interactible {
        public NPCDialogue dialogue;
        public NPCDialogue refillUsedDialogue;
        public List<OpenDoor> unlocksDoors;
        public bool refillAndHeal;
        public bool autoStart;

        private Vector3 _initPos;

        public bool _usedRefill;

        private bool _spokenToOnce;
        
        private void Start() {
            hookDestroy = false;
            base.Start();

            _initPos = transform.position;

            if (!PlayerState.Instance.interactedOverall.Contains(_initPos)) {
                foreach (var door in unlocksDoors) {
                    door.locked = true;
                }

                if (autoStart)
                    OnInteract();
            }
            else _usedRefill = true;
        }

        public override bool OnInteract() {
            if (!autoStart || (autoStart && _spokenToOnce))
                base.OnInteract();
            _spokenToOnce = true;
            // Display dialogue.
            if (refillAndHeal && _usedRefill)
                GameManager.Instance.PlayDialogue(refillUsedDialogue);
            else GameManager.Instance.PlayDialogue(dialogue);
            
            // Unlock doors
            foreach (var door in unlocksDoors) {
                door.locked = false;
            }

            if (!_usedRefill && refillAndHeal) {
                var controller = PlayerController.Instance;
                controller.Heal(26);
                controller.CompleteRefill();
                _usedRefill = true;
            }

            if (!PlayerState.Instance.interactedThisLife.Contains(_initPos))
                PlayerState.Instance.interactedThisLife.Add(_initPos);
            
            return false;
        }

        public override string GetInteractText() {
            return "talk";
        }
    }
}