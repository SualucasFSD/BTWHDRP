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
    public enum KindOfEntity
    {
        Allies,
        Enemy
    }
    public KindOfEntity Kind;
    public List<Entity> Targets=new List<Entity>();

    public List<PathNode> TakePath(Transform pos,LayerMask nodesLayer)
    {
        List<PathNode> Pathnodes=new List<PathNode>();
        Collider[] nodes = Physics.OverlapSphere(pos.position, 25f, nodesLayer);
        if (nodes.Length > 0)
        {
            Pathnodes = PathFinding.Instance.AStar(GameManager.Instance.GetCloseNode(pos), nodes[Random.Range(0, nodes.Length)].GetComponent<PathNode>());

            if (Pathnodes.Count <= 0) { return Pathnodes; }

            float dist = 0;
            PathNode i = default;
            foreach (PathNode node in Pathnodes)
            {
                if (dist == 0)
                {
                    dist += Vector3.Distance(pos.position, node.transform.position);
                }
                else
                {
                    dist += Vector3.Distance(i.transform.position, node.transform.position);
                }
                i = node;
            }
            if (dist > 50)
            {
                return TakePath(pos,nodesLayer);
            }
        }
        return Pathnodes;
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

    public void IsGroundedDetector(float Height=0)
    {
        IsGrounded = Physics.Raycast(origin: transform.position + Vector3.up * Height,direction:-Vector3.up, layerMask: GroundLayer, maxDistance: 1, hitInfo: out _groundDetect);
        //return IsGrounded;
    }
}
