using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    private List<MonoBehaviour> behavioursToReactivate = new List<MonoBehaviour>();
    private Dictionary<Rigidbody, Vector3> savedVelocities = new Dictionary<Rigidbody, Vector3>();
    private Dictionary<Rigidbody, Vector3> savedAngularVelocities = new Dictionary<Rigidbody, Vector3>();
    private Dictionary<Rigidbody, bool> wasKinematic = new();
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
            anim.speed = 0;
        }

        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>(includeInactive: true))
        {
            if (!rb.gameObject.activeInHierarchy) continue;

            wasKinematic[rb] = rb.isKinematic;

            if (!rb.isKinematic)
            {
                savedVelocities[rb] = rb.velocity;
                savedAngularVelocities[rb] = rb.angularVelocity;

                rb.velocity = Vector3.zero;
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
            anim.speed = 1;
        }

        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>(includeInactive: true))
        {
            if (!rb.gameObject.activeInHierarchy) continue;

            if (wasKinematic.ContainsKey(rb) && !wasKinematic[rb])
            {
                rb.isKinematic = false;

                if (savedVelocities.TryGetValue(rb, out var v))
                    rb.velocity = v;
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
