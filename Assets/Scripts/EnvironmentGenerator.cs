using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EnvironmentGenerator : MonoBehaviour
{
    public string environmentTextFile = "environment";
    public GameObject groundPrefab;
    public GameObject wallPrefab;
    public GameObject bankPrefab;
    public GameObject fuelPrefab;
    public GameObject agentPrefab;
    public GameObject housePrefab;
    public GameObject goldPrefab;
    public GameObject energypotPrefab;
    public GameObject ATMlogo;
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
                        Vector3 positionLogo = position + new Vector3(0, 0.61f, 0.2f);
                        position += new Vector3(0, -0.5f, 0);
                        GameObject bank = Instantiate(bankPrefab, position, Quaternion.identity);
                        GameObject atmlogo = Instantiate(ATMlogo, positionLogo, Quaternion.identity);
                        bank.transform.rotation = Quaternion.Euler(-90f,-90f,-90f);
                        atmlogo.transform.rotation = Quaternion.Euler(90f,0,0);
                        bank.tag = "Bank";
                        break;
                    case 'F':
                        position += new Vector3(0, -0.5f, 0);
                        GameObject fuel = Instantiate(fuelPrefab, position, Quaternion.identity);
                        fuel.tag = "Fuel";
                        break;
                    case 'E':
                        position += new Vector3(0, -0.5f, 0);
                        GameObject energyPot = Instantiate(energypotPrefab, position, Quaternion.identity);
                        energyPot.tag = "Energy Pot";
                        break;
                    case 'G':
                        position += new Vector3(0, -0.5f, 0);
                        GameObject gold = Instantiate(goldPrefab, position, Quaternion.identity);
                        //gold.transform.rotation = Quaternion.Euler(-90f, -90f, -90f);
                        gold.tag = "Gold";
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
                        GameObject agentObj = Instantiate(agentPrefab, new Vector3(j, -0.5f, -i), Quaternion.identity);
                        agentObj.name = "Agent_" + symbol;
                        agentObj.tag = "Agent";
                        AgentController agentController = agentObj.AddComponent<AgentController>();
                        agentController.SetHouse(houseObj);
                        break;
                }
            }
        }
    }
}
