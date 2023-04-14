using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float energy = 100f;
    public float delay = 0.1f;
    public GameObject house;
    public string planFileName;
    private Node currentPosition;
    private Vector2Int nextPosition;
    [SerializeField] private string[] plan;
    [SerializeField] private string[] firstLine;
    Node currentDestination;
    private int currentBuildingIndex = 0;
    private bool exploring = false;
    [SerializeField] TextAsset planData;
    private string environmentTextFile = "environment";
    [SerializeField] private Grid grid;
    [SerializeField] private List<Node> currentPath;
    private void Start()
    {
        planFileName = "agent_" + gameObject.name.Split('_')[1] + "_plan";
        planData = Resources.Load<TextAsset>(planFileName);
        if (planData != null)
        {
            // Split the plan by lines
            string[] planLines = planData.text.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

            // Extract the agent number from the first line of the plan
            string[] firstLine = planLines[0].Split();
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
        grid.grid[currentPosition.x, currentPosition.y].walkable = true;

        // Find the position of the building B
        Vector2Int buildingBPosition = GetBuildingPosition("building B");
        Debug.Log(buildingBPosition);
        if (buildingBPosition != Vector2Int.zero)
        {
            // Start moving randomly until the agent discovers the location of the building B
            StartCoroutine(RandomMovement(buildingBPosition));
        }
        else
        {
            Debug.LogError("Building B not found");
        }
    }

    private IEnumerator RandomMovement(Vector2Int buildingBPosition)
    {
        bool foundBuilding = false;
        while (!foundBuilding)
        {
            // Get a list of neighboring nodes
            List<Node> allNeighbors = grid.GetNeighbours(currentPosition);
            
            foreach(Node node in allNeighbors)
            {
                Debug.Log("node: "+node.x + " " + node.y);
                Debug.Log("building: " + buildingBPosition.x + " " + buildingBPosition.y);
                if(node.x == buildingBPosition.x && node.y == buildingBPosition.y)
                {
                    foundBuilding = true;
                    break;
                }
            }
            List<Node> neighbors = grid.GetWalkableNeighbours(currentPosition);
            // Move randomly to one of the neighboring nodes
            if (neighbors.Count > 0 && !foundBuilding)
            {
                int randomIndex = Random.Range(0, neighbors.Count);
                Node nextNode = neighbors[randomIndex];

                // Update the current position and grid
                currentPosition = nextNode;
                grid.grid[currentPosition.x, currentPosition.y].discovered = true;

                // Calculate the target position
                Vector3 targetPosition = new Vector3(currentPosition.x, transform.position.y, -currentPosition.y);

                // Move the agent smoothly towards the target position
                while (transform.position != targetPosition)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    yield return null;
                }
                energy -= 1;
                // Wait for a short delay
                yield return new WaitForSeconds(delay);
            }
            else if (foundBuilding)
            {
                // Once the agent discovers the location of the building B, start following the plan
                currentDestination = grid.grid[buildingBPosition.x, buildingBPosition.y];
                currentPath = Pathfinding.FindPath(grid, currentPosition, currentDestination);
                StartCoroutine(Movement());
            }
        }
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
            energy -= 1;
            yield return new WaitForSeconds(delay);
        }
        currentBuildingIndex += 1;
        //currentPosition.symbol = " ";
        currentPosition.x = Mathf.RoundToInt(transform.position.x);
        currentPosition.y = Mathf.RoundToInt(-transform.position.z);
        FindPath();
    }

    private void FindPath()
    {
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
        if (energy <= 0)
        {
            Destroy(gameObject);
            return;
        }

        if (exploring)
        {
            if (currentPath == null)
            {
                Vector2Int buildingBPosition = GetBuildingPosition("building B");
                if (buildingBPosition != Vector2Int.zero)
                {
                    // Start moving randomly until the agent discovers the location of the building B
                    StartCoroutine(RandomMovement(buildingBPosition));
                }
                else
                {
                    Debug.LogError("Building B not found");
                }
            }
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
    // Bind the agent to a house
    public void SetHouse(GameObject house)
    {
        this.house = house;
    }
}

