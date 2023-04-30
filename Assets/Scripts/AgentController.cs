using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float energy = 100f;
    public int gold = 0;
    public float delay = 1f;
    public GameObject house;
    public string planFileName;
    private Node currentPosition;
    [SerializeField] private string[] plan;
    [SerializeField] private string[] firstLine;
    private Node currentDestination;
    private int currentBuildingIndex = 0;
    private bool exploring = false;
    private bool energySearching = false;
    [SerializeField] TextAsset planData;
    private string environmentTextFile = "environment";
    [SerializeField] private Grid grid;
    [SerializeField] private List<Node> currentPath;
    [SerializeField] private HashSet<Node> energyPotLocations;
    [SerializeField] private int energyPotStorage = 0;
    private List<GameObject> tradedAgents = new List<GameObject>();
    private Animator animator;
    private void Start()
    {
        animator = GetComponent<Animator>();
        energyPotLocations = new HashSet<Node>();
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
        grid.grid[currentPosition.x, currentPosition.y].visited = true;
        grid.grid[currentPosition.x, currentPosition.y].discovered = true;

        // Find the position of the building B
        Vector2Int buildingPosition = GetBuildingPosition(plan[currentBuildingIndex]);
        if (buildingPosition != Vector2Int.zero)
        {
            // Start moving randomly until the agent discovers the location of the building
            string symbol = grid.grid[buildingPosition.x,buildingPosition.y].symbol;
            StartCoroutine(RandomMovement(buildingPosition, symbol));
        }
        else
        {
            Debug.LogError("Building not found");
        }
    }

    public float Distance(Node a, Node b)
    {
        return Mathf.Sqrt((a.x - b.x) ^ 2 + (a.y - b.y) ^ 2);
    }

    private IEnumerator RandomMovement(Vector2Int buildingPosition, string symbol)
    {
        bool foundBuilding = false;
        while (!foundBuilding)
        {
            // Get a list of neighboring nodes
            List<Node> allNeighbors = grid.GetNeighbours(currentPosition);
            energyPotLocations.UnionWith(allNeighbors.FindAll(n => n.symbol == "E"));
            if (energy <= 30)
            {
                if (energyPotStorage > 0)
                {
                    energyPotStorage--;
                    energy += 20;
                }
                else
                {
                    Node energyPotNode = null;
                    float curDistance = 99999;
                    foreach (Node node in energyPotLocations)
                    {
                        if (!node.visited && node.discovered && Distance(currentPosition, node) < curDistance)
                        {
                            energyPotNode = node;
                            curDistance = Distance(currentPosition, node);
                        }
                    }
                    if (energyPotNode != null)
                    {
                        currentDestination = energyPotNode;
                        energySearching = true;
                        currentPath = Pathfinding.FindPath(grid, currentPosition, energyPotNode);
                        StartCoroutine(Movement());
                        yield break;
                    }
                }
            }
            foreach (Node node in allNeighbors)
            {
                Debug.Log("node: " + node.x + " " + node.y);
                Debug.Log("building: " + buildingPosition.x + " " + buildingPosition.y);
                
                if (node.symbol == symbol)
                {
                    foundBuilding = true;
                    break;
                }

                GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");
                foreach (GameObject agent in agents)
                {
                    Vector3 agentPosition = agent.transform.position;
                    int agentX = Mathf.RoundToInt(agentPosition.x);
                    int agentY = Mathf.RoundToInt(-agentPosition.z);

                    if(agentX == node.x && agentY == node.y && energyPotStorage > 0)
                    {
                        SellEnergy(agent);
                    }
                    if (agentX == node.x && agentY == node.y && gold >= 20)
                    {
                        gold -= 20;
                        BuyKnowledge(agent);
                        break;
                    }
                }
            }
            if (grid.grid[buildingPosition.x, buildingPosition.y].discovered)
            {
                currentDestination = grid.grid[buildingPosition.x, buildingPosition.y];
                currentPath = Pathfinding.FindPath(grid, currentPosition, currentDestination);
                StartCoroutine(Movement());
                yield break;
            }

            List<Node> neighborsUnvisited = grid.GetWalkableNeighbours(currentPosition).FindAll(n => !n.visited);
            List<Node> neighborsVisited = grid.GetWalkableNeighbours(currentPosition).FindAll(n => n.visited);
            // Move randomly to one of the neighboring nodes
            if (neighborsUnvisited.Count > 0 && !foundBuilding)
            {
                int randomIndex = UnityEngine.Random.Range(0, neighborsUnvisited.Count);
                Node nextNode = neighborsUnvisited[randomIndex];
                
                // Update the current position and grid
                currentPosition = nextNode;
                grid.grid[currentPosition.x, currentPosition.y].discovered = true;
                grid.grid[currentPosition.x, currentPosition.y].visited = true;
                grid.grid[currentPosition.x+1, currentPosition.y].discovered = true;
                grid.grid[currentPosition.x-1, currentPosition.y].discovered = true;
                grid.grid[currentPosition.x, currentPosition.y+1].discovered = true;
                grid.grid[currentPosition.x, currentPosition.y-1].discovered = true;
                // Calculate the target position
                Vector3 targetPosition = new Vector3(currentPosition.x, transform.position.y, -currentPosition.y);
                if (nextNode.symbol == "E")
                {
                    nextNode.symbol = " ";
                    if (energy < 80)
                    {
                        energy = Mathf.Min(100, energy + 20);
                    }
                    else
                    {
                        energyPotStorage += 1;
                    }
                    
                    GameObject[] energyPots = GameObject.FindGameObjectsWithTag("Energy Pot");

                    foreach (GameObject energyPot in energyPots)
                    {
                        if (energyPot.transform.position.x == currentPosition.x && energyPot.transform.position.z == -currentPosition.y)
                        {
                            energyPot.SetActive(false);
                        }
                    }
                }
                if (nextNode.symbol == "G")
                {
                    nextNode.symbol = " ";
                    gold += 1;
                    GameObject[] goldObj = GameObject.FindGameObjectsWithTag("Gold");
                    foreach (GameObject gold1 in goldObj)
                    {
                        if (gold1.transform.position.x == currentPosition.x && gold1.transform.position.z == -currentPosition.y)
                        {
                            gold1.SetActive(false);
                        }
                    }
                }
                // Move the agent smoothly towards the target position
                while (transform.position != targetPosition)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    animator.SetBool("isMoving", true);
                    transform.LookAt(targetPosition);
                    yield return null;
                }
                animator.SetBool("isMoving", false);
                energy -= 1;
                // Wait for a short delay
                yield return new WaitForSeconds(delay);
            }
            else if(neighborsUnvisited.Count == 0 && neighborsVisited.Count > 0 && !foundBuilding)
            {
                int randomIndex = UnityEngine.Random.Range(0, neighborsVisited.Count);
                Node nextNode = neighborsVisited[randomIndex];

                // Update the current position and grid
                currentPosition = nextNode;
                grid.grid[currentPosition.x, currentPosition.y].discovered = true;
                grid.grid[currentPosition.x, currentPosition.y].visited = true;
                grid.grid[currentPosition.x + 1, currentPosition.y].discovered = true;
                grid.grid[currentPosition.x - 1, currentPosition.y].discovered = true;
                grid.grid[currentPosition.x, currentPosition.y + 1].discovered = true;
                grid.grid[currentPosition.x, currentPosition.y - 1].discovered = true;
                // Calculate the target position
                Vector3 targetPosition = new Vector3(currentPosition.x, transform.position.y, -currentPosition.y);

                // Move the agent smoothly towards the target position
                while (transform.position != targetPosition)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    animator.SetBool("isMoving", true);
                    transform.LookAt(targetPosition);
                    yield return null;
                }
                animator.SetBool("isMoving", false);
                energy -= 1;
                // Wait for a short delay
                yield return new WaitForSeconds(delay);
            }
            else if (foundBuilding)
            {
                // Once the agent discovers the location of the building, start following the plan
                currentBuildingIndex += 1;
                buildingPosition = GetBuildingPosition(plan[currentBuildingIndex]);
                currentDestination = grid.grid[buildingPosition.x, buildingPosition.y];
                currentPath = Pathfinding.FindPath(grid, currentPosition, currentDestination);
                StartCoroutine(Movement());
            }
        }
    }

    private void BuyKnowledge(GameObject agent)
    {
        if (tradedAgents.Contains(agent))
        {
            Debug.Log("Already traded with agent " + agent.name);
            return;
        }

        Node[,] agentGrid = agent.GetComponent<AgentController>().grid.grid;
        for (int i = 0; i < grid.grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.grid.GetLength(1); j++)
            {
                grid.grid[i, j].discovered = grid.grid[i, j].discovered || agentGrid[i, j].discovered;
                
            }
        }
        tradedAgents.Add(agent);
    }

    private void SellEnergy(GameObject agent)
    {
        if((agent.GetComponent<AgentController>().energy < 30 && agent.GetComponent<AgentController>().gold >= 10) || (agent.GetComponent<AgentController>().energy < 80 && agent.GetComponent<AgentController>().gold >= 30))
        {
            energyPotStorage--;
            agent.GetComponent<AgentController>().energy += 20;
            agent.GetComponent<AgentController>().gold -= 10;
            gold += 10;
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
                animator.SetBool("isMoving", true);
                transform.LookAt(targetPosition);
                yield return null;
            }
            animator.SetBool("isMoving", false);
            energy -= 1;
            if (n.symbol == "E")
            {
                n.symbol = " ";
                energy = Mathf.Min(100, energy + 20);
                GameObject[] energyPots = GameObject.FindGameObjectsWithTag("Energy Pot");

                foreach (GameObject energyPot in energyPots)
                {
                    if (energyPot.transform.position.x == n.x && energyPot.transform.position.z == -n.y)
                    {
                        energyPot.SetActive(false);
                    }
                }
            }
            else if (n.symbol == "G")
            {
                n.symbol = " ";
                gold += 1;
                GameObject[] goldObj = GameObject.FindGameObjectsWithTag("Gold");

                foreach (GameObject gold1 in goldObj)
                {
                    if (gold1.transform.position.x == n.x && gold1.transform.position.z == -n.y)
                    {
                        gold1.SetActive(false);
                    }
                }
            }
            yield return new WaitForSeconds(delay);
        }
        if (energySearching)
        {
            energySearching = false;
        }
        else
        {
            currentBuildingIndex += 1;
            //currentPosition.symbol = " ";
        }
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
            if (currentDestination.discovered)
            {
                currentPath = Pathfinding.FindPath(grid, currentPosition, currentDestination);
                StartCoroutine(Movement());
            }
            else
            {
                Vector2Int buildingPosition = GetBuildingPosition(plan[currentBuildingIndex]);
                if (buildingPosition != Vector2Int.zero)
                {
                    // Start moving randomly until the agent discovers the location of the building
                    string symbol = grid.grid[buildingPosition.x, buildingPosition.y].symbol;
                    StartCoroutine(RandomMovement(buildingPosition, symbol));
                }
                else
                {
                    Debug.LogError("Building not found");
                }
            }
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
                Vector2Int buildingPosition = GetBuildingPosition(plan[currentBuildingIndex]);
                if (buildingPosition != Vector2Int.zero)
                {
                    // Start moving randomly until the agent discovers the location of the building B
                    string symbol = grid.grid[buildingPosition.x,buildingPosition.y].symbol;
            StartCoroutine(RandomMovement(buildingPosition, symbol));
                }
                else
                {
                    Debug.LogError("Building not found");
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

