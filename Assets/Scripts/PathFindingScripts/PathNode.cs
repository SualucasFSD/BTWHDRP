using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    [SerializeField] private List<PathNode> _neighbords = new List<PathNode>();
    [SerializeField] private MazeCell _mazeCellParent;
    public List<PathNode> Neighbords {  get { return _neighbords; } }
    [SerializeField] private bool _isTesting=false;

    // Para la búsqueda
    [HideInInspector] public float gCost;
    [HideInInspector] public float hCost;
    public float fCost => gCost + hCost;
    [HideInInspector] public PathNode cameFrom;
    private void Awake()
    {
        _neighbords.Clear();
    }
    private void Start()
    {
        RaycastHit point;
        if (Physics.Raycast(transform.position, -Vector3.up, out point))
        {
            transform.position = point.point + Vector3.up * 1.5f;
        }
        if(_mazeCellParent != null && !_isTesting)
        {
            _mazeCellParent._pathNodesList.Add(this);
            return;
        }
        GameManager.Instance.PreLoadPathNodes.Add(this);
        EventManager.Suscribe(EventManager.KindOfEvent.ReloadNodes, ReloadEvent);
        GameManager.Instance.Timer = 0;
    }
    private void ReloadEvent(params object[] obj)
    {
        StartCoroutine(ReloadRoutine());
    }
    public IEnumerator ReloadRoutine()
    {
        foreach (PathNode N in GameManager.Instance.PreLoadPathNodes)
        {
            if (N == this)
            {
                continue;
            }
            if (GameManager.Instance.SphereLineOfSight(transform.position, N.transform.position, 0.6f) && Vector3.Distance(transform.position, N.transform.position) < 20)
            {
                _neighbords.Add(N);
            }
        }
        yield return null;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach( PathNode node in _neighbords ) { Gizmos.DrawLine(node.transform.position,transform.position); }
    }
    private void OnDestroy()
    {
        if (GameManager.Instance.PathNodes.Contains(this))
        {
            GameManager.Instance.PathNodes.Remove(this);
        }
        if (GameManager.Instance.PreLoadPathNodes.Contains(this))
        {
            GameManager.Instance.PreLoadPathNodes.Remove(this);
        }
        EventManager.Unscribe(EventManager.KindOfEvent.ReloadNodes, ReloadEvent);
    }
}
