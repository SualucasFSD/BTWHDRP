using System.Collections;
using System.Collections.Generic;
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
        //InteractManager.Instance.AddInteract(this);
        /*if (GameManager.Instance.RoomsAvailable < 0)
        {
          InteractManager.Instance.RemoveInteract(this);
            Destroy(this);
        }*/
    }
    private void OnEnable()
    {
        InteractManager.Instance.AddInteract(this);
    }
    private void OnDisable()
    {
        InteractManager.Instance.RemoveInteract(this);
    }
    public override void Activate()
    {
        if (_interactObj != null)
        {
            foreach (var obj in _interactObj)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }

        InteractManager.Instance?.RemoveInteract(this);

        var gm = GameManager.Instance;

        if (_rooms == null || _rooms.Length == 0)
        {
            Debug.LogWarning("No hay salas disponibles en _rooms.");
            return;
        }

        MazeCell prefab = gm.RoomsAvailable == 0 ? _lastRoom : _rooms[Random.Range(0, _rooms.Length)];
        if (prefab == null)
        {
            Debug.LogWarning("El prefab seleccionado es nulo.");
            return;
        }

        MazeCell room = Instantiate(prefab);
        room.transform.rotation = transform.rotation;

        if (room.Pivot == null)
        {
            Debug.LogWarning($"La sala '{room.name}' no tiene asignado un Pivot.");
            return;
        }
        Vector3 offset = room.transform.position - room.Pivot.transform.position;

        Vector3 spawnPos = (_mazeSpawnPoint != null && _mazeSpawnPoint.RigtLeftSpawnVector != null &&(int)_leftRight >= 0 &&(int)_leftRight < _mazeSpawnPoint.RigtLeftSpawnVector.Length &&_mazeSpawnPoint.RigtLeftSpawnVector[(int)_leftRight] != null)? _mazeSpawnPoint.RigtLeftSpawnVector[(int)_leftRight].position: _spawnPoint != null ? _spawnPoint.position : transform.position;

        room.transform.position = spawnPos + offset;

        if (_mazeSpawnPoint != null)
        {
            _mazeSpawnPoint.DesactivateDoor();
            if (room._neighbords == null) room._neighbords = new List<MazeCell>();
            if (_mazeSpawnPoint._neighbords == null) _mazeSpawnPoint._neighbords = new List<MazeCell>();

            room._neighbords.Add(_mazeSpawnPoint);
            _mazeSpawnPoint._neighbords.Add(room);

            if (_mazeSpawnPoint._primalPathNode != null &&
                (int)_leftRight >= 0 &&
                (int)_leftRight < _mazeSpawnPoint._primalPathNode.Length)
            {
                var primalNode = _mazeSpawnPoint._primalPathNode[(int)_leftRight];
                if (primalNode != null &&
                    room._primalPathNode != null &&
                    room._primalPathNode.Length > 2 &&
                    room._primalPathNode[2] != null)
                {
                    room._primalPathNode[2].Neighbords.Add(primalNode);
                    primalNode.Neighbords.Add(room._primalPathNode[2]);
                }
            }
        }
        else
        {
            if (room._primalPathNode != null && room._primalPathNode.Length > 2 && room._primalPathNode[2] != null && _node != null)
            {
                room._primalPathNode[2].Neighbords.Add(_node);
                _node.Neighbords.Add(room._primalPathNode[2]);
            }
        }

        StartCoroutine(Active(room));

        gm.DificultLevel += 0.5f;

        if (gm.RoomsAvailable > 0)
        {
            gm.RoomsAvailable--;
        }
        // Código adicional (por ejemplo abrir puertas)
        // gameObject.SetActive(false);
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
