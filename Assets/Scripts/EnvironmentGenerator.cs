using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class EnvironmentGenerator : MonoBehaviour
{
    public string environmentTextFile = "environment";
    public GameObject groundPrefab;
    public GameObject wallPrefab;
    public GameObject bankPrefab;
    public GameObject fuelPrefab;
    public GameObject phoneboothPrefab;
    public GameObject vendingmachinePrefab;
    public GameObject watercoolerPrefab;
    public GameObject agentPrefab;
    public GameObject housePrefab;
    public GameObject goldPrefab;
    public GameObject energypotPrefab;
    public GameObject ATMlogo;
    public GameObject FuelText;
    public GameObject SnackText;
    public GameObject PhoneText;
    public GameObject HouseText;
    private GameObject groundObject;
    public Material groundMaterial;
    public List<Material> materials;
    public GameObject agentCameraPrefab;
    public GameObject fogPrefab;

    public int agentCount = 0;
    public static TextAsset environmentData;
    public GameObject[,] fogBlocks;
    string environment = "";
    string fileContents = "";
    private void Start()
    {
        environment = PlayerPrefs.GetString("environment");
        if (System.IO.File.Exists(environment))
        {
            // Read the file contents
            fileContents = System.IO.File.ReadAllText(environment);

            // Use the file contents as needed
            Debug.Log("File Contents: " + fileContents);
        }
        else
        {
            Debug.LogWarning("File not found at path: " + environment);
        }
        ParseEnvironment(fileContents);
    }

    public void ParseEnvironment(string environmentText)
    {
        string[] lines = environmentText.Split('\n');
        int numRows = lines.Length;
        int numCols = lines[0].Length - 1; // Subtract 1 for the newline character
        fogBlocks = new GameObject[numCols, numRows];
        Vector3 groundScale = new Vector3(numCols, 1, numRows);
        Vector3 groundPositionCamera = new Vector3(numCols / 2f - 0.5f, -1.5f, -numRows / 2f + 0.5f);
        groundObject = Instantiate(groundPrefab, groundPositionCamera, Quaternion.identity);
        groundObject.GetComponent<Renderer>().material = groundMaterial;
        groundObject.transform.localScale = groundScale;
        groundObject.name = "Ground";
        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numCols; j++)
            {
                char symbol = lines[i][j];
                Vector3 position = new Vector3(j, 0, -i);
                Vector3 groundPosition = new Vector3(j, -1, -i); // Position for the ground block=
                GameObject groundBlock = Instantiate(groundPrefab, groundPosition, Quaternion.identity); // Instantiate the ground block
                if (symbol != '*')
                {
                    Vector3 fogPosition = new Vector3(j, 0, -i);
                    GameObject fogBlock = Instantiate(fogPrefab, fogPosition, Quaternion.identity); // Instantiate the ground block
                    fogBlocks[j,i] = fogBlock;
                    fogBlock.SetActive(false);
                }
                groundBlock.GetComponent<Renderer>().material = groundMaterial; // Set the material to ground material

                switch (symbol)
                {
                    case ' ':
                        GameObject groundBlank = Instantiate(groundPrefab, groundPosition, Quaternion.identity); // Instantiate the ground block
                        groundBlank.GetComponent<Renderer>().material = groundMaterial; // Set the material to ground material
                        break;
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
                        Vector3 positionFuel = position + new Vector3(0.05f, 1.07f, 0f);
                        position += new Vector3(0, -0.5f, 0);
                        GameObject fuel = Instantiate(fuelPrefab, position, Quaternion.identity);
                        GameObject fuelText = Instantiate(FuelText, positionFuel, Quaternion.identity);
                        fuel.tag = "Fuel";
                        fuelText.transform.rotation = Quaternion.Euler(90f, 0, 0);
                        break;
                    case 'P':
                        Vector3 positionPhone = position + new Vector3(0.2f, 1f, 0f);
                        position += new Vector3(0.2f, -0.5f, 0);
                        GameObject phoneBooth = Instantiate(phoneboothPrefab, position, Quaternion.identity);
                        GameObject phoneText = Instantiate(PhoneText, positionPhone, Quaternion.identity);
                        phoneBooth.transform.rotation = Quaternion.Euler(-90f, 180f, 0f);
                        phoneText.transform.rotation = Quaternion.Euler(90f, 0, 0);
                        phoneBooth.tag = "Phone Booth";
                        break;
                    case 'V':
                        Vector3 positionSnack = position + new Vector3(0f, 1.42f, -0.05f);
                        position += new Vector3(0f, -0.5f, 0);
                        GameObject vendingMachine = Instantiate(vendingmachinePrefab, position, Quaternion.identity);
                        GameObject snackText = Instantiate(SnackText, positionSnack, Quaternion.identity);
                        vendingMachine.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                        snackText.transform.rotation = Quaternion.Euler(90f, 0, 0);
                        vendingMachine.tag = "Vending Machine";
                        break;
                    case 'W':
                        position += new Vector3(0f, -0.5f, 0);
                        GameObject waterCooler = Instantiate(watercoolerPrefab, position, Quaternion.identity);
                        //waterCooler.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                        waterCooler.tag = "Water Cooler";
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
                        Vector3 positionHouse = position + new Vector3(0f, -0.42f, 0.45f);
                        GameObject houseObj = Instantiate(housePrefab, new Vector3(j, -0.5f, -i), Quaternion.identity);
                        GameObject agentObj = Instantiate(agentPrefab, new Vector3(j, -0.5f, -i), Quaternion.identity);
                        GameObject chestplate = agentObj.transform.Find("Armors").Find("Plate1").Find("PlateSet1_Chest").gameObject;
                        GameObject chestplate2 = agentObj.transform.Find("Armors").Find("StarterClothes").Find("Starter_Chest").gameObject;
                        Debug.Log(Convert.ToInt32(symbol.ToString()));
                        houseObj.GetComponent<Renderer>().material = materials[Convert.ToInt32(symbol.ToString())];
                        chestplate.GetComponent<Renderer>().material = materials[Convert.ToInt32(symbol.ToString())];
                        chestplate2.GetComponent<Renderer>().material = materials[Convert.ToInt32(symbol.ToString())];
                        GameObject houseText = Instantiate(HouseText, positionHouse, Quaternion.identity);
                        agentObj.name = "Agent_" + symbol;
                        agentObj.tag = "Agent";
                        GameObject agentCamera = agentObj.gameObject.transform.Find("AgentCamera").gameObject;
                        agentCamera.name = "AgentCamera_" + symbol;
                        houseText.GetComponent<TextMeshPro>().text = symbol.ToString();
                        houseText.transform.rotation = Quaternion.Euler(90f, 0, 0);
                        AgentController agentController = agentObj.AddComponent<AgentController>();
                        agentController.SetHouse(houseObj);
                        agentCount++;
                        break;
                }
            }
        }
    }
}
