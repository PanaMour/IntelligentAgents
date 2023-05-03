using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public Button startButton;
    public Button visibilityButton;
    public Button pauseButton;
    public Button overviewButton;
    public GameObject UIbuttons;
    public TextMeshProUGUI visibilityButtonText;
    public TextMeshProUGUI pauseButtonText;

    public List<GameObject> agentUIElements;

    private bool isMenuVisible = false;
    private bool isPaused = false;

    void Start()
    {
        visibilityButton.onClick.AddListener(ToggleMenuVisibility);
        pauseButton.onClick.AddListener(TogglePause);
        UpdateAgentUIElements();
    }

    private void ToggleMenuVisibility()
    {
        isMenuVisible = !isMenuVisible;

        if (isMenuVisible)
        {
            visibilityButtonText.text = "Hide Menu";
            ShowUIElements();
        }
        else
        {
            visibilityButtonText.text = "Show Menu";
            HideUIElements();
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            pauseButtonText.text = "Continue";
            // Add any code to pause your game here
        }
        else
        {
            pauseButtonText.text = "Pause";
            // Add any code to resume your game here
        }
    }

    private void HideUIElements()
    {
        foreach (GameObject agentUIElement in agentUIElements)
        {
            agentUIElement.SetActive(false);
            UIbuttons.SetActive(false);
        }
    }

    private void ShowUIElements()
    {
        UIbuttons.SetActive(true);
        UpdateAgentUIElements();        
    }

    private void UpdateAgentUIElements()
    {
        // Hide all agent UI elements initially
        foreach (GameObject agentUIElement in agentUIElements)
        {
            agentUIElement.SetActive(false);
        }

        // Get agents from the environment
        GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");

        // Show UI elements for existing agents
        foreach (GameObject agent in agents)
        {
            int agentNumber = int.Parse(agent.name.Substring(agent.name.Length - 1));

            if (agentNumber >= 0 && agentNumber < agentUIElements.Count)
            {
                agentUIElements[agentNumber].SetActive(true);
            }
        }
    }
}
