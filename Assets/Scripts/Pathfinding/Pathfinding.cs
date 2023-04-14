using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class Pathfinding
{
    public static List<Node> FindPath(Grid grid, Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        int myInt;
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while(openSet.Count > 0)
        {
            Node node = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < node.fCost || openSet[i].fCost == node.fCost)
                {
                    if (openSet[i].hCost < node.hCost)
                        node = openSet[i];
                }
            }

            openSet.Remove(node);
            closedSet.Add(node);

            List<Node> neighs = grid.GetNeighbours(node);
            foreach (Node neighbour in neighs)
            {
                if(neighbour==endNode && int.TryParse(neighbour.symbol, out myInt))
                {
                    endNode.parent = node;
                    return RetracePath(startNode, endNode);
                }
                else if(neighbour == endNode)
                {
                    return RetracePath(startNode, node);
                }
         
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

                float newCostToNeighbour = node.gCost + GetDistance(node, neighbour);
                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, endNode);
                    neighbour.parent = node;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }
        return new List<Node>();
    }

    static List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }



    static int GetDistance(Node a, Node b)
    {
        return 10*Mathf.Abs(a.x - b.x) + 10*Mathf.Abs(a.y - b.y);
    }
}
