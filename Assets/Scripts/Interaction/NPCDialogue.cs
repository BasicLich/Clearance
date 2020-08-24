using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Interaction {
    [CreateAssetMenu(fileName = "dialogue.asset", menuName = "NPC Dialogue", order = 0)]
    public class NPCDialogue : ScriptableObject {
        [FormerlySerializedAs("name")] public string displayName;
        public List<string> dialogue;
    }
}