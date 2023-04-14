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
    private Node currentPosition;
    private Vector2Int nextPosition;
    [SerializeField] private string[] plan;
    [SerializeField] private string[] firstLine;
    Node currentDestination;
    private int currentBuildingIndex = 0;
    private bool exploring = true;
    [SerializeField] TextAsset planData;
    private string environmentTextFile = "environment";
    [SerializeField] private Grid grid;
    [SerializeField] private List<Node> currentPath;
    private void Start()
    {
        planFileName = "agent_"+gameObject.name.Split('_')[1]+"_plan";
        planData = Resources.Load<TextAsset>(planFileName);
        if (planData != null)
        {
            // Split the plan by lines
            string[] planLines = planData.text.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

            // Extract the agent number from the first line of the plan
            firstLine = planLines[0].Split();
            string agentNumber = firstLine[firstLine.Length - 1];
            // Extract the buildings from the plan that are assigned to this agent
            List<string> agentBuildings = new List<string>();

            bool foundBeginPlan = false;
            for (int i = 0; i < planLines.Length; i++)
            {
                if (planLines[i] == "ENDPLAN" && foundBeginPlan)
                {
                    agentBuildings.Add($"building {agentNumber}");
                    break;
                }
                else if (foundBeginPlan)
                {
                    agentBuildings.Add(planLines[i]);
                }
                else if (planLines[i] == $"BEGINPLAN agent number {agentNumber}")
                {
                    foundBeginPlan = true;
                }
            }

            // Set the agent's plan to the extracted buildings
            plan = agentBuildings.ToArray();
        }

        // Set the current position of the agent based on its initial position in the grid
        currentPosition = new Node(" ", Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(-transform.position.z));

        TextAsset environmentData = Resources.Load<TextAsset>(environmentTextFile);
        grid = new Grid(environmentData.text, currentPosition);
        grid.grid[currentPosition.x, currentPosition.y].walkable= true;
        FindPath();
    }


    private IEnumerator Movement()
    {
        foreach (Node n in currentPath)
        {
            Vector3 targetPosition = new Vector3(n.x, transform.position.y, -n.y);
            while (transform.position != targetPosition)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
        }
        currentBuildingIndex += 1;
        //currentPosition.symbol = " ";
        currentPosition.x = Mathf.RoundToInt(transform.position.x);
        currentPosition.y = Mathf.RoundToInt(-transform.position.z);
        FindPath();
    }

    private void FindPath()
    {
        Debug.Log(plan.Length + " " + currentBuildingIndex);
        if (currentBuildingIndex < plan.Length)
        {
            Vector2Int nextBuildingPosition = GetBuildingPosition(plan[currentBuildingIndex]);
            string building = plan[currentBuildingIndex];
            string[] buildingInfo = building.Split();
            string buildingIdentifier = buildingInfo[buildingInfo.Length - 1];
            currentDestination = grid.grid[nextBuildingPosition.x, nextBuildingPosition.y];
            currentPath = Pathfinding.FindPath(grid, currentPosition, currentDestination);
            StartCoroutine(Movement());
        }
    }


    private void Update()
    {
        /*// Check if the agent has run out of energy and destroy it if so
        if (energy <= 0)
        {
            *//*Destroy(gameObject);
            return;*//*
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
        }*/
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
}

