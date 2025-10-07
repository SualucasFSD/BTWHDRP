using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum EnemyCatalogue
{
    Esqueleton,
    Mague,
    SavageDog
}
public enum PjPower
{
    EsqueletonDagger,
    FirstMagueClass
}
public class GameManager : MonoBehaviour
{
    [SerializeField] private ShaderVariantCollection svc;
    public List<EnemyStats> ConfigurationEnemys = new List<EnemyStats>();
    public Dictionary<EnemyCatalogue, EnemyStats> EnemyConfiguration = new Dictionary<EnemyCatalogue, EnemyStats>();
    private HashSet<PjPower> _powers;
    public List<PathNode> PathNodes = new List<PathNode>();
    public List<PathNode> PreLoadPathNodes = new List<PathNode>();
    public static GameManager Instance;
    [SerializeField] private LayerMask _blockLayer;
    [SerializeField] private LayerMask _nodeLayerMask;
    private List<Entity> _enemy = new List<Entity>();
    private List<Entity> _allies = new List<Entity>();
    public bool IsPaused = false;
    public float Timer = 0;
    public Transform Gameplay;
    public Action GenericUpdate = delegate { };
    public Action GenericFixedUpdate = delegate { };
    public float LaunchAngle = 45f;
    public int RoomsAvailable;
    public float DificultLevel=1;
    private void FixedUpdate()
    {
        if (GenericFixedUpdate != null) { GenericFixedUpdate(); }
    }
    private void Update()
    {
        if(GenericUpdate != null) { GenericUpdate(); }
        //print(_enemy.Count + " Enemigos");
        //print(_allies.Count + " Ayudantes");
    }

    public void ReloadShaders()
    {
        if (svc != null)
        {
            Debug.Log("Precargando shaders...");
            svc.WarmUp();
            Debug.Log("Shaders precargados");
        }
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
      foreach(EnemyStats e in ConfigurationEnemys)
      {
         EnemyConfiguration.Add(e.Kind,e);
      }
    }
    private void Start()
    {
        StartCoroutine(PatNodeInitialize());
    }
    public bool FieldOfView(GameObject Caster, GameObject Target, float AreaOfVision, float Distance, float rad)
    {
        if (Vector3.Distance(Target.transform.position, Caster.transform.position) < Distance)
        {
            float backFrontAngle = Vector3.Dot(Caster.transform.forward, (Target.transform.position - Caster.transform.position).normalized);
            if (backFrontAngle > AreaOfVision) { return SphereLineOfSight(Caster.transform.position, Target.transform.position, rad); }
            else { return false; }
        }
        else
        {
            return false;
        }
    }
    public bool SphereLineOfSight(Vector3 obj, Vector3 tg, float rad)
    {
        Vector3 Dir = tg - obj;
        return !Physics.SphereCast(origin: obj, radius: rad, direction: Dir, maxDistance: Dir.magnitude, layerMask: _blockLayer, hitInfo: out RaycastHit p);
    }
    public bool LineOfSight(Vector3 obj, Vector3 tg)
    {
        Vector3 Dir = tg - obj;
        return !Physics.Raycast(obj, Dir, Dir.magnitude, _blockLayer);
    }
    public PathNode GetCloseNode(Transform _Obj)
    {
        float Distance = Mathf.Infinity;
        PathNode CloseNode = null;
        Vector3 dir;
        foreach (PathNode node in PathNodes)
        {
            dir = node.transform.position - _Obj.transform.position;
            if (dir.magnitude < Distance)
            {
                if (LineOfSight(node.transform.position, _Obj.position))
                {
                    CloseNode = node;
                    Distance = dir.magnitude;
                }
            }
        }
        if (CloseNode == null)
        {
            Distance = Mathf.Infinity;
            foreach (PathNode node in PathNodes)
            {
                dir = node.transform.position - _Obj.transform.position;
                if (dir.magnitude < Distance)
                {
                    CloseNode = node;
                    Distance = dir.magnitude;
                }
            }
        }
        return CloseNode;
    }

    private IEnumerator PatNodeInitialize()
    {
        while (Timer < 3)
        {
            Timer += 1f;
            yield return new WaitForSeconds(1f);
        }
        Timer = 0;
        RaycastHit point;
        foreach (PathNode node in PreLoadPathNodes)
        {
            if (Physics.Raycast(node.transform.position, -Vector3.up, out point))
            {
                node.transform.position = point.point + Vector3.up * 1.5f;
            }
            yield return null;
        }
        EventManager.Ejecute(EventManager.KindOfEvent.ReloadNodes);
        if(PathNodes.Count <= 0) { PathNodes = PreLoadPathNodes; }
    }
    public bool OnView(GameObject P, float ViewDistance, float VisionAngle, float Radius, List<Entity> Targets)
    {
        foreach (Entity j in Targets)
        {
            if (FieldOfView(P, j.gameObject, VisionAngle, ViewDistance, Radius))
            {
                return true;
            }
        }
        return false;
    }
    public List<Entity> RefreshEnemy(Entity.KindOfEntity k)
    {
        if (k == Entity.KindOfEntity.Enemy)
        {
            return _allies;
        }
        else
        {
            return _enemy;
        }
    }
    public GameObject GetCloseEnemy(List<Entity> targets, Transform position)
    {
        if (targets.Count <= 0)
        {
            return null;
        }
        float distance = Mathf.Infinity;
        Entity closeTg = null;
        foreach (Entity j in targets)
        {
            if (Vector3.Distance(j.transform.position, position.position) < distance)
            {
                closeTg = j;
                distance = Vector3.Distance(j.transform.position, position.position);
            }
        }
        if (closeTg == null)
        {
            return null;
        }
        return closeTg.gameObject;
    }
    public bool HavePower(PjPower p)
    {
        return _powers.Contains(p);
    }
    public List<Transform> GetSeparationEntityes()
    {
        List < Transform > p= new List <Transform>();
        p=_allies.Concat(_enemy).Select(j => j.transform).ToList();
        return p;
    }
    public void AddEntity(Entity p, Entity.KindOfEntity k)
    {
        if(k==Entity.KindOfEntity.Allies)
        {
            if (!_allies.Contains(p))
            {
                _allies.Add(p);
               //print(_allies.Count + " aliado aniadido");
            }
        }
        else
        {
            if (!_enemy.Contains(p))
            {
                _enemy.Add(p);
                //print(_enemy.Count + " enemigo aniadido");
            }
        }
    }
    public void RemoveEntity(Entity p, Entity.KindOfEntity k)
    {
        if (k == Entity.KindOfEntity.Allies)
        {
            if (_allies.Contains(p))
            {
                _allies.Remove(p);
                //print(_allies.Count + " aliado removido");
            }
        }
        else
        {
            if (_enemy.Contains(p))
            {
                _enemy.Remove(p);
               //print(_enemy.Count + " enemigo removido");
            }
        }
    }
    public void LaunchProjectile(GameObject p,Vector3 Target)
    {
        Rigidbody _rb=p.GetComponent<Rigidbody>();
        if (Target == null|| _rb ==null )
        {
            return;
        }
        Vector3 toTarget = Target - p.transform.position;

        Vector3 toTargetXZ = new Vector3(toTarget.x, 0, toTarget.z);
        float distanceXZ = toTargetXZ.magnitude;
        float heightDifference = toTarget.y;

        float angleRad = LaunchAngle * Mathf.Deg2Rad;
        float gravity = Mathf.Abs(Physics.gravity.y);

        float cosAngle = Mathf.Cos(angleRad);
        float sinAngle = Mathf.Sin(angleRad);

        float numerator = gravity * distanceXZ * distanceXZ;
        float denominator = 2 * cosAngle * cosAngle * (distanceXZ * Mathf.Tan(angleRad) - heightDifference);

        if (denominator <= 0)
        {
            Debug.LogWarning("No hay solución real: angulo muy bajo o objetivo demasiado alto");
            return;
        }

        float velocity = Mathf.Sqrt(numerator / denominator);

        Vector3 velocityXZ = toTargetXZ.normalized * velocity * cosAngle;
        float velocityY = velocity * sinAngle;

        Vector3 finalVelocity = velocityXZ + Vector3.up * velocityY;

        _rb.velocity = finalVelocity;
    }
}
