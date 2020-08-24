using UnityEngine;

namespace Interaction {
    public class AmmoPickup : Interactible {
        private void Start() {
            hookDestroy = false;
            base.Start();
        }
        public override bool OnInteract() {
            base.OnInteract();
            var currentState = PlayerState.Instance.currentState;
            var controller = PlayerController.Instance;

            // Primary gun ammo
            currentState.primaryAmmo = currentState.primaryWeapon.startingAmmo;
            currentState.primaryClip =
                Mathf.Clamp(currentState.primaryClip + currentState.primaryWeapon.replenishClip, 0,
                            currentState.primaryWeapon.clipSize);

            // Secondary if we have it
            if (currentState.secondaryWeapon != null) {
                currentState.secondaryAmmo = currentState.secondaryWeapon.startingAmmo;
                currentState.secondaryClip =
                    Mathf.Clamp(currentState.secondaryClip + currentState.secondaryWeapon.replenishClip, 0,
                                currentState.secondaryWeapon.clipSize);
            }
            
            controller.UpdateWeaponUI();
            Destroy(gameObject);
            return true;
        }

        public override bool CanInteract() {
            var currentState = PlayerState.Instance.currentState;
            return  currentState.primaryAmmo < currentState.primaryWeapon.startingAmmo ||
                    (currentState.secondaryWeapon != null && currentState.secondaryAmmo < currentState.secondaryWeapon.startingAmmo) ||
                    currentState.primaryClip < currentState.primaryWeapon.clipSize ||
                    (currentState.secondaryWeapon != null && currentState.secondaryClip < currentState.secondaryWeapon.clipSize);
        }

        public override string GetInteractText() {
            return "refill ammo";
        }
    }
}