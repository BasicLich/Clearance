using Items;
using UnityEngine;

namespace Interaction {
    public class WeaponPickup : Interactible {
        public Weapon weapon;
        public int ammo;
        public int clip;

        public void Start() {
            hookDestroy = false;
            if (weapon == null) {
                Debug.LogError("WEAPON WAS NULL!");
                return;
            }
            
            GetComponent<SpriteRenderer>().sprite = weapon.sprite;
            ammo = weapon.startingAmmo;
            clip = weapon.clipSize - ammo;

            base.Start();
        }

        public override bool OnInteract() {
            if (weapon == null) {
                Debug.LogError("WEAPON WAS NULL!");
                return false;
            }
            
            base.OnInteract();
            
            // Get the players current loadout
            var state = PlayerState.Instance;
            Weapon oldWeapon;
            int oldAmmo;
            int oldClip;
            
            if (!weapon.secondary) {
                oldWeapon = state.currentState.primaryWeapon;
                oldAmmo = state.currentState.primaryAmmo;
                oldClip = state.currentState.primaryClip;
                
                // Set weapon and ammo
                PlayerController.Instance.SetPrimary(weapon, ammo, clip);
            } else {
                oldWeapon = state.currentState.secondaryWeapon;
                oldAmmo = state.currentState.secondaryAmmo;
                oldClip = state.currentState.secondaryClip;
                
                // Set weapon and ammo
                PlayerController.Instance.SetSecondary(weapon, ammo, clip);
            }

            // Store old details
            weapon = oldWeapon;
            ammo = oldAmmo;
            clip = oldClip;
            
            if (weapon == null)
                Destroy(gameObject);
            else {
                // Move to players feet (allows them to move things around easier)
                transform.position = PlayerController.Instance.transform.position;

                // Update sprite
                GetComponent<SpriteRenderer>().sprite = weapon.sprite;
            }

            return false;
        }

        public override string GetInteractText() {
            return "swap weapon";
        }
    }
}