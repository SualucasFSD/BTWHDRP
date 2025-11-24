using System;
using System.Collections;
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
    public bool UseGravity = true;
    public float GravValue = 0;
    public LayerMask GroundLayer;
    protected RaycastHit _groundDetect;
    public float GroundDistanceDetector;
    public Transform Tg;
    public MazeCell Cell;
    public bool Ready = true;
    public bool Stuned = false;
    private GameObject _venomEffect;
    private GameObject _thunderEffect;
    private GameObject _thunderImpact;
    protected Coroutine _stopRoutine;
    public bool IsRayStunable=true;
    //private float _pauseTime=0;
    public enum KindOfEntity
    {
        Allies,
        Enemy
    }
    public KindOfEntity Kind;
    public List<Entity> Targets = new List<Entity>();
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

    public void Detection(EnemyStats stats, Transform pos, Action CombatState)
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
        Physics.Raycast(origin: transform.position, direction: -Vector3.up, layerMask: GroundLayer, maxDistance: 10, hitInfo: out _groundDetect);
        if (_groundDetect.collider != null)
        {
            if (Vector3.Distance(transform.position, _groundDetect.point) < GroundDistanceDetector)
            {
                IsGrounded = true;
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
    public virtual void FlyFunct()
    {

    }
    public virtual void GetToTheGround()
    {

    }
    public virtual void PauseForMoment(float time)
    {
        if (!IsGrounded || Life <= 0)
        {
            return;
        }
        _thunderImpact = GameObjectFactory.Instance.GetObj(GenericObjectType.LighningImpact, transform.position + Vector3.up * 2, transform.rotation);
        _thunderImpact.transform.parent = transform;
        if (_thunderImpact.TryGetComponent<ParticleSystem>(out var Component1))
        {
            Component1.Play();
        }
        _thunderEffect = GameObjectFactory.Instance.GetObj(GenericObjectType.ThunderEffect, transform.position, transform.rotation);
        _thunderEffect.transform.parent = transform;
        if (_thunderEffect.TryGetComponent<ParticleSystem>(out var Component))
        {
            Component.Play();
        }
        Ready = false;
        _stopRoutine = StartCoroutine(Stop(time));
    }
    IEnumerator Stop(float time)
    {
        float i = 0;
        while (i < time)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsPaused);
            i += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        EventManager.Ejecute(EventManager.KindOfEvent.ResumeOneEnemy, gameObject);
        if (_thunderEffect != null)
        {
            if (_thunderEffect.TryGetComponent<ParticleSystem>(out var Component))
            {
                Component.Stop();
            }
            GameObjectFactory.Instance.ReturnObj(GenericObjectType.ThunderEffect, _thunderEffect);
            _thunderEffect = null;
        }
        if (_thunderImpact != null)
        {
            if (_thunderImpact.TryGetComponent<ParticleSystem>(out var Component1))
            {
                Component1.Stop();
            }
            GameObjectFactory.Instance.ReturnObj(GenericObjectType.LighningImpact, _thunderImpact);
            _thunderImpact = null;
        }
        _stopRoutine = null;
        Ready = true;
    }
    public void FinishElectricPause()
    {
        if (_stopRoutine != null)
        {
            StopCoroutine(_stopRoutine);
        }
        EventManager.Ejecute(EventManager.KindOfEvent.ResumeOneEnemy, gameObject);
        if (_thunderEffect != null)
        {
            if (_thunderEffect.TryGetComponent<ParticleSystem>(out var Component))
            {
                Component.Stop();
            }
            GameObjectFactory.Instance.ReturnObj(GenericObjectType.ThunderEffect, _thunderEffect);
            _thunderEffect = null;
        }
        if (_thunderImpact != null)
        {
            if (_thunderImpact.TryGetComponent<ParticleSystem>(out var Component1))
            {
                Component1.Stop();
            }
            GameObjectFactory.Instance.ReturnObj(GenericObjectType.LighningImpact, _thunderImpact);
            _thunderImpact = null;
        }
        Ready = true;
    }
    public virtual void GetVenemous(float poisonDuration, float tickRate, float damagePerTick)
    {
        if (Life <= 0)
        {
            return;
        }
        if (GameObjectFactory.Instance != null)
        {
            _venomEffect = GameObjectFactory.Instance.GetObj(GenericObjectType.PoisonEffect, transform.position, transform.rotation);
            _venomEffect.transform.parent = transform;
            if (_venomEffect.TryGetComponent<ParticleSystem>(out var Component))
            {
                Component.Play();
            }
        }
        EventManager.Ejecute(EventManager.KindOfEvent.GetVenemous, this, poisonDuration, tickRate, damagePerTick);
    }
    public virtual void PopVenemous()
    {
        EventManager.Ejecute(EventManager.KindOfEvent.PopVenemous, this);
        if (_venomEffect != null)
        {
            if (_venomEffect.TryGetComponent<ParticleSystem>(out var Component))
            {
                Component.Stop();
            }
            GameObjectFactory.Instance.ReturnObj(GenericObjectType.PoisonEffect, _venomEffect);
            _venomEffect = null;
        }
    }
}
