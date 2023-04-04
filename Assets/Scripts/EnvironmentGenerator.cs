using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnvironmentGenerator : MonoBehaviour
{
    public string environmentTextFile = "environment";
    public GameObject groundPrefab;
    public GameObject wallPrefab;
    public GameObject bankPrefab;
    public GameObject agentPrefab;
    public GameObject housePrefab;
    private GameObject groundObject;
    public static TextAsset environmentData;

    private void Start()
    {
        environmentData = Resources.Load<TextAsset>(environmentTextFile);
        ParseEnvironment(environmentData.text);
    }

    public void ParseEnvironment(string environmentText)
    {
        string[] lines = environmentText.Split('\n');
        int numRows = lines.Length;
        int numCols = lines[0].Length - 1; // Subtract 1 for the newline character
        Vector3 groundScale = new Vector3(numCols, 1, numRows);
        Vector3 groundPosition = new Vector3(numCols / 2f - 0.5f, -1f, -numRows / 2f + 0.5f);
        groundObject = Instantiate(groundPrefab, groundPosition, Quaternion.identity);
        groundObject.transform.localScale = groundScale;
        groundObject.name = "Ground";
        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numCols; j++)
            {
                char symbol = lines[i][j];
                Vector3 position = new Vector3(j, 0, -i);
                switch (symbol)
                {
                    case '*':
                        Instantiate(wallPrefab, position, Quaternion.identity);
                        break;
                    case 'B':
                        GameObject bank = Instantiate(bankPrefab, position, Quaternion.identity);
                        bank.tag = "Bank";
                        break;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        GameObject houseObj = Instantiate(housePrefab, new Vector3(j, -0.5f, -i), Quaternion.identity);
                        GameObject agentObj = Instantiate(agentPrefab, position, Quaternion.identity);
                        agentObj.tag = "Agent";
                        AgentController agentController = agentObj.AddComponent<AgentController>();
                        agentController.SetHouse(houseObj);
                        agentController.SetPlan("agent_0_plan.txt");
                        break;
                }
            }
        }
    }
}
