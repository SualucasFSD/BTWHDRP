using System.Collections;
using UnityEngine;

public class NextRoom : InteractuableGeneric
{
    private enum LeftRight
    {
        Left,
        Right
    }
    [SerializeField] private MazeCell[] _rooms;
    [Header("Si es un modulo random")]
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private PathNode _node;
    [Header("Si es desde un mazecell")]
    [SerializeField] private MazeCell _mazeSpawnPoint;
    [SerializeField] private LeftRight _leftRight;
    [SerializeField] private MazeCell _lastRoom;
    public override void Interacting()
    {
        base.Interacting();
    }
    private void Start()
    {
        InteractManager.Instance.AddInteract(this);
        if (GameManager.Instance.RoomsAvailable < 0)
        {
          InteractManager.Instance.RemoveInteract(this);
        }
    }
    public override void Activate()
    {
        InteractManager.Instance.RemoveInteract(this);
        if(GameManager.Instance.RoomsAvailable==0)
        {
           MazeCell Room= Instantiate(_lastRoom);
           Vector3 pivot = Room.Pivot.transform.position;
           Vector3 offset = Room.transform.position - pivot;
           Room.transform.position = _mazeSpawnPoint.RigtLeftSpawnVector[(int)_leftRight].position + offset;
           Room._neighbords.Add(_mazeSpawnPoint);
           _mazeSpawnPoint._neighbords.Add(Room);
            return;
        }
        if (_mazeSpawnPoint != null)
        {
            MazeCell Room = Instantiate(_rooms[Random.Range(0, _rooms.Length)]);
            Vector3 pivot = Room.Pivot.transform.position;
            Vector3 offset = Room.transform.position - pivot;
            Room.transform.position = _mazeSpawnPoint.RigtLeftSpawnVector[(int)_leftRight].position + offset;
            Room._neighbords.Add(_mazeSpawnPoint);
            Room._primalPathNode[2].Neighbords.Add(_mazeSpawnPoint._primalPathNode[(int)_leftRight]);
            _mazeSpawnPoint._primalPathNode[(int)_leftRight].Neighbords.Add(Room._primalPathNode[2]);
            _mazeSpawnPoint._neighbords.Add(Room);
            StartCoroutine(Active(Room));
        }
        else
        {
            MazeCell Room = Instantiate(_rooms[Random.Range(0, _rooms.Length)]);
            Vector3 pivot = Room.Pivot.transform.position;
            Vector3 offset = Room.transform.position - pivot;
            Room.transform.position = _spawnPoint.position + offset;
            Room._primalPathNode[2].Neighbords.Add(_node);
            _node.Neighbords.Add(Room._primalPathNode[2]);
            StartCoroutine(Active(Room));
        }
        GameManager.Instance.RoomsAvailable--;
        //Codigo Para Abrir Puerta
        //gameObject.SetActive(false);
    }
    private IEnumerator Active(MazeCell p)
    {
        yield return null;
        if (OptimizerScript.instance != null)
        {
            OptimizerScript.instance.Activate(p);
        }
        yield return null;
        gameObject.SetActive(false);
    }
    public override void Desactivate()
    {
      
    }
    private void OnDestroy()
    {
        InteractManager.Instance.RemoveInteract(this);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position,new Vector3(1,1,1));
        //Gizmos.DrawWireSphere(transform.position, 2);
    }
}
