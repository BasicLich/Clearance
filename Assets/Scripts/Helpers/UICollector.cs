using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Helpers {
    public class UICollector : MonoBehaviour {
        public Image primaryGunImage;
        public Text primaryAmmoText;
        public Image secondaryGunImage;
        public Text secondaryAmmoText;
        public GameObject interactControl;
        public Text interactText;

        public GameObject heartContainer;
        
        [NonSerialized]
        public List<Image> hearts;

        public RectTransform bossbar;
        public Text bossLabel;
        
        [NonSerialized]
        public List<Image> bossHearts;

        public Text companionState;

        public Sprite heartEmpty;
        public Sprite heartHalf;
        public Sprite heartFull;

        public GameObject heartPrefab;
        public GameObject bossHeartPrefab;
        
        public Color enemyHeart;
        public Color enemyInvulnerable;

        public Text companionCounter;

        public void Reset() {
            foreach (var heart in hearts)
                Destroy(heart.gameObject);
            hearts.Clear();
            if (bossHearts != null) {
                RemoveBoss();
            }
        }

        public void CreateHearts(int heartCount) {
            if (hearts == null)
                hearts = new List<Image>();
            for (var i = 0; i < heartCount; i++) {
                hearts.Add(Instantiate(heartPrefab, heartContainer.transform).GetComponent<Image>());
            }
        }

        public void RegisterBoss(string bossName, int heartCount) {
            if (bossHearts == null)
                bossHearts = new List<Image>();
            for (var i = 0; i < heartCount; i++) {
                bossHearts.Add(Instantiate(bossHeartPrefab, bossbar).GetComponent<Image>());
            }

            bossLabel.text = bossName;
        }

        public void RemoveBoss() {
            bossLabel.text = "";
            foreach (var h in bossHearts)
                Destroy(h.gameObject);
            bossHearts.Clear();
        }

        public void BossVulnerable() {
            foreach (var h in bossHearts)
                h.color = enemyHeart;
        }
        
        public void BossInvulnerable() {
            foreach (var h in bossHearts)
                h.color = enemyInvulnerable;
        }
    }
}