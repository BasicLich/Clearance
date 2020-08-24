using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

namespace DefaultNamespace.Boss {

    [CreateAssetMenu(fileName = "bossconfig.asset", menuName = "Bosses/Boss Config", order = 0)]
    public class BossConfig : ScriptableObject {
        public string displayName;
        public int hitpoints;
        public List<BossPhase> phases;
        public float phaseTransitionTime;
        public float lootCrateTimer;
        public int hitpointsPerHeart;
        public List<GameObject> loot;
        public AudioClip initiateEffect;
    }
}