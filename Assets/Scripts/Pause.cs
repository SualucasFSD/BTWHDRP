/*using System.Collections.Generic;
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
        EventManager.Suscribe(EventManager.KindOfEvent.ResumeTime, ResumeOn);
        EventManager.Suscribe(EventManager.KindOfEvent.PauseTime, PauseOn);
    }
    public void PauseApp(params object[] obj)
    {
        if(!GameManager.Instance.IsPaused)
        {
            ResumeOn();
        }
        else { PauseOn(); }
    }
    private void PauseOn(params object[] p)
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
    private void ResumeOn(params object[] p)
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
        EventManager.Unscribe(EventManager.KindOfEvent.ResumeTime, ResumeOn);
        EventManager.Unscribe(EventManager.KindOfEvent.PauseTime, PauseOn);
    }
}*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Pause : MonoBehaviour
{
    public static Pause instance;
    //private List<MonoBehaviour> behavioursToReactivate = new List<MonoBehaviour>();
    private Dictionary<Rigidbody, Vector3> savedVelocities = new Dictionary<Rigidbody, Vector3>();
    private Dictionary<Rigidbody, Vector3> savedAngularVelocities = new Dictionary<Rigidbody, Vector3>();
    private Dictionary<Rigidbody, bool> wasKinematic = new();
    private List<Animator> _animExclude = new List<Animator>();

    private Dictionary<GameObject, Coroutine> pausedObjects = new();
    private Dictionary<GameObject, float> remainingTimes = new();

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.PauseGame, PauseApp);
        EventManager.Suscribe(EventManager.KindOfEvent.ResumeTime, ResumeOn);
        EventManager.Suscribe(EventManager.KindOfEvent.PauseTime, PauseOn);
        EventManager.Suscribe(EventManager.KindOfEvent.PauseOneEnemy,PauseObjectForTime);
        EventManager.Suscribe(EventManager.KindOfEvent.ResumeOneEnemy, ResetIndividualPause);
    }

    public void PauseApp(params object[] obj)
    {
        if (!GameManager.Instance.IsPaused)
        {
            ResumeOn();
        }
        else
        {
            PauseOn();
        }
    }

    private void PauseOn(params object[] p)
    {
        GameManager.Instance.IsPaused = true;
        foreach (Animator anim in GetComponentsInChildren<Animator>())
        {
            if (anim.speed != 0)
                anim.speed = 0;
            else
                _animExclude.Add(anim);
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
    }

    private void ResumeOn(params object[] p)
    {
        GameManager.Instance.IsPaused = false;
        foreach (Animator anim in GetComponentsInChildren<Animator>())
        {
            if (_animExclude.Contains(anim))
                continue;
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
    }

    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.PauseGame, PauseApp);
        EventManager.Unscribe(EventManager.KindOfEvent.ResumeTime, ResumeOn);
        EventManager.Unscribe(EventManager.KindOfEvent.PauseTime, PauseOn);
        EventManager.Unscribe(EventManager.KindOfEvent.PauseOneEnemy, PauseObjectForTime);
        EventManager.Unscribe(EventManager.KindOfEvent.ResumeOneEnemy, ResetIndividualPause);
    }

    private IEnumerator PauseIndividualCoroutine(GameObject target)
    {
        var animators = new List<Animator>();
        foreach (var anim in target.GetComponentsInChildren<Animator>(true))
        {
            if (anim.speed != 0)
            {
                anim.speed = 0;
                animators.Add(anim);
            }
        }

        var rigidbodies = new List<Rigidbody>();
        var velocities = new List<(Vector3, Vector3)>();

        foreach (var rb in target.GetComponentsInChildren<Rigidbody>(true))
        {
            if (!rb.isKinematic)
            {
                rigidbodies.Add(rb);
                velocities.Add((rb.linearVelocity, rb.angularVelocity));
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }

        while (remainingTimes.ContainsKey(target) && remainingTimes[target] > 0)
        {
            if (!GameManager.Instance.IsPaused)
            {
                remainingTimes[target] -= Time.deltaTime;
            }
            yield return null;
        }

        RestoreObjectState(target);

        pausedObjects.Remove(target);
        remainingTimes.Remove(target);
    }

   private void RestoreObjectState(GameObject target)
    {
        if (target == null) return;

        foreach (var anim in target.GetComponentsInChildren<Animator>(true))
        {
            anim.speed = 1;
        }

        foreach (var rb in target.GetComponentsInChildren<Rigidbody>(true))
        {
            rb.isKinematic = false;
        }
    }

    public void PauseObjectForTime(params object[]p)
    {
        GameObject r = (GameObject)p[0];
        if (r == null ||(float)p[1] <= 0f) return;

        if (pausedObjects.ContainsKey(r))
        {
            remainingTimes[r] = (float)p[1];
            return;
        }
        
        remainingTimes[r] = (float)p[1];
        Entity j = r.GetComponent<Entity>();
        if(j!=null)
        {
            j.PauseForMoment((float)p[1]);
        }
        pausedObjects[r] = StartCoroutine(PauseIndividualCoroutine((GameObject)p[0]));
    }

    public void ResetIndividualPause(params object[] target)
    {
        if ((GameObject)target[0] == null) return;

        if (pausedObjects.TryGetValue((GameObject)target[0], out var routine))
        {
            StopCoroutine(routine);
            RestoreObjectState((GameObject)target[0]);
            pausedObjects.Remove((GameObject)target[0]);
            remainingTimes.Remove((GameObject)target[0]);
        }
    }
}
