using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    public static PathFinding Instance;
    private void Awake()
    {
        Instance = this;
    }
    public List<PathNode> Theta(PathNode begin, PathNode end, Transform obj, float rad)
    {
        List<PathNode> path = AStar(begin, end);
        int current = 0;
        while (current + 2 < path.Count)
        {
            if (GameManager.Instance.SphereLineOfSight(path[current].transform.position, path[current + 2].transform.position, rad))
            {
                path.RemoveAt(current + 1);
            }
            else
            {
                current++;
            }
        }
        if (path.Count >= 2)
        {
            if (GameManager.Instance.SphereLineOfSight(obj.position, path[1].transform.position, rad))
            {
                path.RemoveAt(0);
            }
        }
        return path;
    }

    public List<PathNode> AStar(PathNode startNode, PathNode goalNode)
    {
        // Validación inicial
        if (startNode == null || goalNode == null)
        {
            //Debug.LogWarning("Pathfinder: Nodo de inicio o destino es nulo.");
            return new List<PathNode>(); // Camino vacío
        }

        // Reset de nodos para evitar basura de búsquedas anteriores
        ResetAllNodes();

        List<PathNode> openSet = new List<PathNode> { startNode };
        HashSet<PathNode> closedSet = new HashSet<PathNode>();

        startNode.gCost = 0;
        startNode.hCost = Heuristic(startNode, goalNode);
        startNode.cameFrom = null;

        while (openSet.Count > 0)
        {
            PathNode current = openSet.OrderBy(n => n.fCost).ThenBy(n => n.hCost).First();

            if (current == goalNode)
                return ReconstructPath(current);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (PathNode neighbor in current.Neighbords)
            {
                if (neighbor == null || closedSet.Contains(neighbor))
                    continue;

                float tentativeG = current.gCost + Vector3.Distance(current.transform.position, neighbor.transform.position);

                if (!openSet.Contains(neighbor) || tentativeG < neighbor.gCost)
                {
                    neighbor.gCost = tentativeG;
                    neighbor.hCost = Heuristic(neighbor, goalNode);
                    neighbor.cameFrom = current;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        // Si no encontró camino
        //Debug.LogWarning("Pathfinder: No se encontró camino al nodo destino.");
        return new List<PathNode>(); // Lista vacía
    }

    static float Heuristic(PathNode a, PathNode b)
    {
        return Vector3.Distance(a.transform.position, b.transform.position);
    }

    static List<PathNode> ReconstructPath(PathNode current)
    {
        List<PathNode> path = new List<PathNode>();
        while (current != null)
        {
            path.Add(current);
            current = current.cameFrom;
        }
        path.Reverse();
        return path;
    }

    static void ResetAllNodes()
    {

        List<PathNode> allNodes = GameManager.Instance.PathNodes;
        foreach (PathNode node in allNodes)
        {
            node.gCost = float.MaxValue;
            node.hCost = float.MaxValue;
            node.cameFrom = null;
        }
    }
}
