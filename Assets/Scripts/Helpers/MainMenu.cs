using DefaultNamespace.Balance;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Helpers {
    public class MainMenu : MonoBehaviour {
        public DifficultyPreset easy;
        public DifficultyPreset normal;
        public DifficultyPreset hard;
        public DifficultyPreset impossible;

        public Button hardB;
        public Button impossB;

        public void Quit() {
            Application.Quit();
        }
        
        private void Start() {
            hardB.gameObject.SetActive(false);
            impossB.gameObject.SetActive(false);
        }

        public void ToggleUntested() {
            hardB.gameObject.SetActive(!hardB.gameObject.activeSelf);
            impossB.gameObject.SetActive(!impossB.gameObject.activeSelf);
        }
        
        public void PlayEasy() {
            DifficultOwner.Instance.currentPreset = easy;
            SceneManager.LoadScene(1);
        }
        
        public void PlayNormal() {
            DifficultOwner.Instance.currentPreset = normal;
            SceneManager.LoadScene(1);
        }
        
        public void PlayHard() {
            DifficultOwner.Instance.currentPreset = hard;
            SceneManager.LoadScene(1);
        }
        
        public void PlayImpossible() {
            DifficultOwner.Instance.currentPreset = impossible;
            SceneManager.LoadScene(1);
        }
    }
}