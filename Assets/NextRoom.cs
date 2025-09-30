using UnityEngine;

public class NextRoom : InteractuableGeneric
{
    private enum LeftRight
    {
        Left,
        Right
    }
    [SerializeField] private GameObject[] _rooms;
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

    public override void Activate()
    {
        InteractManager.Instance.RemoveInteract(this);
        if(GameManager.Instance.RoomsAvailable<=0)
        {
           MazeCell Room= Instantiate(_lastRoom);
           Vector3 pivot = Room.Pivot.transform.position;
           Vector3 offset = Room.transform.position - pivot;
           Room.transform.position = _mazeSpawnPoint.RigtLeftSpawnVector[(int)_leftRight].position + offset;
            Room._neighbords.Clear();
            Room._neighbords.Add(_mazeSpawnPoint);
           _mazeSpawnPoint._neighbords.Add(Room);
            return;
        }
        if (_mazeSpawnPoint != null)
        {
            MazeCell Room = Instantiate(_rooms[Random.Range(0, _rooms.Length)]).GetComponent<MazeCell>();
            Vector3 pivot = Room.Pivot.transform.position;
            Vector3 offset = Room.transform.position - pivot;
            Room.transform.position = _mazeSpawnPoint.RigtLeftSpawnVector[(int)_leftRight].position + offset;
            Room._neighbords.Clear();
            Room._pathNodesList.Clear();
            Room._neighbords.Add(_mazeSpawnPoint);
            Room._primalPathNode[2].Neighbords.Add(_mazeSpawnPoint._primalPathNode[(int)_leftRight]);
            _mazeSpawnPoint._primalPathNode[(int)_leftRight].Neighbords.Add(Room._primalPathNode[2]);
            _mazeSpawnPoint._neighbords.Add(Room);
            //OptimizerScript.instance.Activate(Room);
        }
        else
        {
            MazeCell Room = Instantiate(_rooms[Random.Range(0, _rooms.Length)]).GetComponent<MazeCell>();
            Vector3 pivot = Room.Pivot.transform.position;
            Vector3 offset = Room.transform.position - pivot;
            Room.transform.position = _spawnPoint.position + offset;
            Room._neighbords.Clear();
            Room._pathNodesList.Clear();
            Room._primalPathNode[2].Neighbords.Add(_node);
            _node.Neighbords.Add(Room._primalPathNode[2]);
        }
        GameManager.Instance.RoomsAvailable--;
        gameObject.SetActive(false);
    }

    public override void Desactivate()
    {
      _isActive = false;
    }
    private void OnDestroy()
    {
        InteractManager.Instance.RemoveInteract(this);
    }
}
