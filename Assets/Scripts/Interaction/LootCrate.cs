using Items;
using UnityEngine;

namespace Interaction {
    public class LootCrate : Interactible {
        public string crateName;
        public LootTable table;

        public bool destroyOnOpen;

        public LayerMask solidLayer;

        public GameObject weaponPickup;
        public GameObject foodPickup;
        public GameObject primaryAmmo;

        private bool _looted;

        private Vector3 _initialPosition;

        private void Start() {
            hookDestroy = false;
            table.ValidateTable();
            base.Start();

            _initialPosition = transform.position;
            if (!destroyOnOpen && PlayerState.Instance.lootedOverall.Contains(_initialPosition))
                _looted = true;
        }

        private Vector3 PickSpawnLocation(out bool success) {
            for (var attempt = 0; attempt < 15; attempt++) {
                var x = Random.Range(-2f, 2f);
                var y = Random.Range(-2f, 2f);

                if (x == 0 && y == 0) continue;
                var check = Physics2D.OverlapCircle(transform.position + new Vector3(x, y, 0), 0.2f, solidLayer);
                if (check != null) continue;

                success = true;
                return new Vector3(x, y, 0);
            }

            success = false;
            return new Vector3();
        }

        public override bool OnInteract() {
            base.OnInteract();
            PlayerState.Instance.lootCount++;
            
            // Get drops
            table.Get(out var food, out var weapon, out var ammo);
            
            // Spawn the weapon
            if (weapon != null) {
                // Pick location
                var pos = PickSpawnLocation(out var success);
                if (success) {
                    var pickup = Instantiate(weaponPickup, transform.position + pos, Quaternion.identity);
                    var wepPick = pickup.GetComponent<WeaponPickup>();
                    wepPick.weapon = weapon;
                    wepPick.ammo = weapon.startingAmmo;                    
                }
            }

            if (food != null) {
                // Pick location
                var pos = PickSpawnLocation(out var success);
                if (success) {
                    var pickup = Instantiate(foodPickup, transform.position + pos,
                                             Quaternion.identity);
                    var wepPick = pickup.GetComponent<FoodPickup>();
                    wepPick.SetFood(food);
                }
            }

            if (ammo) {
                // Pick location
                var pos = PickSpawnLocation(out var success);
                if (success)
                    Instantiate(primaryAmmo, transform.position + pos, Quaternion.identity);
            }

            if (!destroyOnOpen)
                PlayerState.Instance.lootedThisLife.Add(_initialPosition);

            _looted = true;
            if (destroyOnOpen)
                Destroy(gameObject);
            return true;
        }

        public override bool CanInteract() {
            return !_looted;
        }

        public override string GetInteractText() {
            return "loot " + crateName;
        }
    }
}