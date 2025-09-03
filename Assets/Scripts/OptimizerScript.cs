using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class OptimizerScript : MonoBehaviour
{
    public static OptimizerScript instance;
    public List<MazeCell> MazeCells = new List<MazeCell>();
    private List<PathNode> _pathNodes = new List<PathNode>();
    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    public void Refresh(MazeCell p)
    {
        p.TurnOnLight();
        _pathNodes.Clear();
        foreach(MazeCell cell in MazeCells)
        {
           if(p._neighbords.Contains(cell))
           {
                cell.TurnOnMazeCell();  
                cell.TurnOnLight();
                foreach (PathNode node in cell._pathNodesList)
                {
                    _pathNodes.Add(node);
                }
           }
           else if(cell!=p) { cell.TurnOfMazeCell(); }
        }
        foreach(PathNode n in p._pathNodesList)
        {
            _pathNodes.Add(n);
        }
        p.TurnOnMazeCell();
        GameManager.Instance.PathNodes = _pathNodes.Concat(GameManager.Instance.PreLoadPathNodes).ToList();
    }
    public void Activate(MazeCell mid)
    {
        StartCoroutine(PathfindingNodeRefresh(mid));
    }
    private IEnumerator PathfindingNodeRefresh(MazeCell mid)
    {
        foreach(MazeCell m in MazeCells)
        {
           m.PathNodeRefresh();
           yield return null;
        }
        Refresh(mid);
    }
    private void OnDestroy()
    {

    }
}
