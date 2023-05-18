using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using UnityEditor.PackageManager;

public class UIController : MonoBehaviour
{
    public Button startButton;
    public Button visibilityButton;
    public Button pauseButton;
    public Button nextmoveButton;
    public Button overviewButton;
    public GameObject UIbuttons;
    public TextMeshProUGUI visibilityButtonText;
    public TextMeshProUGUI pauseButtonText;
    public List<GameObject> agentUIElements;
    public List<GameObject> statagentUIElements;

    private bool isMenuVisible = false;
    private bool isPaused = false;
    public bool hasStarted = false;
    public bool NextMoveTriggered = false;
    public AgentController agentController;
    public Camera mainCamera;
    public GameObject EnvironmentGenerator;
    public GameObject[,] fogBlocks;
    private void Awake()
    {
        Time.timeScale = 0f;
    }
    void Start()
    {
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
        GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");
        foreach (GameObject agent in agents)
        {
            AgentController agentController = agent.GetComponent<AgentController>();
            agentController.Continue = true;
            agentController.hasStarted = true;
        }
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

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            nextmoveButton.interactable = true;
            pauseButtonText.text = "Continue";
            GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");
            foreach (GameObject agent in agents)
            {
                AgentController agentController = agent.GetComponent<AgentController>();
                agentController.Continue = false;
            }
        }
        else
        {
            pauseButtonText.text = "Pause";
            nextmoveButton.interactable = false;
            GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");
            foreach (GameObject agent in agents)
            {
                AgentController agentController = agent.GetComponent<AgentController>();
                agentController.Continue = true;
            }
            Time.timeScale = 1f; // Continue time
        }
    }

    public void OnNextMoveButtonClick()
    {
        Time.timeScale = 1f;
        pauseButton.interactable = false;
        GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");

        // Execute a single movement for each agent
        foreach (GameObject agent in agents)
        {
            AgentController agentController = agent.GetComponent<AgentController>();
            agentController.TriggerNextMove();
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
            statagentUIElements[agentNumber].SetActive(true);
        }
    }
    }

    public void AgentCamera(GameObject agentObject)
    {
        // Get the name of the agent object
        string agentName = agentObject.name;

        // Check if the name matches the pattern "AgentX"
        if (Regex.IsMatch(agentName, "^Agent[0-9]$"))
        {
            // Get the agent number from the name
            int agentNumber = int.Parse(agentName.Substring(agentName.Length - 1));

            // Get the corresponding camera
            GameObject agentCamera = GameObject.Find("AgentCamera_" + agentNumber);
            GameObject[] cameras = GameObject.FindGameObjectsWithTag("Camera");
            foreach (GameObject camera in cameras)
            {
                if (camera.gameObject == agentCamera)
                {
                    camera.GetComponent<Camera>().enabled = true;
                }
                else
                {
                    camera.GetComponent<Camera>().enabled = false;
                }

            }
            // Activate the camera and deactivate all other cameras
            foreach (Camera camera in Camera.allCameras)
            {
                if (camera.gameObject == agentCamera)
                {
                    camera.enabled = true;
                }
                else
                {
                    camera.enabled = false;
                }
            }
        }
    }

    public void Overview()
    {
        fogBlocks = EnvironmentGenerator.GetComponent<EnvironmentGenerator>().fogBlocks;

        for (int i = 1; i < fogBlocks.GetLength(0) - 1; i++)
        {
            for (int j = 1; j < fogBlocks.GetLength(1) - 1; j++)
            {
                fogBlocks[i, j].SetActive(false);
            }
        }

        mainCamera.enabled = true;
        GameObject[] cameras = GameObject.FindGameObjectsWithTag("Camera");
        foreach (GameObject camera in cameras)
        {
            camera.GetComponent<Camera>().enabled = false;

        }
    }
    public void ToggleStatistics()
    {
        GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");
        foreach (GameObject agent in agents)
        {
            int agentNumber = int.Parse(agent.name.Substring(agent.name.Length - 1));
            Debug.Log(agentNumber);
            if (agentNumber >= 0 && agentNumber < statagentUIElements.Count)
            {
                // Get the child EnergyX and GoldX TextMeshPro components of the agent UI element
                Transform energyText = statagentUIElements[agentNumber].transform.Find("Energy0");
                Transform goldText = statagentUIElements[agentNumber].transform.Find("Gold0");
                Transform stepsText = statagentUIElements[agentNumber].transform.Find("Steps");
                Transform knowledgeText = statagentUIElements[agentNumber].transform.Find("Knowledge");
                Transform aliveText = statagentUIElements[agentNumber].transform.Find("Alive");

                // Get the AgentController component of the agent
                AgentController agentController = agent.GetComponent<AgentController>();

                // Update the Energy and Gold text of the agent UI element
                energyText.GetComponent<TextMeshProUGUI>().text = "Energy Spent: " + agentController.ecount.ToString();
                goldText.GetComponent<TextMeshProUGUI>().text = "Gold Collected: " + agentController.gcount.ToString();
                stepsText.GetComponent<TextMeshProUGUI>().text = "Steps: " + agentController.scount.ToString();
                knowledgeText.GetComponent<TextMeshProUGUI>().text = "Trades: " + agentController.tcount.ToString();
                aliveText.GetComponent<TextMeshProUGUI>().text = "Alive: " + agentController.acount.ToString();
            }
        }
    }
}
