//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Pause : MonoBehaviour
//{
//    public static Pause instance;
//    //private List<MonoBehaviour> behavioursToReactivate = new List<MonoBehaviour>();
//    private Dictionary<Rigidbody, Vector3> savedVelocities = new Dictionary<Rigidbody, Vector3>();
//    private Dictionary<Rigidbody, Vector3> savedAngularVelocities = new Dictionary<Rigidbody, Vector3>();
//    private Dictionary<Rigidbody, bool> wasKinematic = new();
//    private List<Animator> _animExclude = new List<Animator>();

//    private Dictionary<GameObject, Coroutine> pausedObjects = new();
//    private Dictionary<GameObject, float> remainingTimes = new();

//    private void Awake()
//    {
//        instance = this;
//    }
//    private void Start()
//    {
//        EventManager.Suscribe(EventManager.KindOfEvent.PauseGame, PauseApp);
//        EventManager.Suscribe(EventManager.KindOfEvent.ResumeTime, ResumeOn);
//        EventManager.Suscribe(EventManager.KindOfEvent.PauseTime, PauseOn);
//        EventManager.Suscribe(EventManager.KindOfEvent.PauseOneEnemy, PauseObjectForTime);
//        EventManager.Suscribe(EventManager.KindOfEvent.ResumeOneEnemy, ResetIndividualPause);
//    }

//    public void PauseApp(params object[] obj)
//    {
//        if (!GameManager.Instance.IsPaused)
//        {
//            ResumeOn();
//        }
//        else
//        {
//            PauseOn();
//        }
//    }

//    private void PauseOn(params object[] p)
//    {
//        GameManager.Instance.IsPaused = true;
//        foreach (Animator anim in GetComponentsInChildren<Animator>())
//        {
//            if (anim.speed != 0)
//                anim.speed = 0;
//            else
//                _animExclude.Add(anim);
//        }

//        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>(includeInactive: true))
//        {
//            if (!rb.gameObject.activeInHierarchy) continue;

//            wasKinematic[rb] = rb.isKinematic;

//            if (!rb.isKinematic)
//            {
//                savedVelocities[rb] = rb.linearVelocity;
//                savedAngularVelocities[rb] = rb.angularVelocity;

//                rb.linearVelocity = Vector3.zero;
//                rb.angularVelocity = Vector3.zero;
//                rb.isKinematic = true;
//            }
//        }
//    }

//    private void ResumeOn(params object[] p)
//    {
//        GameManager.Instance.IsPaused = false;
//        foreach (Animator anim in GetComponentsInChildren<Animator>())
//        {
//            if (_animExclude.Contains(anim))
//                continue;
//            anim.speed = 1;
//        }
//        _animExclude.Clear();

//        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>(includeInactive: true))
//        {
//            if (!rb.gameObject.activeInHierarchy) continue;

//            if (wasKinematic.ContainsKey(rb) && !wasKinematic[rb])
//            {
//                rb.isKinematic = false;

//                if (savedVelocities.TryGetValue(rb, out var v))
//                    rb.linearVelocity = v;
//                if (savedAngularVelocities.TryGetValue(rb, out var av))
//                    rb.angularVelocity = av;
//            }
//        }

//        savedVelocities.Clear();
//        savedAngularVelocities.Clear();
//        wasKinematic.Clear();
//    }

//    private void OnDestroy()
//    {
//        EventManager.Unscribe(EventManager.KindOfEvent.PauseGame, PauseApp);
//        EventManager.Unscribe(EventManager.KindOfEvent.ResumeTime, ResumeOn);
//        EventManager.Unscribe(EventManager.KindOfEvent.PauseTime, PauseOn);
//        EventManager.Unscribe(EventManager.KindOfEvent.PauseOneEnemy, PauseObjectForTime);
//        EventManager.Unscribe(EventManager.KindOfEvent.ResumeOneEnemy, ResetIndividualPause);
//    }

//    private IEnumerator PauseIndividualCoroutine(GameObject target)
//    {
//        var animators = new List<Animator>();
//        foreach (var anim in target.GetComponentsInChildren<Animator>(true))
//        {
//            if (anim.speed != 0)
//            {
//                anim.speed = 0;
//                animators.Add(anim);
//            }
//        }

//        var rigidbodies = new List<Rigidbody>();
//        var velocities = new List<(Vector3, Vector3)>();

//        foreach (var rb in target.GetComponentsInChildren<Rigidbody>(true))
//        {
//            if (!rb.isKinematic)
//            {
//                rigidbodies.Add(rb);
//                velocities.Add((rb.linearVelocity, rb.angularVelocity));
//                rb.linearVelocity = Vector3.zero;
//                rb.angularVelocity = Vector3.zero;
//                rb.isKinematic = true;
//            }
//        }

//        while (remainingTimes.ContainsKey(target) && remainingTimes[target] > 0)
//        {
//            if (!GameManager.Instance.IsPaused)
//            {
//                remainingTimes[target] -= Time.deltaTime;
//            }
//            yield return null;
//        }

//        RestoreObjectState(target);

//        pausedObjects.Remove(target);
//        remainingTimes.Remove(target);
//    }

//    private void RestoreObjectState(GameObject target)
//    {
//        if (target == null) return;

//        foreach (var anim in target.GetComponentsInChildren<Animator>(true))
//        {
//            anim.speed = 1;
//        }

//        foreach (var rb in target.GetComponentsInChildren<Rigidbody>(true))
//        {
//            rb.isKinematic = false;
//        }
//    }

//    public void PauseObjectForTime(params object[] p)
//    {
//        GameObject r = (GameObject)p[0];
//        if (r == null || (float)p[1] <= 0f) return;

//        if (pausedObjects.ContainsKey(r))
//        {
//            remainingTimes[r] = (float)p[1];
//            return;
//        }

//        remainingTimes[r] = (float)p[1];
//        Entity j = r.GetComponent<Entity>();
//        if (j != null)
//        {
//            j.PauseForMoment((float)p[1]);
//        }
//        pausedObjects[r] = StartCoroutine(PauseIndividualCoroutine((GameObject)p[0]));
//    }

//    public void ResetIndividualPause(params object[] target)
//    {
//        if ((GameObject)target[0] == null) return;

//        if (pausedObjects.TryGetValue((GameObject)target[0], out var routine))
//        {
//            StopCoroutine(routine);
//            RestoreObjectState((GameObject)target[0]);
//            pausedObjects.Remove((GameObject)target[0]);
//            remainingTimes.Remove((GameObject)target[0]);
//        }
//    }
//}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    public static Pause instance;

    private List<Animator> pausedAnimators = new List<Animator>();

    private Dictionary<Rigidbody, RigidbodyState> pausedRBs = new Dictionary<Rigidbody, RigidbodyState>();

    private Dictionary<GameObject, Coroutine> pausedObjects = new Dictionary<GameObject, Coroutine>();
    private Dictionary<GameObject, float> remainingTimes = new Dictionary<GameObject, float>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.PauseGame, PauseApp);
        EventManager.Suscribe(EventManager.KindOfEvent.PauseTime, PauseAll);
        EventManager.Suscribe(EventManager.KindOfEvent.ResumeTime, ResumeAll);
        EventManager.Suscribe(EventManager.KindOfEvent.PauseOneEnemy, PauseObjectForTime);
        EventManager.Suscribe(EventManager.KindOfEvent.ResumeOneEnemy, ResetIndividualPause);
    }

    public void PauseApp(params object[] obj)
    {
        if (!GameManager.Instance.IsPaused)
        {
            ResumeAll();
        }
        else
        {
            PauseAll();
        }
    }

    #region Pausa global
    private void PauseAll(params object[] p)
    {
        GameManager.Instance.IsPaused = true;

        // Pausar animadores
        foreach (Animator anim in GetComponentsInChildren<Animator>())
        {
            if (anim.speed != 0)
            {
                anim.speed = 0;
                pausedAnimators.Add(anim);
            }
        }

        // Pausar rigidbodies
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>(true))
        {
            if (!rb.gameObject.activeInHierarchy || pausedRBs.ContainsKey(rb))
                continue;

            RigidbodyState state = new RigidbodyState
            {
                //velocity = rb.velocity,
                angularVelocity = rb.angularVelocity,
                isKinematic = rb.isKinematic,
                useGravity = rb.useGravity,
                constraints = rb.constraints,
                collisionMode = rb.collisionDetectionMode
            };

            pausedRBs[rb] = state;

            //rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    private void ResumeAll(params object[] p)
    {
        GameManager.Instance.IsPaused = false;

        foreach (Animator anim in pausedAnimators)
        {
            if (anim != null)
                anim.speed = 1;
        }
        pausedAnimators.Clear();

        foreach (var rb in new List<Rigidbody>(pausedRBs.Keys))
        {
            if (rb == null) continue;

            var s = pausedRBs[rb];
            rb.isKinematic = s.isKinematic;
            rb.useGravity = s.useGravity;
            rb.constraints = s.constraints;
            rb.collisionDetectionMode = s.collisionMode;
            //rb.velocity = s.velocity;
            rb.angularVelocity = s.angularVelocity;
        }
        pausedRBs.Clear();
    }
    #endregion

    #region Pausa individual
    public void PauseObjectForTime(params object[] p)
    {
        if (p.Length < 2) return;

        GameObject target = p[0] as GameObject;
        float time = (float)p[1];

        if (target == null || time <= 0f) return;

        if (pausedObjects.ContainsKey(target))
        {
            remainingTimes[target] = time;
            return;
        }

        remainingTimes[target] = time;
        pausedObjects[target] = StartCoroutine(PauseIndividualCoroutine(target, time));

        // Si tiene entidad, invocar método de pausa
        Entity e = target.GetComponent<Entity>();
        if (e != null)
        {
            e.PauseForMoment(time);
        }
    }

    private IEnumerator PauseIndividualCoroutine(GameObject target, float time)
    {
        List<Animator> animators = new List<Animator>();
        foreach (var anim in target.GetComponentsInChildren<Animator>(true))
        {
            if (anim.speed != 0)
            {
                animators.Add(anim);
                anim.speed = 0;
            }
        }

        List<Rigidbody> rbs = new List<Rigidbody>();
        List<RigidbodyState> states = new List<RigidbodyState>();

        foreach (var rb in target.GetComponentsInChildren<Rigidbody>(true))
        {
            if (!rb.isKinematic)
            {
                rbs.Add(rb);
                states.Add(new RigidbodyState
                {
                    //velocity = rb.angularVelocity,
                    angularVelocity = rb.angularVelocity,
                    isKinematic = rb.isKinematic,
                    useGravity = rb.useGravity,
                    constraints = rb.constraints,
                    collisionMode = rb.collisionDetectionMode
                });

                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }

        while (remainingTimes.ContainsKey(target) && remainingTimes[target] > 0f)
        {
            if (!GameManager.Instance.IsPaused)
                remainingTimes[target] -= Time.deltaTime;

            yield return null;
        }

        for (int i = 0; i < animators.Count; i++)
        {
            if (animators[i] != null)
                animators[i].speed = 1;
        }

        for (int i = 0; i < rbs.Count; i++)
        {
            if (rbs[i] == null) continue;
            var s = states[i];
            rbs[i].isKinematic = s.isKinematic;
            rbs[i].useGravity = s.useGravity;
            rbs[i].constraints = s.constraints;
            rbs[i].collisionDetectionMode = s.collisionMode;
            rbs[i].angularVelocity = s.angularVelocity;
        }

        pausedObjects.Remove(target);
        remainingTimes.Remove(target);
    }

    public void ResetIndividualPause(params object[] target)
    {
        if (target.Length < 1) return;

        GameObject t = target[0] as GameObject;
        if (t == null) return;

        if (pausedObjects.TryGetValue(t, out var routine))
        {
            StopCoroutine(routine);
            pausedObjects.Remove(t);
            remainingTimes.Remove(t);

            foreach (var anim in t.GetComponentsInChildren<Animator>(true))
                anim.speed = 1;

            foreach (var rb in t.GetComponentsInChildren<Rigidbody>(true))
                rb.isKinematic = false;
        }
    }
    #endregion

    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.PauseGame, PauseApp);
        EventManager.Unscribe(EventManager.KindOfEvent.PauseTime, PauseAll);
        EventManager.Unscribe(EventManager.KindOfEvent.ResumeTime, ResumeAll);
        EventManager.Unscribe(EventManager.KindOfEvent.PauseOneEnemy, PauseObjectForTime);
        EventManager.Unscribe(EventManager.KindOfEvent.ResumeOneEnemy, ResetIndividualPause);
    }

    #region Helper
    private struct RigidbodyState
    {
        public Vector3 velocity;
        public Vector3 angularVelocity;
        public bool isKinematic;
        public bool useGravity;
        public RigidbodyConstraints constraints;
        public CollisionDetectionMode collisionMode;
    }
    #endregion
}
