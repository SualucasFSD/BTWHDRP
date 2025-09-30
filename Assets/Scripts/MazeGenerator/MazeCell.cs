using System.Collections.Generic;
using UnityEngine;

public class MazeCell : MonoBehaviour
{
    [SerializeField]
    public GameObject Pivot;
    private GameObject[] _walls = new GameObject[4];
    //public int[] MazeArrayPosition = new int[2];
    public Transform[] RigtLeftSpawnVector = new Transform[2];
    public bool IsVisited=false;
    public PathNode[] _primalPathNode;
    public List<PathNode> _pathNodesList=new List<PathNode>();
    public List<MazeCell> _neighbords=new List<MazeCell>();
    public Transform DDD;
    private Color _color;
    [SerializeField] private GameObject _lights;
    [SerializeField] private List<GameObject> _enemies;
    [SerializeField] private SpawnEnemies _spawn;
    private void Start()
    {
        OptimizerScript.instance.MazeCells.Add(this);
        _color = Random.ColorHSV();
    }
    public void Visit()
    {
        IsVisited = true;
    }
    public void ClearWall(int i)
    {
        _walls[i].SetActive(false);
        //Destroy(_walls[i]);
    }
    public void PathNodeRefresh()
    {
       foreach(PathNode node in _pathNodesList)
        {
            foreach(PathNode n in _pathNodesList)
            {
                if(node==n)
                {
                    continue;
                }
                if(GameManager.Instance.SphereLineOfSight(node.transform.position, n.transform.position, 0.6f))
                {
                    node.Neighbords.Add(n);
                }
            }
        }
    }
    public void TurnOnLight()
    {    if (_lights != null)
        {
            _lights.SetActive(true);
        }
    }
    public void TurnOfMazeCell()
    {
        foreach(GameObject p in _enemies)
        {
            p.SetActive(false);
        }
        gameObject.SetActive(false);
    }
    public void TurnOnMazeCell()
    {
        gameObject.SetActive(true);
        foreach (GameObject p in _enemies)
        {
            p.SetActive(true);
        }
    }
    private void OnDestroy()
    {
        if(OptimizerScript.instance.MazeCells.Contains(this))
        {
            OptimizerScript.instance.MazeCells.Remove(this);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        PlayerController p = other.gameObject.GetComponent<PlayerController>();
        if (p != null) { OptimizerScript.instance.Refresh(this); return; }
        Entity r=other.gameObject.GetComponent<Entity>();
        if(r != null)
        {
            if(_enemies.Contains(r.gameObject))
            {
                return;
            }
            _enemies.Add(r.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        PlayerController p = other.gameObject.GetComponent<PlayerController>();
        if (p != null) {return;}
        Entity r = other.gameObject.GetComponent<Entity>();
        if (r != null)
        {
            _enemies.Remove(r.gameObject);
        }
    }

    /*private void OnDrawGizmos()
    {
        Gizmos.color = _color;
        foreach (var N in _neighbords) { Gizmos.DrawLine(DDD.position, N.DDD.position); }
    }*/
}
