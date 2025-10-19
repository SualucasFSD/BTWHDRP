using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
public class Entity : MonoBehaviour
{
    [Header("Variables Heredadas")]
    public Vector3 Dir;
    public float Life;
    public float _visionTimer = 0f;
    public float _visionThreshold = 1f;
    public bool IsGrounded;
    public LayerMask GroundLayer;
    protected RaycastHit _groundDetect;
    public float GroundDistanceDetector;
    public Transform Tg;
    public MazeCell Cell;
    public enum KindOfEntity
    {
        Allies,
        Enemy
    }
    public KindOfEntity Kind;
    public List<Entity> Targets=new List<Entity>();
    public List<PathNode> TakePath(Transform pos, LayerMask nodesLayer, int maxTries = 5)
    {

        List<PathNode> pathNodes = new List<PathNode>();
        Collider[] nodes = Physics.OverlapSphere(pos.position, 25f, nodesLayer);

        if (nodes.Length == 0)
        {
            return pathNodes;
        }
        PathNode start = GameManager.Instance.GetCloseNode(pos);

        for (int i = 0; i < maxTries; i++)
        {
            PathNode target = nodes[Random.Range(0, nodes.Length)].GetComponent<PathNode>();
            pathNodes = PathFinding.Instance.AStar(start, target);

            if (pathNodes.Count == 0)
                continue;

            float dist = 0f;
            PathNode prev = null;
            foreach (PathNode node in pathNodes)
            {
                if (prev == null)
                    dist += Vector3.Distance(pos.position, node.transform.position);
                else
                    dist += Vector3.Distance(prev.transform.position, node.transform.position);

                prev = node;
            }

            if (dist <= 50f)
                return pathNodes;
        }

        return new List<PathNode>();
    }

    public void Detection(EnemyStats stats,Transform pos, Action CombatState)
    {

        Transform potentialTarget = null;
        List<Entity> enemyList = GameManager.Instance.RefreshEnemy(Kind);

        foreach (var enemy in enemyList)
        {
            if (enemy == null) continue;

            Vector3 dir = enemy.transform.position - pos.position;
            float dist = dir.magnitude;

            if (dist > 15f) continue;

            if (dist <= stats.VisionDistance * 0.5f)
            {
                potentialTarget = enemy.transform;
                break;
            }
        }

        if (potentialTarget == null &&
            GameManager.Instance.OnView(pos.gameObject, stats.VisionDistance, stats.VisionCone, GameManager.Instance.EnemyConfiguration[EnemyCatalogue.Mague].Radius, enemyList))
        {
            foreach (var enemy in enemyList)
            {
                if (enemy == null) continue;

                Vector3 dir = enemy.transform.position - pos.position;
                float dist = dir.magnitude;
                if (dist > 15f) continue;

                float dot = Vector3.Dot(pos.forward, dir.normalized);
                if (dot > stats.VisionCone && GameManager.Instance.SphereLineOfSight(pos.position, enemy.transform.position, stats.Radius))
                {
                    potentialTarget = enemy.transform;
                    break;
                }
            }
        }

        if (potentialTarget != null)
        {
            float dist = Vector3.Distance(pos.position, potentialTarget.position);

            if (dist <= 5f)
            {
                CombatState();
                return;
            }

            _visionThreshold = Mathf.Lerp(0.5f, 5f, dist / stats.VisionDistance);
            _visionTimer += Time.deltaTime;

            if (_visionTimer >= _visionThreshold)
            {
                CombatState();
                return;
            }
        }
        else
        {
            _visionTimer = Mathf.Max(0f, _visionTimer - Time.deltaTime * 2);
        }
    }

    public void IsGroundedDetector()
    {
       Physics.Raycast(origin: transform.position,direction:-Vector3.up, layerMask: GroundLayer, maxDistance: 10, hitInfo: out _groundDetect);
        if(_groundDetect.collider!=null)
        {
            if(Vector3.Distance(transform.position,_groundDetect.point)<GroundDistanceDetector)
            {
                IsGrounded=true;
            }
            else
            {
                IsGrounded = false;
            }
        }
        else
        {
            IsGrounded = false;
        }
    }
    public virtual void EnableAgain()
    {

    }
    public virtual void FlyFunct(float height = 4f)
    {

    }
    public virtual void GetToTheGround()
    {

    }
}
