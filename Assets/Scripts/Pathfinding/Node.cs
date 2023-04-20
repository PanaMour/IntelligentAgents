using UnityEngine;
using System.Collections;

public class Node
{

    public bool walkable;
    public int x;
    public int y;
    public string symbol;
    public bool discovered;
    public bool visited;

    public float gCost;
    public int hCost;
    public Node parent;

    public Node(string symbol , int x, int y)
    {
        this.symbol = symbol;
        if (symbol.Equals(" "))
        {
            this.walkable = true;
        } else
        {
            this.walkable = false;
        }
        this.discovered = false;
        this.x = x;
        this.y = y;
    }

    public float fCost
    {
        get
        {
            return gCost + hCost;
        }
    }
}
