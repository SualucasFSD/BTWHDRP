using System.Collections.Generic;
using UnityEngine;

public class MazeCell : MonoBehaviour
{
    public GameObject Pivot;
    [SerializeField] private GameObject _principalDoor;
    public Transform[] RigtLeftSpawnVector = new Transform[2];
    public PathNode[] _primalPathNode;
    public List<PathNode> _pathNodesList=new List<PathNode>();
    public List<MazeCell> _neighbords=new List<MazeCell>();
    [SerializeField] private GameObject _lights;
    [SerializeField] private List<GameObject> _enemies;
    private NextRoom[] _nextRooms;
    private void Awake()
    {
        _pathNodesList=new List<PathNode> ();
        _neighbords=new List<MazeCell>();
    }
    private void Start()
    {
        OptimizerScript.instance.MazeCells.Add(this);
    }
    public void PathNodeRefresh()
    {
        _nextRooms = GetComponentsInChildren<NextRoom>();
        foreach (NextRoom r in _nextRooms)
        {
            r.enabled = false;
        }
        if (_pathNodesList.Count <= 0 || _pathNodesList == null)
        {
            print("No Se Cargo");
        }
        else
        {
            print(_pathNodesList.Count);
        }
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
       OptimizerScript.instance.MazeCells.Remove(this);
    }
    private void OnTriggerEnter(Collider other)
    {
        PlayerController p = other.gameObject.GetComponent<PlayerController>();
        if (p != null) { OptimizerScript.instance.Refresh(this); print("Refreshing"); _principalDoor.SetActive(true); return; }
        Entity r=other.gameObject.GetComponent<Entity>();
        if(r != null)
        {
            print("Enemy In"+ gameObject.name+r.name);
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
        if (p != null) {return; }
        Entity r = other.gameObject.GetComponent<Entity>();
        if (r != null)
        {
            print("Enemy Out" + gameObject.name+ r.name);
            _enemies.Remove(r.gameObject);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (_primalPathNode[2]!=null)
        {
            Gizmos.DrawWireCube(_primalPathNode[2].transform.position, new Vector3(1, 1, 1));
        }
    }
    public void OnEnemyKilledInside(GameObject p)
    {
        _enemies.Remove(p);
        Comprobate();
    }
    private void Comprobate()
    {
      if(_enemies.Count<=0)
      {
            foreach (NextRoom r in _nextRooms)
            {
                r.enabled = true;
            }
        }
    }
}
