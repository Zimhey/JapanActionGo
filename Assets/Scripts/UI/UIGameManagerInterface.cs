using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameUIScreens
{
    MainMenu,
    PlayMenu,
    SettingsPanel,
    ControlsPanel,
    GraphicsPanel,
    HelpPanel,
    PausePanel,
    GameOverPanel,
    FirstPersonHUDPanel,
    VirtualRealityHUDPanel
}

public class UIGameManagerInterface : MonoBehaviour {

    public GameObject MainPanel;
    public GameObject PlayPanel;
    public GameObject SettingsPanel;
    public GameObject ControlsPanel;
    public GameObject GraphicsPanel;
    public GameObject HelpPanel;
    public GameObject PausePanel;
    public GameObject GameOverPanel;
    public GameObject FirstPersonHUDPanel;
    public GameObject VirtualRealityHUDPanel;

    public GameUIScreens StartScreen;

    // do not exit this directly
    private GameObject currentPanel;
    private GameObject lastPanel;
    private GameObject beforeSettingsPanel;
    private GameObject camera;

    public GameObject CurrentPanel
    {
        get
        {
            return currentPanel;
        }
        set
        {
            if (currentPanel != null)
                currentPanel.SetActive(false);
            value.SetActive(true);
            lastPanel = currentPanel;
            currentPanel = value;
        }
    }

	// Use this for initialization
	void Start ()
    {
        camera = GameObject.FindGameObjectWithTag("MainCamera");

        // Set Start Screen
        ShowPanel(StartScreen);

	}

    public void ShowPanel(GameUIScreens panel)
    {
        switch (panel)
        {
            case GameUIScreens.MainMenu:
                ShowMainMenu();
                break;
            case GameUIScreens.PlayMenu:
                ShowPlayMenu();
                break;
            case GameUIScreens.SettingsPanel:
                ShowSettingsMenu();
                break;
            case GameUIScreens.ControlsPanel:
                ShowControlsMenu();
                break;
            case GameUIScreens.GraphicsPanel:
                ShowGraphicsMenu();
                break;
            case GameUIScreens.HelpPanel:
                ShowHelpMenu();
                break;
            case GameUIScreens.PausePanel:
                ShowPauseMenu();
                break;
            case GameUIScreens.GameOverPanel:
                ShowGameOverMenu();
                break;
            case GameUIScreens.FirstPersonHUDPanel:
                ShowFirstPersonHUD();
                break;
            case GameUIScreens.VirtualRealityHUDPanel:
                ShowVirtualRealityHUD();
                break;
        }
    }

    public void ShowMainMenu()
    {
        camera.SetActive(true);
        CurrentPanel = MainPanel;
    }

    public void ShowPlayMenu()
    {
        camera.SetActive(true);
        CurrentPanel = PlayPanel;
    }

    public void ShowSettingsMenu()
    {
        camera.SetActive(true);
        beforeSettingsPanel = CurrentPanel;
        CurrentPanel = SettingsPanel;
    }

    public void ShowControlsMenu()
    {
        camera.SetActive(true);
        CurrentPanel = ControlsPanel;
    }

    public void ShowGraphicsMenu()
    {
        camera.SetActive(true);
        CurrentPanel = GraphicsPanel;
    }

    public void ShowHelpMenu()
    {
        camera.SetActive(true);
        CurrentPanel = HelpPanel;
    }

    public void ShowPauseMenu()
    {
        camera.SetActive(true);
        CurrentPanel = PausePanel;
    }

    public void ShowGameOverMenu()
    {
        camera.SetActive(true);
        CurrentPanel = GameOverPanel;
    }

    public void ShowHUD()
    {
        if (GameManager.Instance.PlayersVRType == VirtualRealityType.None)
            ShowFirstPersonHUD();
        else
            ShowVirtualRealityHUD();
    }

    public void ShowFirstPersonHUD()
    {
        camera.SetActive(false);
        CurrentPanel = FirstPersonHUDPanel;
    }

    public void ShowVirtualRealityHUD()
    {
        camera.SetActive(false);
        CurrentPanel = VirtualRealityHUDPanel;
    }

    public void ShowLastPanel()
    {
        camera.SetActive(true);
        CurrentPanel = lastPanel;
    }

    public void ShowBeforeSettingsPanel()
    {
        camera.SetActive(true);
        CurrentPanel = beforeSettingsPanel;
    }

    public void SetDifficulty(int difficulty)
    {
        GameManager.Instance.SetDifficulty((Difficulty)difficulty);
    }

    public void EnableTutorial(bool tutorialEnabled)
    {
        GameManager.Instance.EnableTutorial(tutorialEnabled);
    }

    public void SetSeed(string seed)
    {
        int temp = int.Parse(seed);
        GameManager.Instance.SetSeed(temp);
    }

    public void StartPlaying()
    {
        GameManager.Instance.StartGame();
        ShowHUD();
    }

    public void ResumePlaying()
    {
        GameManager.Instance.UnPause();
    }

    public void QuitToMainMenu()
    {
        GameManager.Instance.MainMenu();
        ShowMainMenu();
    }

}
