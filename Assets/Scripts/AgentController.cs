using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float energy = 100f;
    public GameObject house;
    public string planFileName;
    private Vector2Int currentPosition;
    private Vector2Int nextPosition;
    [SerializeField] private string[] plan;
    private int currentBuildingIndex = 0;
    private bool exploring = true;
    [SerializeField]TextAsset planData;
    private string environmentTextFile = "environment";
    [SerializeField] private GridBlock[,] grid;
    private void Start()
    {
        // Read the plan from the text file with the given filename
        planFileName = "agent_0_plan";
        SetPlan(planFileName);
        planData = Resources.Load<TextAsset>(planFileName);
        if (planData != null)
        {
            plan = planData.text.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        }
        // Set the current position of the agent based on its initial position in the grid
        currentPosition = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(-transform.position.z));

        TextAsset environmentData = Resources.Load<TextAsset>(environmentTextFile);
        InitializeGrid(environmentData.text);

        grid[Mathf.RoundToInt(transform.position.x),Mathf.RoundToInt(-transform.position.z)].discovered = true;
        Debug.Log("2, 7" + grid[2, 7].discovered);
    }
    private void InitializeGrid(string environmentText)
    {
        string[] lines = environmentText.Split('\n');
        int numRows = lines.Length;
        int numCols = lines[0].Length - 1; // Subtract 1 for the newline character
        grid = new GridBlock[numRows, numCols];
        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numCols; j++)
            {
                GridBlock block = new GridBlock();
                block.symbol = lines[i][j].ToString();
                block.x = i;
                block.y = -j;
                block.discovered = false;
                grid[i, j] = block;
            }
        }
    }


    private void Update()
    {
        // Check if the agent has run out of energy and destroy it if so
        if (energy <= 0)
        {
            /*Destroy(gameObject);
            return;*/
        }

        // If the agent is exploring, try to move to a neighboring block that has not been visited
        if (exploring)
        {
            List<Vector2Int> unvisitedNeighbors = GetUnvisitedNeighbors(currentPosition);
            if (unvisitedNeighbors.Count > 0)
            {
                nextPosition = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
            }
            else
            {
                exploring = false;
                currentBuildingIndex = 0;
                nextPosition = new Vector2Int(Mathf.RoundToInt(house.transform.position.x), Mathf.RoundToInt(-house.transform.position.z));
            }
        }
        // Otherwise, move to the next block in the plan
        else
        {
            Vector2Int nextBuildingPosition = GetBuildingPosition(plan[currentBuildingIndex]);
            if (nextBuildingPosition != currentPosition)
            {
                nextPosition = nextBuildingPosition;
            }
            else
            {
                currentBuildingIndex++;
                if (currentBuildingIndex >= plan.Length)
                {
                    exploring = true;
                }
                else
                {
                    nextBuildingPosition = GetBuildingPosition(plan[currentBuildingIndex]);
                    nextPosition = nextBuildingPosition;
                }
            }
        }

        // Move the agent towards the next position
        Vector3 targetPosition = new Vector3(nextPosition.x, transform.position.y, -nextPosition.y);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Consume energy when the agent moves to a new block
        if (currentPosition != nextPosition)
        {
            energy--;
            currentPosition = nextPosition;
        }
    }

    // Get the position of a building in the grid based on its index in the plan
    private Vector2Int GetBuildingPosition(string building)
    {
        string[] buildingInfo = building.Split();
        string buildingIdentifier = buildingInfo[buildingInfo.Length - 1];
        int buildingIndex;
        if (int.TryParse(buildingIdentifier, out buildingIndex))
        {
            string[] lines = EnvironmentGenerator.environmentData.text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < lines[i].Length; j++)
                {
                    if (char.IsDigit(lines[i][j]) && int.Parse(lines[i][j].ToString()) == buildingIndex)
                    {
                        return new Vector2Int(j, i);
                    }
                }
            }
            Debug.LogError("Building index not found: " + buildingIndex);
            return Vector2Int.zero;
        }
        else if (buildingIdentifier == "B")
        {
            // Return position of bank
            GameObject[] banks = GameObject.FindGameObjectsWithTag("Bank");
            if (banks.Length > 0)
            {
                return new Vector2Int(Mathf.RoundToInt(banks[0].transform.position.x), Mathf.RoundToInt(-banks[0].transform.position.z));
            }
            else
            {
                Debug.LogError("No bank found in the scene");
                return Vector2Int.zero;
            }
        }
        else
        {
            Debug.LogError("Invalid building identifier: " + buildingIdentifier);
            return Vector2Int.zero;
        }
    }



    // Get a list of unvisited neighboring blocks
    private List<Vector2Int> GetUnvisitedNeighbors(Vector2Int position)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        int x = position.x;
        int y = position.y;
        if (IsValidPosition(x - 1, y) && !IsVisited(x - 1, y))
        {
            neighbors.Add(new Vector2Int(x - 1, y));
        }
        if (IsValidPosition(x + 1, y) && !IsVisited(x + 1, y))
        {
            neighbors.Add(new Vector2Int(x + 1, y));
        }
        if (IsValidPosition(x, y - 1) && !IsVisited(x, y - 1))
        {
            neighbors.Add(new Vector2Int(x, y - 1));
        }
        if (IsValidPosition(x, y + 1) && !IsVisited(x, y + 1))
        {
            neighbors.Add(new Vector2Int(x, y + 1));
        }
        return neighbors;
    }

    // Check if a block at the given position has been visited by the agent
    private bool IsVisited(int x, int y)
    {
        Collider[] colliders = Physics.OverlapBox(new UnityEngine.Vector3(x, 0.5f, -y), new UnityEngine.Vector3(0.5f, 0.5f, 0.5f));
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.CompareTag("Agent"))
            {
                return true;
            }
        }
        return false;
    }

    // Check if a position is valid in the grid
    private bool IsValidPosition(int x, int y)
    {
        string[] lines = EnvironmentGenerator.environmentData.text.Split('\n');
        return x >= 0 && x < lines[0].Length && y >= 0 && y < lines.Length && lines[y][x] != '*';
    }

    // Bind the agent to a house
    public void SetHouse(GameObject house)
    {
        this.house = house;
    }

    // Set the plan for the agent to follow
    public void SetPlan(string planFileName)
    {
        TextAsset planData = Resources.Load<TextAsset>(planFileName);
        if (planData != null)
        {
            plan = planData.text.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            currentBuildingIndex = 0;
            exploring = false;
        }
    }
}

