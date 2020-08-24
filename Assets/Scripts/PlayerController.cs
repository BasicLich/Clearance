using System;
using System.Collections;
using System.Collections.Generic;
using Combat;
using Helpers;
using Interaction;
using Items;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(Animator)), RequireComponent(typeof(WeaponUser))]
public class PlayerController : MonoBehaviour {
    public float moveSpeed;

    public Weapon defaultWeapon;

    public CircleCollider2D interactCircle;

    public SpriteRenderer modelRenderer;

    public Sprite noGunIcon;
    
    public Material invulnerableMaterial;

    public GameObject bloodFX;

    public static PlayerController Instance;
    
    private UICollector _ui;

    [FormerlySerializedAs("_inputVector")] public Vector2 inputVector;
    [FormerlySerializedAs("_rb")] public Rigidbody2D rb;
    private WeaponUser _weaponUser;

    private Animator _animator;
    
    private List<Interactible> _interactibles;
    private Interactible _lastOutlined;

    private float _primaryReloadTimer;
    private bool _primaryReload;
    private float _secondaryReloadTimer;
    private bool _secondaryReload;

    private float _invulnerabilityTimer;
    private Material _normalMaterial;

    public void Awake() {
        Instance = this;
    }

    public void Hit(float angle) {
        Instantiate(bloodFX, transform.position, Quaternion.Euler(new Vector3(angle - 90, -90, 0)));
    }

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _weaponUser = GetComponent<WeaponUser>();
        _interactibles = new List<Interactible>();
        _ui = GameManager.Instance.hud;
        
        var state = PlayerState.Instance;
        
        if (state.hasCheckpoint) {
            transform.position = state.lastCheckpoint;
        }

        if (state.currentStateInvalid) {
            if (state.hasLevelStartState && !state.hasCheckpoint)
                state.currentState = new CurrentPlayerState(state.enteredSaveState);
            else state.currentState = new CurrentPlayerState(state.lastSaveState);
        } else if (state.currentState.primaryWeapon == null) {
            SetPrimary(defaultWeapon, defaultWeapon.startingAmmo, defaultWeapon.clipSize - defaultWeapon.startingAmmo);
            SetSecondary(null, 0, 0);
            #if !DEBUG
            state.currentState.maxHP = 6;
            #endif
        }
        
        // Now save level enter state
        state.hasLevelStartState = true;
        state.enteredSaveState = new CurrentPlayerState(state.currentState);
        
        UpdateWeaponUI();
        
        state.hitpoints = state.currentState.maxHP;
        _ui.CreateHearts((int) Mathf.Ceil(state.currentState.maxHP * 0.5f));
        UpdateHealthUI();

        RespawnCompanions();

        _ui.gameObject.SetActive(true);

        GameManager.Instance.InitObjectiveCount();

        _normalMaterial = modelRenderer.material;
    }

    public void SummonCompanions() {
        // Summon all
        var companions = FindObjectsOfType<CompanionAI>();
        foreach (var companion in companions)
            companion.transform.position = transform.position;
    }
    
    public void RespawnCompanions() {
        // Destroy any existing
        var companions = FindObjectsOfType<CompanionAI>();
        foreach (var companion in companions)
            Destroy(companion.gameObject);
        
        // Respawn companions
        foreach (var companion in PlayerState.Instance.savedOverall) {
            var c = Instantiate(companion.prefab, transform.position, Quaternion.identity);
            c.GetComponent<CompanionAI>().myPrefab = companion.prefab;
        }
        
        // Carryovers
        if (PlayerState.Instance.companionPrefabCarryover != null)
            foreach (var companion in PlayerState.Instance.companionPrefabCarryover) {
                if (companion != null) {
                    var c = Instantiate(companion, transform.position, Quaternion.identity);
                    c.GetComponent<CompanionAI>().myPrefab = companion;
                }
            }
        else {
            Debug.LogError("THE THING HAPPENED, IM SO SORRY.");
        }
    }

    public void SetPrimary(Weapon weapon, int ammo, int clip) {
        var currentState = PlayerState.Instance.currentState;
        currentState.primaryWeapon = weapon;
        currentState.primaryAmmo = ammo;
        currentState.primaryClip = clip;
        UpdateWeaponUI();
    }

    public void SetSecondary(Weapon weapon, int ammo, int clip) {
        var currentState = PlayerState.Instance.currentState;
        currentState.secondaryWeapon = weapon;
        currentState.secondaryAmmo = ammo;
        currentState.secondaryClip = clip;
        UpdateWeaponUI();
    }

    public void UpdateWeaponUI() {
        var state = PlayerState.Instance;
        _ui.primaryGunImage.sprite = state.currentState.primaryWeapon.sprite;
        _ui.primaryAmmoText.text = state.currentState.primaryAmmo + " : " + state.currentState.primaryClip;

        if (state.currentState.secondaryWeapon != null) {
            _ui.secondaryGunImage.sprite = state.currentState.secondaryWeapon.sprite;
            _ui.secondaryAmmoText.text = state.currentState.secondaryAmmo + " : " + state.currentState.secondaryClip;
        } else {
            _ui.secondaryGunImage.sprite = noGunIcon;
            _ui.secondaryAmmoText.text = "???";
        }
    }

    public void ReloadPrimary() {
        var currentState = PlayerState.Instance.currentState;
        currentState.primaryAmmo = Mathf.Clamp(currentState.primaryWeapon.startingAmmo, 0, currentState.primaryClip);
        currentState.primaryClip = Mathf.Clamp(currentState.primaryClip - currentState.primaryAmmo, 0,
                                               currentState.primaryWeapon.clipSize);
        UpdateWeaponUI();
    }

    public void ReloadSecondary() {
        var currentState = PlayerState.Instance.currentState;
        if (currentState.secondaryWeapon == null) return;
        currentState.secondaryAmmo = Mathf.Clamp(currentState.secondaryWeapon.startingAmmo, 0, currentState.secondaryClip);
        currentState.secondaryClip = Mathf.Clamp(currentState.secondaryClip - currentState.secondaryAmmo, 0,
                                                 currentState.secondaryWeapon.clipSize);
        UpdateWeaponUI();
    }

    public void CompleteRefill() {
        var currentState = PlayerState.Instance.currentState;
        currentState.primaryAmmo = currentState.primaryWeapon.startingAmmo;
        currentState.primaryClip = currentState.primaryWeapon.clipSize;
        if (currentState.secondaryWeapon != null) {
            currentState.secondaryAmmo = currentState.secondaryWeapon.startingAmmo;
            currentState.secondaryClip = currentState.secondaryWeapon.clipSize;
        }

        UpdateWeaponUI();
    }

    public void Damage(int damage) {
        if (_invulnerabilityTimer >= 0)
            return;
        PlayerState.Instance.hitpoints -= damage;
        UpdateHealthUI();

        _invulnerabilityTimer = 1.5f;
        modelRenderer.material = invulnerableMaterial;

        if (PlayerState.Instance.hitpoints <= 0) {
            GameManager.Instance.Died();
            PlayerState.Instance.deaths++;
        }
    }

    public void Heal(int health) {
        var state = PlayerState.Instance;
        state.hitpoints = Mathf.Clamp(state.hitpoints + health, 0, state.currentState.maxHP);
        UpdateHealthUI();
    }

    public void UpdateHealthUI() {
        var state = PlayerState.Instance;
        
        // Check for new hearts
        var heartCount = (int) Mathf.Ceil(state.currentState.maxHP * 0.5f);
        if (_ui.hearts.Count < heartCount)
            _ui.CreateHearts(heartCount - _ui.hearts.Count);
        
        // Set initial health
        for (int i = 0; i < heartCount; i++) {
            if (state.hitpoints >= i * 2 + 2) {
                _ui.hearts[i].sprite = _ui.heartFull;
            } else if (state.hitpoints == i * 2 + 1) {
                _ui.hearts[i].sprite = _ui.heartHalf;
            } else _ui.hearts[i].sprite = _ui.heartEmpty;
        }
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.CompareTag("Interactible") && (other.gameObject.transform.position - transform.position).magnitude <= interactCircle.radius) {
            var interactible = other.GetComponent<Interactible>();
            if (!_interactibles.Contains(interactible))
                _interactibles.Add(interactible);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.CompareTag("Interactible")) {
            _interactibles.Remove(other.GetComponent<Interactible>());
        }
    }

    private Interactible GetClosestInteractible(out bool present) {
        if (_interactibles.Count > 0) {
            var myPos = transform.position;

            var shortestDist = Mathf.Infinity;
            Interactible closest = null;
            foreach (var i in _interactibles) {
                var dist = (i.transform.position - myPos).magnitude;
                if (dist < shortestDist && i.CanInteract()) {
                    shortestDist = dist;
                    closest = i;
                }
            }

            present = closest != null;
            return closest;
        }
        
        present = false;
        return null;
    }

    private void FixedUpdate() {
        rb.velocity = inputVector * moveSpeed;

        if (_invulnerabilityTimer >= 0) {
            _invulnerabilityTimer -= Time.deltaTime;
        } else if (_invulnerabilityTimer > -1f) {
            _invulnerabilityTimer = -1;
            modelRenderer.material = _normalMaterial;
        }

        var interact = GetClosestInteractible(out var has);
        _ui.interactControl.SetActive(has);
        if (has) {
            if (interact != _lastOutlined && _lastOutlined != null)
                _lastOutlined.StopOutline();
            _lastOutlined = interact;
            interact.Outline();
            _ui.interactText.text = interact.GetInteractText();
        } else if (_lastOutlined != null) {
            _lastOutlined.StopOutline();
            _lastOutlined = null;
        }
        
        // Check for weapon changes, if changed, sync
        var state = PlayerState.Instance;
        if (state.currentState.primaryWeapon != _weaponUser.primary) {
            _weaponUser.SetPrimary(state.currentState.primaryWeapon);
            UpdateWeaponUI();
        }

        if (state.currentState.secondaryWeapon != _weaponUser.secondary) {
            _weaponUser.SetSecondary(state.currentState.secondaryWeapon);
            UpdateWeaponUI();
        }
        
        // Reload
        if (_primaryReload && _primaryReloadTimer >= 0) {
            _primaryReloadTimer -= Time.deltaTime;
        } else if (_primaryReload) {
            _primaryReload = false;
            ReloadPrimary();
        }
        if (_secondaryReload && _secondaryReloadTimer >= 0) {
            _secondaryReloadTimer -= Time.deltaTime;
        } else if (_secondaryReload) {
            _secondaryReload = false;
            ReloadSecondary();
        }

        // Weapons
        if (!GameManager.Instance.inDialogue) {
            try {
                var mouse = Mouse.current;
                var keyboard = Keyboard.current;
                var mousePos = mouse.position.ReadValue();
                if (mouse.leftButton.isPressed || keyboard[Key.N].isPressed) {
                    if (state.currentState.primaryAmmo > 0) {
                        var direction = new Vector3(mousePos.x, mousePos.y) -
                                        Camera.main.WorldToScreenPoint(transform.position);
                        direction.Normalize();

                        if (_weaponUser.FirePrimary(rb, transform.position, direction)) {
                            state.currentState.primaryAmmo--;
                            UpdateWeaponUI();
                        }
                    }
                    else if (state.currentState.primaryClip > 0 && !_primaryReload) {
                        _primaryReload = true;
                        _primaryReloadTimer = state.currentState.primaryWeapon.reloadTime * DifficultOwner.Instance.currentPreset.reloadScale;
                    }
                }
                else if (mouse.rightButton.isPressed || keyboard[Key.M].isPressed) {
                    if (state.currentState.secondaryAmmo > 0) {
                        var direction = new Vector3(mousePos.x, mousePos.y) -
                                        Camera.main.WorldToScreenPoint(transform.position);
                        direction.Normalize();

                        if (_weaponUser.FireSecondary(rb, transform.position, direction)) {
                            state.currentState.secondaryAmmo--;
                            UpdateWeaponUI();
                        }
                    }
                    else if (state.currentState.secondaryClip > 0 && !_secondaryReload) {
                        _secondaryReload = true;
                        _secondaryReloadTimer = state.currentState.secondaryWeapon.reloadTime * DifficultOwner.Instance.currentPreset.reloadScale;
                    }
                }

                // Mouse flippage
                var mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

                var flipped = false;
                if (mouseWorldPos.x < transform.position.x) {
                    if (!_leftTriggered) {
                        _animator.SetTrigger("FlipLeft");
                        _leftTriggered = true;
                        _animator.ResetTrigger("FlipRight");
                        _rightTriggered = false;
                    }

                    flipped = true;
                }
                else if (mouseWorldPos.x > transform.position.x) {
                    if (!_rightTriggered) {
                        _animator.SetTrigger("FlipRight");
                        _rightTriggered = true;
                        _animator.ResetTrigger("FlipLeft");
                        _leftTriggered = false;
                    }
                }

                _weaponUser.PointGun(mouseWorldPos, flipped);
            } catch (Exception ex) {
                
            }
        }
    }

    private bool _leftTriggered;
    private bool _rightTriggered = true;

    public void Move(InputAction.CallbackContext ctx) {
        if (GameManager.Instance.inDialogue) return;
        inputVector = ctx.ReadValue<Vector2>();
        _animator.SetBool("Walking", inputVector.magnitude != 0);
    }

    public void CompanionFollowChange(InputAction.CallbackContext ctx) {
        if (GameManager.Instance.inDialogue) return;
        if (ctx.performed) {
            var state = PlayerState.Instance;
            state.companionFollow = !state.companionFollow;

            _ui.companionState.text = "companions " + (state.companionFollow ? "following" : "patrolling");
        }
    }

    public void Interact(InputAction.CallbackContext ctx) {
        if (GameManager.Instance.inDialogue) return;
        if (ctx.performed) {
            var interactible = GetClosestInteractible(out var has);
            if (has) {
                if (interactible.OnInteract()) {
                    _interactibles.Remove(interactible);
                }
            }
        }
    }

    public void Pause(InputAction.CallbackContext ctx) {
        if (GameManager.Instance.inDialogue) return;
        if (ctx.performed) {
            GameManager.Instance.TogglePause();
        }
    }

    public void PostDialogeHotfix() {
        _weaponUser.primaryTimer = 0.1f;
        _weaponUser.secondaryTimer = 0.1f;
    }
}
