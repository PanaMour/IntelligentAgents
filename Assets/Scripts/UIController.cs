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
    public bool hasStarted = false;
    public bool NextMoveTriggered = false;
    public AgentController agentController;

    void Start()
    {
        Time.timeScale = 0f;
        UpdateAgentUIElements();
    }

    private void Update()
    {
        if (isMenuVisible)
            UpdateAgentUIElements();
    }
    public void ToggleStart()
    {
        Time.timeScale = 1f;
        pauseButton.interactable = true;
        startButton.interactable = false;
    }
    public void ToggleMenuVisibility()
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

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            pauseButtonText.text = "Continue";
            Time.timeScale = 0f; // Stop time
        }
        else
        {
            pauseButtonText.text = "Pause";
            Time.timeScale = 1f; // Continue time
        }
    }
    public void OnNextMoveButtonClick()
    {
        Time.timeScale = 1f;
        GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");

        // Execute a single movement for each agent
        foreach (GameObject agent in agents)
        {
            AgentController agentController = agent.GetComponent<AgentController>();
            agentController.TriggerNextMove();
            //StartCoroutine(agentController.RandomMovement(agentController.buildingPosition, agentController.symbol));

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

    private void UpdateAgentUIElements(){
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
            
            // Get the child EnergyX and GoldX TextMeshPro components of the agent UI element
            Transform energyText = agentUIElements[agentNumber].transform.Find("Energy0");
            Transform goldText = agentUIElements[agentNumber].transform.Find("Gold0");

            // Get the AgentController component of the agent
            AgentController agentController = agent.GetComponent<AgentController>();

            // Update the Energy and Gold text of the agent UI element
            energyText.GetComponent<TextMeshProUGUI>().text = agentController.energy + "E";
            goldText.GetComponent<TextMeshProUGUI>().text = agentController.gold + "G";

            // Show the agent UI element
            agentUIElements[agentNumber].SetActive(true);
        }
    }
}

}
