using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class MazeCell : MonoBehaviour
{
    public GameObject Pivot;
    [SerializeField] private GameObject _principalDoor;
    public Transform[] RigtLeftSpawnVector = new Transform[2];
    public PathNode[] _primalPathNode;
    public List<PathNode> _pathNodesList = new List<PathNode>();
    public List<MazeCell> _neighbords = new List<MazeCell>();
    [SerializeField] private GameObject _lights;
    [SerializeField] private List<GameObject> _enemies;
    private NextRoom[] _nextRooms;
    private bool _isActive = false;
    [SerializeField] private GameObject _finalBox;
    [SerializeField] AcquireAbility _Text;

    private void Awake()
    {
        _pathNodesList = new List<PathNode>();
        _neighbords = new List<MazeCell>();
    }

    private void Start()
    {
        OptimizerScript.instance.MazeCells.Add(this);

        if (_principalDoor != null)
            _principalDoor.SetActive(false);
    }

    private IEnumerator SpawnEnemies()
    {
        yield return null;
        yield return null;

        int enemyCount = Mathf.FloorToInt(5 * GameManager.Instance.DificultLevel);
        int pathNodeCount = _pathNodesList.Count;

        if (pathNodeCount == 0)
        {
            yield break;
        }

        List<int> shuffledIndices = new List<int>();
        for (int i = 0; i < pathNodeCount; i++)
            shuffledIndices.Add(i);
        for (int i = 0; i < shuffledIndices.Count; i++)
        {
            int rand = Random.Range(i, shuffledIndices.Count);
            (shuffledIndices[i], shuffledIndices[rand]) = (shuffledIndices[rand], shuffledIndices[i]);
        }

        for (int i = 0; i < enemyCount; i++)
        {
            int nodeIndex = shuffledIndices[i % pathNodeCount];
            Transform node = _pathNodesList[nodeIndex].transform;

            Vector3 spawnPosition = node.position + Vector3.up * 1f;

            EnemyCatalogue selectedType = (Random.value < 0.5f)
                ? EnemyCatalogue.Mague
                : EnemyCatalogue.SavageDog;

            Entity enemy = GenericFactory.Instance.GetObj(selectedType, spawnPosition);
            _enemies.Add(enemy.gameObject);
            enemy.Cell = this;
        }

        GameManager.Instance.DificultLevel += 0.1f;
    }

    public void PathNodeRefresh()
    {
        _nextRooms = GetComponentsInChildren<NextRoom>();

        foreach (NextRoom r in _nextRooms)
            r.enabled = false;

        if (_pathNodesList == null || _pathNodesList.Count <= 0)
            print("No Se Cargo");
        else
            print(_pathNodesList.Count);

        foreach (PathNode node in _pathNodesList)
        {
            foreach (PathNode n in _pathNodesList)
            {
                if (node == n) continue;

                if (GameManager.Instance.SphereLineOfSight(node.transform.position, n.transform.position, 0.6f))
                    node.Neighbords.Add(n);
            }
        }
    }

    public void TurnOnLight()
    {
        if (_lights == null) return;

        Transform[] trs = _lights.GetComponentsInChildren<Transform>(true);
        List<GameObject> objs = new List<GameObject>();

        foreach (Transform t in trs)
        {
            if (t != _lights.transform)
                objs.Add(t.gameObject);
        }

        StartCoroutine(LightsRoutine(objs));
    }

    IEnumerator LightsRoutine(List<GameObject> objs)
    {
        foreach (GameObject l in objs)
        {
            if (!l.activeSelf)
                l.SetActive(true);

            yield return null;
        }
    }
    public void TurnOfMazeCell()
    {
        foreach (GameObject p in _enemies)
            p.SetActive(false);

        gameObject.SetActive(false);
    }

    public void TurnOnMazeCell()
    {
        gameObject.SetActive(true);

        foreach (GameObject p in _enemies)
            p.SetActive(true);
    }

    private void OnDestroy()
    {
        OptimizerScript.instance.MazeCells.Remove(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isActive) return;
        
        PlayerController p = other.gameObject.GetComponent<PlayerController>();
        if (p != null)
        {
            //OptimizerScript.instance.OnlyKeep(this);
            OptimizerScript.instance.Refresh(this);
            _principalDoor.SetActive(true);
            StartCoroutine(SpawnEnemies());
            OptimizerScript.instance.OnlyKeep(this);
            _isActive = true;
        }
    }

    public void OnEnemyKilledInside(GameObject p)
    {
        _enemies.Remove(p);
        Comprobate();
    }

    private void Comprobate()
    {
        if (_enemies.Count <= 0)
        {
            foreach (NextRoom r in _nextRooms)
                r.enabled = true;

            if (_finalBox != null)
            {
                _Text = GameObject.Find("GameManager").gameObject.GetComponent<AcquireAbility>();
                if (_Text != null)
                    StartCoroutine(_Text.OnAbilityAcquired());

                GameObject p = _pathNodesList[Random.Range(0, _pathNodesList.Count)].gameObject;
                Instantiate(_finalBox, p.transform.position, Quaternion.Euler(0, p.transform.rotation.y, 0));
            }
        }
    }

    public void DesactivateDoor()
    {
        foreach (NextRoom r in _nextRooms)
            r.enabled = false;
    }
}
