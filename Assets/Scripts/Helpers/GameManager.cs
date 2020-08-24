using System;
using System.Collections.Generic;
using DefaultNamespace.Boss;
using Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Helpers {
    public class GameManager : MonoBehaviour {
        public static GameManager Instance;

        public Canvas pauseUI;

        public Canvas dieUI;

        public UICollector hud;

        public Canvas dialogCanvas;
        public Text speakerName;
        public TypeWriter dialog;

        public Camera uiCamera;

        public Text enemyObjective;
        public Text rescueObjective;
        public Text lootObjective;

        public bool inDialogue;
        private NPCDialogue _currentDialogue;
        private int _currentMsg;

        public Image transitionOverlay;

        public AudioSource audioSource;
        public FadingAudioSource musicSource;

        public AudioClip normalMusic;
        public AudioClip bossMusic;

        public Image indicatorArrow;
        public RectTransform uiCenter;
        public float indicatorRadius;

        public Color enemyIndicator;
        public Color companionIndicator;

        [FormerlySerializedAs("_bossBattle")] public bool bossBattle;

        private Coroutine _coroutine;

        public void InitBoss() {
            bossBattle = true;
            rescueObjective.transform.parent.gameObject.SetActive(false);
            lootObjective.transform.parent.gameObject.SetActive(false);
            musicSource.Fade(bossMusic, 0.4f, true);
        }

        public void BossEnd() {
            bossBattle = false;
            musicSource.Fade(normalMusic, 0.4f, true);
        }

        public void PlayDialogue(NPCDialogue dialogue) {
            if (dialogue.dialogue.Count == 0) return;

            // Stop movement
            PlayerController.Instance.inputVector = Vector2.zero;

            // Prepare
            inDialogue = true;
            _currentDialogue = dialogue;
            speakerName.text = dialogue.displayName;
            dialogCanvas.gameObject.SetActive(true);

            // Run first message
            _currentMsg = 0;
            RunMessage(dialogue.dialogue[_currentMsg]);
        }

        private void RunMessage(string msg) {
            dialog.StartTypeWriting(msg);
        }

        private void FinishDialogue() {
            dialogCanvas.gameObject.SetActive(false);
            inDialogue = false;
        }

        private void Update() {
            if (SceneManager.GetActiveScene().buildIndex == 0)
                Destroy(gameObject);
            
            if (SceneManager.GetActiveScene().buildIndex != SceneManager.sceneCount - 1) {
                // Destroy on main menu to clear data
                if (SceneManager.GetActiveScene().buildIndex == 0)
                    Destroy(gameObject);

                if (inDialogue) {
                    var mouse = Mouse.current;
                    if (mouse.leftButton.wasPressedThisFrame) {
                        if (dialog.finishedTyping) {
                            if (_currentMsg < _currentDialogue.dialogue.Count - 1) {
                                _currentMsg++;
                                RunMessage(_currentDialogue.dialogue[_currentMsg]);
                            }
                            else {
                                FinishDialogue();
                                PlayerController.Instance.PostDialogeHotfix();
                            }
                        }
                        else {
                            dialog.QuickFinish();
                        }
                    }
                }

                hud.companionCounter.text = FindObjectsOfType<CompanionAI>().Length.ToString();

                UpdateObjectiveCount();

                // Iterate over enemies and find closest
                var state = PlayerState.Instance;
                int kills = state.killedOverall.Count + state.killedThisLife.Count;

                if (kills < _enemyCount || bossBattle) {
                    var enemies = FindObjectsOfType<EnemyAI>();
                    var plr = PlayerController.Instance;
                    if (enemies.Length > 0) {
                        var smallestDistance = Single.MaxValue;
                        EnemyAI closest = null;
                        foreach (var enemy in enemies) {
                            var dist = enemy.transform.position - plr.transform.position;
                            if (dist.magnitude < smallestDistance) {
                                smallestDistance = dist.magnitude;
                                closest = enemy;
                            }
                        }

                        if (closest != null) {
                            var viewportPos = Camera.main.WorldToViewportPoint(closest.transform.position);
                            if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1) {
                                var dir = closest.transform.position - plr.transform.position;

                                var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
                                indicatorArrow.rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
                                indicatorArrow.rectTransform.position =
                                    uiCenter.position + dir.normalized * indicatorRadius;
                                indicatorArrow.gameObject.SetActive(true);
                                indicatorArrow.color = enemyIndicator;
                            }
                            else indicatorArrow.gameObject.SetActive(false);
                        }
                        else indicatorArrow.gameObject.SetActive(false);
                    }
                    else {
                        var boss = FindObjectOfType<BossManager>();
                        var viewportPos = Camera.main.WorldToViewportPoint(boss.transform.position);
                        if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1) {
                            var dir = boss.transform.position - plr.transform.position;

                            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
                            indicatorArrow.rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
                            indicatorArrow.rectTransform.position =
                                uiCenter.position + dir.normalized * indicatorRadius;
                            indicatorArrow.gameObject.SetActive(true);
                            indicatorArrow.color = enemyIndicator;
                        }
                        else indicatorArrow.gameObject.SetActive(false);
                    }
                }
                else {
                    var plr = PlayerController.Instance;
                    var companions = FindObjectsOfType<Rescue>();
                    var boss = FindObjectOfType<BossManager>();
                    if (boss == null) return;

                    if (companions.Length > 0 && boss.exitDoor.locked) {
                        var smallestDistance = Single.MaxValue;
                        Rescue closest = null;
                        foreach (var companion in companions) {
                            var dist = companion.transform.position - plr.transform.position;
                            if (dist.magnitude < smallestDistance) {
                                smallestDistance = dist.magnitude;
                                closest = companion;
                            }
                        }

                        if (closest != null) {
                            var viewportPos = Camera.main.WorldToViewportPoint(closest.transform.position);
                            if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1) {
                                var dir = closest.transform.position - plr.transform.position;

                                var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
                                indicatorArrow.rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
                                indicatorArrow.rectTransform.position =
                                    uiCenter.position + dir.normalized * indicatorRadius;
                                indicatorArrow.gameObject.SetActive(true);
                                indicatorArrow.color = companionIndicator;
                            }
                            else indicatorArrow.gameObject.SetActive(false);
                        }
                        else indicatorArrow.gameObject.SetActive(false);
                    }
                    else {
                        if (boss.exitDoor.locked) {
                            var viewportPos = Camera.main.WorldToViewportPoint(boss.entryDoor.transform.position);
                            if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1) {
                                var dir = boss.entryDoor.transform.position - plr.transform.position;

                                var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
                                indicatorArrow.rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
                                indicatorArrow.rectTransform.position =
                                    uiCenter.position + dir.normalized * indicatorRadius;
                                indicatorArrow.gameObject.SetActive(true);
                                indicatorArrow.color = enemyIndicator;
                            }
                            else indicatorArrow.gameObject.SetActive(false);
                        }
                        else {
                            var viewportPos = Camera.main.WorldToViewportPoint(boss.exitDoor.transform.position);
                            if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1) {
                                var dir = boss.exitDoor.transform.position - plr.transform.position;

                                var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
                                indicatorArrow.rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
                                indicatorArrow.rectTransform.position =
                                    uiCenter.position + dir.normalized * indicatorRadius;
                                indicatorArrow.gameObject.SetActive(true);
                                indicatorArrow.color = enemyIndicator;
                            }
                            else indicatorArrow.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        public void TogglePause() {
            transitionOverlay.color = new Color(0, 0, 0, 0);
            if (pauseUI.gameObject.activeSelf) {
                pauseUI.gameObject.SetActive(false);
                Time.timeScale = 1f;
            }
            else {
                pauseUI.gameObject.SetActive(true);
                Time.timeScale = 0f;
            }
        }

        public void Died() {
            transitionOverlay.color = new Color(0, 0, 0, 0);
            Time.timeScale = 0f;
            dieUI.gameObject.SetActive(true);
            BossEnd();
        }

        public void RestartScene() {
            Time.timeScale = 1f;
            PlayerState.Instance.currentStateInvalid = true;
            var state = PlayerState.Instance;
            state.destroyedThisLife.Clear();
            state.killedThisLife.Clear();
            state.savedThisLife.Clear();
            state.lootedThisLife.Clear();
            state.interactedThisLife.Clear();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            hud.Reset();
            pauseUI.gameObject.SetActive(false);
            dialogCanvas.gameObject.SetActive(false);
            dieUI.gameObject.SetActive(false);
            inDialogue = false;
            transitionOverlay.color = new Color(0, 0, 0, 0);
            musicSource.Fade(normalMusic, 0.4f, true);
        }

        private void ResetState() {
            var state = PlayerState.Instance;
            state.destroyedOverall.Clear();
            state.destroyedThisLife.Clear();
            state.killedOverall.Clear();
            state.killedThisLife.Clear();
            state.savedThisLife.Clear();
            state.savedOverall.Clear();
            state.lootedOverall.Clear();
            state.lootedThisLife.Clear();
            state.interactedThisLife.Clear();
            state.interactedOverall.Clear();
            state.hasCheckpoint = false;
            transitionOverlay.color = new Color(0, 0, 0, 0);
        }

        public void NextLevel() {
            Time.timeScale = 1f;
            ResetState();
            var state = PlayerState.Instance;
            state.lastSaveState = state.currentState;
            SaveCompanions();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void SaveCompanions() {
            var state = PlayerState.Instance;
            if (state.companionPrefabCarryover == null)
                state.companionPrefabCarryover = new List<GameObject>();
            state.companionPrefabCarryover.Clear();
            var companions = FindObjectsOfType<CompanionAI>();
            foreach (var companion in companions)
                state.companionPrefabCarryover.Add(companion.myPrefab);
            PlayerState.Instance.finishedWithCompanions = companions.Length;
        }

        public void RestartLevel() {
            Time.timeScale = 1f;
            ResetState();
            PlayerState.Instance.currentStateInvalid = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            musicSource.Fade(normalMusic, 0.4f, true);
        }

        public void FixCompanions() {
            PlayerController.Instance.SummonCompanions();
        }

        private int _enemyCount;
        private int _companionCount;
        private int _crateCount;

        public void InitObjectiveCount() {
            // Count objectives
            _enemyCount = FindObjectsOfType<EnemyAI>().Length;
            _companionCount = FindObjectsOfType<Rescue>().Length;
            _crateCount = FindObjectsOfType<LootCrate>().Length;
            UpdateObjectiveCount();
        }

        public void UpdateObjectiveCount() {
            if (bossBattle) {
                var state = PlayerState.Instance;
                int kills = state.killedOverall.Count + state.killedThisLife.Count;
                var enemyCount = FindObjectsOfType<EnemyAI>().Length - (_enemyCount - kills);
                if (enemyCount == 0)
                    enemyObjective.transform.parent.gameObject.SetActive(false);
                else {
                    enemyObjective.transform.parent.gameObject.SetActive(true);
                    enemyObjective.text = enemyCount + " remaining";
                }
            }
            else {
                // Get player state
                var state = PlayerState.Instance;

                int kills = state.killedOverall.Count + state.killedThisLife.Count;
                int saves = state.savedOverall.Count + state.savedThisLife.Count;
                int looted = state.lootedOverall.Count + state.lootedThisLife.Count;

                enemyObjective.text = kills + " of " + _enemyCount;
                rescueObjective.text = saves + " of " + _companionCount;
                lootObjective.text = looted + " of " + _crateCount;
            }
        }

        public bool ObjectivesCompleted() {
            var state = PlayerState.Instance;
            int kills = state.killedOverall.Count + state.killedThisLife.Count;
            return (kills >= _enemyCount);
        }

        private void Awake() {
            if (Instance == null) {
                DontDestroyOnLoad(gameObject);
                Instance = this;
            }
            else if (Instance != this)
                Destroy(gameObject);
        }
    }
}