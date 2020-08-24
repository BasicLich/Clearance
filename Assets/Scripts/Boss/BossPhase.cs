using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace DefaultNamespace.Boss {
    public enum PhaseEndCondition {
        None,
        HitpointThreshold,
        EnemiesKilled,
        Timer
    }
    
    [CreateAssetMenu(fileName = "phase.asset", menuName = "Bosses/Boss Phase", order = 0)]
    public class BossPhase : ScriptableObject {
        public bool firesBullets;
        public float timeBetweenShots;
        public List<GameObject> bulletTypes;
        public int bulletSpacing;
        public float bulletVelocity;
        [FormerlySerializedAs("rotationalOffsetIncr")] public int rotationalOffsetIncrFrom;
        public int rotationalOffsetIncrTo;
        public bool spawnsEnemies;
        public List<GameObject> possibleEnemies;
        public int totalCost;

        public PhaseEndCondition endCondition;
        public int hitpointThreshold;
        public float timerLength;

        public bool invulnerableDuringPhase;
    }
}