using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InterfaceBehaviour : MonoBehaviour {
    //  [SerializeField] private TextMeshProUGUI dogModeText;
    [SerializeField] private TextMeshProUGUI dogAutoEnabledText;
    // private string dogToggleTextPrefix = "D.O.G. Mode: ";
    private string dogEnabledPrefix = "D.O.G. Transfer: ";
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject leaveWallPrompt;
    [SerializeField] private List<GameObject> tutorialMessages;
    
    //[SerializeField] private GameObject player3D;
    //[SerializeField] private GameObject player2D;
    //[SerializeField] private GameObject playerBehavior;
    
    

    public void SetDogToggleText(bool dogIsRangedMode) {
        //  dogModeText.text = dogToggleTextPrefix + (dogIsRangedMode ? "Manual" : "Auto");
        dogAutoEnabledText.gameObject.SetActive(!dogIsRangedMode);
    }
    public void SetDogAutoEnabledText(bool isAutoEnabled) {
        dogAutoEnabledText.text = dogEnabledPrefix + (isAutoEnabled ? "On" : "Off");
    }

    public void Pause() {
        
        pauseMenu.SetActive(true);
    }
    public void UnPause() {
        pauseMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        
    }
    public void DisplayTutorialMessageByIndex(int index) {
        
        if (index < 0 || index >= tutorialMessages.Count) return;

        tutorialMessages[index].SetActive(true);
        for (int x = 0; x < tutorialMessages.Count; x++) {
            if (x != index) {
                tutorialMessages[x].SetActive(false);
            }
        }
    }
    public void DisableActiveTutorials() {
        for (int x = 0; x < tutorialMessages.Count; x++) {
            tutorialMessages[x].SetActive(false);

        }
    }

    public void ToggleWallPrompt()
    {
        if (leaveWallPrompt.activeInHierarchy)
        {
            leaveWallPrompt.SetActive(false);
        }
        else
        {
            leaveWallPrompt.SetActive(true);
        }
    }
    
    public void QuitGame() {
        Application.Quit();
    }
    public void LoadMainMenu() {
        Debug.Log("Loading main menu");
        SceneManager.LoadScene(0);
    }
    public void LoadNextLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void ResetLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1;
    }

}
