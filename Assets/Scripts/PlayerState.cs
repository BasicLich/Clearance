using System;
using System.Collections.Generic;
using DefaultNamespace.Balance;
using Interaction;
using Items;
using UnityEngine;

[Serializable]
public class CurrentPlayerState {
    public Weapon primaryWeapon;
    public int primaryAmmo;
    public int primaryClip;
    public Weapon secondaryWeapon;
    public int secondaryAmmo;
    public int secondaryClip;
    public int maxHP = 6;

    public CurrentPlayerState() { }

    public CurrentPlayerState(CurrentPlayerState state) {
        primaryWeapon = state.primaryWeapon;
        primaryAmmo = state.primaryAmmo;
        primaryClip = state.primaryClip;
        secondaryWeapon = state.secondaryWeapon;
        secondaryAmmo = state.secondaryAmmo;
        secondaryClip = state.secondaryClip;
        maxHP = state.maxHP;
    }
}

public struct SavedCompanion {
    public Vector3 spawnerPosition;
    public GameObject prefab;

    public SavedCompanion(Vector3 pos, GameObject p) {
        spawnerPosition = pos;
        prefab = p;
    }
}

public class PlayerState : MonoBehaviour {
    public CurrentPlayerState currentState;
    public bool currentStateInvalid;
    public CurrentPlayerState lastSaveState;
    public Vector3 lastCheckpoint;
    public bool hasCheckpoint;

    public CurrentPlayerState enteredSaveState;
    public bool hasLevelStartState;

    public List<Vector3> destroyedThisLife;
    public List<Vector3> destroyedOverall;
    
    public List<Vector3> killedThisLife;
    public List<Vector3> killedOverall;
    
    public List<SavedCompanion> savedThisLife;
    public List<SavedCompanion> savedOverall;

    public List<Vector3> companionsDiedThisLife;
    public List<Vector3> companionsDiedOverall;

    public List<GameObject> companionPrefabCarryover;

    public List<Vector3> lootedThisLife;
    public List<Vector3> lootedOverall;

    public List<Vector3> interactedThisLife;
    public List<Vector3> interactedOverall;

    public int deaths;
    public int kills;
    public int collectedCompanions;
    public int finishedWithCompanions;
    public int lootCount;
    
    [NonSerialized]
    public int hitpoints = 6;

    [NonSerialized]
    public bool companionFollow = false;

    public const int AbsoluteMaxHP = 26;
    
    public static PlayerState Instance;

    private void Awake() {
        if (Instance == null) {
            DontDestroyOnLoad(gameObject);
            Instance = this;
            
            destroyedThisLife = new List<Vector3>();
            destroyedOverall = new List<Vector3>();
            killedThisLife = new List<Vector3>();
            killedOverall = new List<Vector3>();
            savedThisLife = new List<SavedCompanion>();
            savedOverall = new List<SavedCompanion>();
            lootedThisLife = new List<Vector3>();
            lootedOverall = new List<Vector3>();
            interactedThisLife = new List<Vector3>();
            interactedOverall = new List<Vector3>();
            companionPrefabCarryover = new List<GameObject>();
        } else if (Instance != this)
            Destroy(gameObject);
    }
}