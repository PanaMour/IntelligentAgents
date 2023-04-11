using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    public int sizeX;
    public int sizeY;
    public Node[,] grid;

    public Grid(string environment)
    {
        InitializeGrid(environment);
    }

    Node[,] InitializeGrid(string environment)
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
                Node node = new Node(symbol, i, j);
                grid[i, j] = node;
            }
        }
        return grid;
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        Debug.Log("(X,Y) = ("+node.x + "," + node.y + ")");
        Debug.Log("SizeX " + sizeX);
        Debug.Log("SizeY " + sizeY);
        if (node.x + 1 < sizeX-1) neighbours.Add(grid[node.x + 1, node.y]);
        if (node.y + 1 < sizeY-1) neighbours.Add(grid[node.x, node.y + 1]);
        if (node.x - 1 >= 0) neighbours.Add(grid[node.x - 1, node.y]);
        if (node.y - 1 >= 0) neighbours.Add(grid[node.x, node.y - 1]);
        return neighbours;
    }
}
