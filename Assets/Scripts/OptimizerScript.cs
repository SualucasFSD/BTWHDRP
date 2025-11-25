/*using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class OptimizerScript : MonoBehaviour
{
    public static OptimizerScript instance;
    [SerializeField] private GameObject _Hub;
    public List<MazeCell> MazeCells = new List<MazeCell>();
    private List<PathNode> _pathNodes = new List<PathNode>();
    private void Awake()
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
           else if(cell!=p)
           {
                print("Desactivo " + cell.name);
                cell.TurnOfMazeCell();
           }
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
        mid.PathNodeRefresh();
        Refresh(mid);
    }
}*/
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OptimizerScript : MonoBehaviour
{
    public static OptimizerScript instance;

    [SerializeField] private GameObject _hub;
    public List<MazeCell> MazeCells = new List<MazeCell>();

    private List<PathNode> _pathNodes = new List<PathNode>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void OnlyKeep(MazeCell activeCell)
    {
        _pathNodes.Clear();

        if(_hub!=null)
        {
            _hub.SetActive(false);
        }

        foreach (MazeCell cell in MazeCells)
        {
            if (cell == activeCell)
            {
                foreach (var node in cell._pathNodesList)
                    _pathNodes.Add(node);
            }
            else
            {
                cell.TurnOfMazeCell();
            }
        }

        GameManager.Instance.PathNodes = _pathNodes.Concat(GameManager.Instance.PreLoadPathNodes).ToList();
    }

    public void Refresh(MazeCell p)
    {
        p.TurnOnLight();
        _pathNodes.Clear();

        foreach (MazeCell cell in MazeCells)
        {
            if (p._neighbords.Contains(cell))
            {
                cell.TurnOnMazeCell();
                cell.TurnOnLight();

                foreach (PathNode node in cell._pathNodesList)
                    _pathNodes.Add(node);
            }
            else if (cell != p)
            {
                cell.TurnOfMazeCell();
            }
        }

        foreach (PathNode n in p._pathNodesList)
            _pathNodes.Add(n);

        p.TurnOnMazeCell();

        GameManager.Instance.PathNodes =
            _pathNodes.Concat(GameManager.Instance.PreLoadPathNodes).ToList();
    }

    public void Activate(MazeCell mid)
    {
        mid.PathNodeRefresh();
        Refresh(mid);
    }
}
