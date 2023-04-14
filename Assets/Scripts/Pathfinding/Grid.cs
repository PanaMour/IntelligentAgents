using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class Grid
{
    public int sizeX;
    public int sizeY;
    public Node[,] grid;

    public Grid(string environment, Node position)
    {
        InitializeGrid(environment,position);
    }

    Node[,] InitializeGrid(string environment, Node position)
    {
        string[] lines = environment.Split('\n');
        this.sizeX = lines.Length;
        this.sizeY = lines[0].Length - 1; // Subtract 1 for the newline character
        grid = new Node[sizeX, sizeY];
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                string symbol = lines[i][j].ToString();
                Node node = new Node(symbol, j, i);
                //node.gCost = EuclideanDistance(position, node);
                grid[j, i] = node;
            }
        }
        return grid;
    }

    float EuclideanDistance(Node a, Node b)
    {
        return 10 * Mathf.Sqrt((a.x-b.x)^2+(a.y-b.y)^2);
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        if (node.x + 1 < sizeX - 1) 
        {
            neighbours.Add(grid[node.x + 1, node.y]); 
        }
        if (node.y + 1 < sizeY - 1)
        {
            neighbours.Add(grid[node.x, node.y + 1]);
        }
        if (node.x - 1 >= 0)
        {
            neighbours.Add(grid[node.x - 1, node.y]);
        }
        if (node.y - 1 >= 0)
        {
            neighbours.Add(grid[node.x, node.y - 1]);
        }

        foreach(Node n in neighbours)
        {
            if (node.x == 5 && node.y == 5) Debug.Log("DESTINATION FOUND");
        }
        return neighbours;
    }

    public List<Node> GetWalkableNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        if (node.x + 1 < sizeX - 1 && grid[node.x + 1, node.y].walkable)
        {
            neighbours.Add(grid[node.x + 1, node.y]);
        }
        if (node.y + 1 < sizeY - 1 && grid[node.x , node.y + 1].walkable)
        {
            neighbours.Add(grid[node.x, node.y + 1]);
        }
        if (node.x - 1 >= 0 && grid[node.x - 1, node.y].walkable)
        {
            neighbours.Add(grid[node.x - 1, node.y]);
        }
        if (node.y - 1 >= 0 && grid[node.x, node.y - 1].walkable)
        {
            neighbours.Add(grid[node.x, node.y - 1]);
        }

        foreach (Node n in neighbours)
        {
            if (node.x == 5 && node.y == 5) Debug.Log("DESTINATION FOUND");
        }
        return neighbours;
    }
}
