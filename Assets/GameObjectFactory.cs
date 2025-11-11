using AYellowpaper.SerializedCollections;
using UnityEngine;
public enum GenericObjectType
{
    BloodDecal,
    MagueBullet,
    ThunderEffect,
    PoisonEffect,
    BigBullet,
    Item,
    Obstacle
}
public class GameObjectFactory : MonoBehaviour
{

    public static GameObjectFactory Instance { get; private set; }

    private GenericObjectPool<GameObject> _pool;

    [SerializedDictionary("Type", "Prefab")]
    public SerializedDictionary<GenericObjectType, GameObject> _prefabs = new SerializedDictionary<GenericObjectType, GameObject>();

    [SerializeField] private int _initialAmount = 5;
    [SerializeField] private Transform _parent;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        _pool = new GenericObjectPool<GameObject>(
            InstantiatePrefab,
            (go) => go.SetActive(false),
            (go) => go.SetActive(true),
            _prefabs.Keys,
            _initialAmount
        );
    }

    private GameObject InstantiatePrefab(GenericObjectType type)
    {
        if (!_prefabs.ContainsKey(type))
        {
            Debug.LogWarning($"No prefab found for type {type}");
            return null;
        }

        GameObject prefab = _prefabs[type];
        GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity, _parent);
        instance.SetActive(false);
        return instance;
    }

    public GameObject GetObj(GenericObjectType type, Vector3 pos, Quaternion rot)
    {
        GameObject obj = _pool.GetObject(type, pos, rot);
        return obj;
    }

    public void ReturnObj(GenericObjectType type, GameObject obj)
    {
        obj.transform.parent= _parent;
        _pool.ReturnObj(type, obj);
    }
}
