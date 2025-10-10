using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    private List<MonoBehaviour> behavioursToReactivate = new List<MonoBehaviour>();
    private Dictionary<Rigidbody, Vector3> savedVelocities = new Dictionary<Rigidbody, Vector3>();
    private Dictionary<Rigidbody, Vector3> savedAngularVelocities = new Dictionary<Rigidbody, Vector3>();
    private Dictionary<Rigidbody, bool> wasKinematic = new();
    private List<Animator> _animExclude=new List<Animator>();
    private void Start()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.PauseGame,PauseApp);
    }
    public void PauseApp(params object[] obj)
    {
        if(!GameManager.Instance.IsPaused)
        {
            ResumeOn();
        }
        else { PauseOn(); }
    }
    private void PauseOn()
    {
        foreach (Animator anim in GetComponentsInChildren<Animator>())
        {
            if (anim.speed != 0)
            {
                anim.speed = 0;
            }
            else
            {
                _animExclude.Add(anim);
            }
        }

        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>(includeInactive: true))
        {
            if (!rb.gameObject.activeInHierarchy) continue;

            wasKinematic[rb] = rb.isKinematic;

            if (!rb.isKinematic)
            {
                savedVelocities[rb] = rb.linearVelocity;
                savedAngularVelocities[rb] = rb.angularVelocity;

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }

        foreach (MonoBehaviour script in GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
        {
            if (script != null && script.enabled && script != this && script.gameObject.activeInHierarchy)
            {
                script.enabled = false;
                behavioursToReactivate.Add(script);
            }
        }
    }
    private void ResumeOn()
    {
        foreach (Animator anim in GetComponentsInChildren<Animator>())
        {
            if(_animExclude.Contains(anim))
            {
               continue;
            }
            anim.speed = 1;
        }
        _animExclude.Clear();
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>(includeInactive: true))
        {
            if (!rb.gameObject.activeInHierarchy) continue;

            if (wasKinematic.ContainsKey(rb) && !wasKinematic[rb])
            {
                rb.isKinematic = false;

                if (savedVelocities.TryGetValue(rb, out var v))
                    rb.linearVelocity = v;
                if (savedAngularVelocities.TryGetValue(rb, out var av))
                    rb.angularVelocity = av;
            }
        }

        savedVelocities.Clear();
        savedAngularVelocities.Clear();
        wasKinematic.Clear();

        foreach (MonoBehaviour script in behavioursToReactivate)
        {
            if (script != null)
                script.enabled = true;
        }

        behavioursToReactivate.Clear();
    }
    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.PauseGame, PauseApp);
    }
}
