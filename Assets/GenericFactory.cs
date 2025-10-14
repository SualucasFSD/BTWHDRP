using AYellowpaper.SerializedCollections;
using UnityEngine;

public class GenericFactory : MonoBehaviour
{
    public static GenericFactory Instance { get; private set; }

    private EnemyPool<Entity> _pool;

    [SerializedDictionary("EnemyCatalogue", "Entity")]
    public SerializedDictionary<EnemyCatalogue, Entity> _prefabs = new SerializedDictionary<EnemyCatalogue, Entity>();

    [SerializeField] private int _initialAmount = 3;
    [SerializeField] private Transform _parent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        _pool = new EnemyPool<Entity>(
            InstantiatePrefab,
            (e) => e.gameObject.SetActive(false),
            (e) =>
            {
                e.transform.hasChanged = false;
                e.gameObject.SetActive(true);
            },
            _prefabs.Keys,
            _initialAmount
        );
    }

    private Entity InstantiatePrefab(EnemyCatalogue type)
    {
        Entity prefab = _prefabs[type];
        Entity instance = Instantiate(prefab, Vector3.zero, Quaternion.identity, _parent);
        instance.gameObject.SetActive(false);
        return instance;
    }

    public void ReturnObj(EnemyCatalogue type, Entity obj)
    {
        _pool.ReturnObj(type, obj);
    }

    public Entity GetObj(EnemyCatalogue type, Vector3 pos)
    {
        return _pool.GetObject(type, pos);
    }
}
