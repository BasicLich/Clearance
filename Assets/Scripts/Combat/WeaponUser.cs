using Helpers;
using Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace Combat {
    /// <summary>
    /// Attaches to a weapon user. Ammo will be tracked in the player only.
    /// </summary>
    public class WeaponUser : MonoBehaviour {
        public Weapon primary;
        public Weapon secondary;

        public SpriteRenderer weaponRenderer;
        public Animator weaponAnimator;

        public AudioClip shoot;
        public AudioClip meleeHit;

        public bool isGoodGuy;
        public LayerMask player;
        public LayerMask enemy;
        public LayerMask companion;

        public float timeoutMultiplier = 1;

        public float primaryTimer;
        public float secondaryTimer;

        private AudioSource _audioSource;

        private void Start() {
            if (primary != null && weaponRenderer != null) 
                weaponRenderer.sprite = primary.sprite;
            _audioSource = GetComponent<AudioSource>();
        }

        public void SetPrimary(Weapon weapon) {
            primary = weapon;
            primaryTimer = 0;
            if (weaponRenderer != null) weaponRenderer.sprite = weapon.sprite;
        }

        public void SetSecondary(Weapon weapon) {
            secondary = weapon;
            secondaryTimer = 0;
        }

        public bool FirePrimary(Rigidbody2D user, Vector3 position, Vector3 direction) {
            if (primary != null && primaryTimer <= 0) {
                if (weaponAnimator != null) weaponAnimator.SetTrigger("Fire");
                Fire(user, primary, position, direction);
                primaryTimer = primary.usetime * timeoutMultiplier * DifficultOwner.Instance.currentPreset.timeoutScale;
                if (weaponRenderer != null) weaponRenderer.sprite = primary.sprite;
                return true;
            }

            return false;
        }

        public bool FireSecondary(Rigidbody2D user, Vector3 position, Vector3 direction) {
            if (secondary != null && secondaryTimer <= 0) {
                if (weaponAnimator != null) weaponAnimator.SetTrigger("Fire");
                Fire(user, secondary, position, direction);
                secondaryTimer = secondary.usetime * timeoutMultiplier * DifficultOwner.Instance.currentPreset.timeoutScale;
                if (weaponRenderer != null) weaponRenderer.sprite = secondary.sprite;
                return true;
            }

            return false;
        }

        private void Fire(Rigidbody2D user, Weapon weapon, Vector3 position, Vector3 direction) {
            if (weapon.isGun) {
                _audioSource.PlayOneShot(shoot);
                // Create bullet
                var bullet = Instantiate(weapon.bulletPrefab, position, Quaternion.identity);

                if (weapon.bulletsHaveContainer) {
                    // Configure rotation
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
                    bullet.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angle));

                    // Get bullets
                    var bullets = bullet.GetComponentsInChildren<Rigidbody2D>();

                    // Setup each bullet
                    foreach (var b in bullets) {
                        var bulletComp = b.GetComponent<Bullet>();

                        bulletComp.goodBullet = isGoodGuy;

                        b.transform.localRotation = Quaternion.Euler(b.transform.localRotation.x,
                                                                     b.transform.localRotation.y,
                                                                     -b.transform.localRotation.z +
                                                                     bulletComp.rotationOffset);

                        b.velocity = b.transform.up * weapon.velocity;

                        Physics2D.IgnoreCollision(b.GetComponent<Collider2D>(), GetComponent<Collider2D>());

                        if (bulletComp.goodBullet) {
                            // Get all companions and ignore them
                            var companions = FindObjectsOfType<CompanionAI>();
                            foreach (var companion in companions) {
                                Physics2D.IgnoreCollision(b.GetComponent<Collider2D>(), companion.GetComponent<Collider2D>());
                            }
                        }

                        b.transform.SetParent(null, true);
                    }

                    // Now delete container
                    Destroy(bullet);
                } else {
                    var bulletComp = bullet.GetComponent<Bullet>();
                    bulletComp.goodBullet = isGoodGuy;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90 +
                                  bulletComp.rotationOffset;
                    bullet.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angle));
                    bullet.GetComponent<Rigidbody2D>().velocity = direction * weapon.velocity;
                    Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), GetComponent<Collider2D>());
                    
                    if (bulletComp.goodBullet) {
                        // Get all companions and ignore them
                        var companions = FindObjectsOfType<CompanionAI>();
                        foreach (var companion in companions) {
                            Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), companion.GetComponent<Collider2D>());
                        }
                    }
                }
                
                // Recoil
                user.AddForce(-direction.normalized * (weapon.recoil), ForceMode2D.Impulse);
            } else {
                _audioSource.PlayOneShot(meleeHit);
                var filter = companion; // Companions
                if (isGoodGuy)
                    filter |= enemy; // enemies
                else
                    filter |= player; // player
                
                var hit = Physics2D.Raycast(position, direction, weapon.meleeReach, filter);
                if (hit.collider != null) {
                    if (isGoodGuy) {
                        if (hit.collider.CompareTag("Enemy")) {
                            hit.collider.gameObject.GetComponent<EnemyAI>().Damage(weapon.meleeDamage);
                            hit.rigidbody.AddForce(direction.normalized * weapon.meleeKnockbackImpulse, ForceMode2D.Impulse);
                        }
                    } else {
                        if (hit.collider.CompareTag("Player")) {
                            hit.collider.GetComponent<PlayerController>().Damage(weapon.meleeDamage);
                            hit.rigidbody.AddForce(direction.normalized * weapon.meleeKnockbackImpulse, ForceMode2D.Impulse);
                        } else if (hit.collider.CompareTag("Companion")) {
                            hit.collider.GetComponent<CompanionAI>().Damage(weapon.meleeDamage);
                            hit.rigidbody.AddForce(direction.normalized * weapon.meleeKnockbackImpulse, ForceMode2D.Impulse);
                        }
                    }
                }
            }
        }

        public void PointGun(Vector3 target, bool flipped) {
            if (weaponRenderer == null) return;
            var d = new Vector3(target.x, target.y, 0) -
                    transform.position;
            var tilt = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            if (flipped) {
                tilt = (tilt - 180) * -1f;
            }

            weaponRenderer.transform.localRotation = Quaternion.AngleAxis(tilt, Vector3.forward);
        }

        public void StopPointGun() {
            if (weaponRenderer == null) return;
            weaponRenderer.transform.localRotation = Quaternion.AngleAxis(0, Vector3.forward);
        }

        private void Update() {
            // Timings
            if (primaryTimer > 0)
                primaryTimer -= Time.deltaTime;
            if (secondaryTimer > 0)
                secondaryTimer -= Time.deltaTime;
        }
    }
}