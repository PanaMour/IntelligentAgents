using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentGenerator : MonoBehaviour
{
    public string environmentTextFile = "environment";
    public GameObject groundPrefab;
    public GameObject wallPrefab;
    public GameObject bankPrefab;
    private GameObject groundObject;

    private void Start()
    {
        TextAsset environmentData = Resources.Load<TextAsset>(environmentTextFile);
        ParseEnvironment(environmentData.text);
    }

    private void ParseEnvironment(string environmentText)
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
                        Instantiate(bankPrefab, position, Quaternion.identity);
                        break;
                }
            }
        }
    }
}
