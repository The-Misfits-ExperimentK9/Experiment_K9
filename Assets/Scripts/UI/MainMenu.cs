using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject levelSelectMenu;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameObject levelSelectAnchor;
    [SerializeField] private GameObject levelSelectButtonPrefab;
    [SerializeField] private Button[] levelButtons;
    private float buttonHeight = 100;
    private Button selectedButton;
    // Start is called before the first frame update
    void Start()
    {   
        levelButtons = new Button[levelManager.levelNames.Count];
        for (int x = 0;x < levelManager.levelNames.Count; x++) {
            CreateLevelButton(x);
        }
    }
    void CreateLevelButton(int index) {
        var button = Instantiate(levelSelectButtonPrefab, levelSelectAnchor.transform).GetComponent<Button>();
        var buttonTransform = button.GetComponent<RectTransform>();
        
        button.GetComponentInChildren<TextMeshProUGUI>().text = levelManager.levelNames[index];
        buttonTransform.anchoredPosition = Vector2.zero;
        if (index != 0) {
            buttonTransform.anchoredPosition = new Vector2(0, -buttonHeight * index);
        }
        button.onClick.AddListener(SetLoadIndex);
        levelButtons[index] = button;
        
    }
    public void SetLoadIndex() {
        selectedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
    }
    public void LoadSelectedLevel() {
        levelManager.LoadLevelByIndex(levelManager.levelNames.IndexOf(selectedButton.GetComponentInChildren<TextMeshProUGUI>().text));
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartGame() {
        levelManager.LoadLevelByIndex(0);
    }
    public void QuitGame() {
        Application.Quit();
    }
    public void ShowLevelSelect() {
        mainMenu.SetActive(false);
        levelSelectMenu.SetActive(true);
    }
    public void HideLevelSelect() {
        mainMenu.SetActive(true);
        levelSelectMenu.SetActive(false);
    }
}
