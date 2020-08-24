using UnityEngine;
using UnityEngine.Serialization;

namespace Items {
    [CreateAssetMenu(fileName = "weapon.asset", menuName = "Items/Weapon")]
    public class Weapon : ScriptableObject {
        public string name;
        public bool secondary;
        [FormerlySerializedAs("firetime")] public float usetime;
        public bool isGun;
        public int meleeDamage;
        public float meleeReach;
        public float meleeKnockbackImpulse;
        public float velocity;
        public Sprite sprite;
        public GameObject bulletPrefab;
        public bool bulletsHaveContainer;
        public int startingAmmo;
        public int clipSize;
        public int replenishClip;
        public float recoil;
        public float reloadTime;
    }
}