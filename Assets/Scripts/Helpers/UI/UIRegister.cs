using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Helpers.UI {
    public class UIRegister : MonoBehaviour {
        public Canvas pauseUI;
        
        public Canvas dieUI;
        
        public UICollector hud;
        
        public Canvas dialogCanvas;
        public Text speakerName;
        public TypeWriter dialog;

        public Image transitionOverlay;
        
        public Text enemyObjective;
        public Text rescueObjective;
        public Text lootObjective;
        
        public Image indicatorArrow;
        public RectTransform uiCenter;
        
        private void Awake() {
            var mgr = GameManager.Instance;
            mgr.pauseUI = pauseUI;
            mgr.dieUI = dieUI;
            mgr.hud = hud;
            mgr.dialogCanvas = dialogCanvas;
            mgr.speakerName = speakerName;
            mgr.dialog = dialog;
            mgr.transitionOverlay = transitionOverlay;
            mgr.enemyObjective = enemyObjective;
            mgr.rescueObjective = rescueObjective;
            mgr.lootObjective = lootObjective;
            mgr.indicatorArrow = indicatorArrow;
            mgr.uiCenter = uiCenter;
        }

        public void TogglePause() {
            GameManager.Instance.TogglePause();
        }

        public void Respawn() {
            GameManager.Instance.RestartScene();
        }

        public void Restart() {
            GameManager.Instance.RestartLevel();
        }

        public void MainMenu() {
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        }
    }
}