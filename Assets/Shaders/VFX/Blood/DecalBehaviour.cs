using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class DecalBehaviour : MonoBehaviour
{
    [Header("Duraciones")]
    [SerializeField] private float _decalDuration = 2f;
    [SerializeField] private float _lerpDuration = 1f;

    [Header("Referencias")]
    [SerializeField] private DecalProjector _myDecal; 

    private float _elapsed = 0f;
    private bool _fading = false;

    private void Awake()
    {
        if (_myDecal == null)
        {
            _myDecal = GetComponent<DecalProjector>();
        }
    }

    private void OnEnable()
    {
        _elapsed = 0f;
        _fading = false;
        _myDecal.fadeFactor = 1f;

        CancelInvoke();
        Invoke(nameof(InvokeDecal), _decalDuration);
    }

    private void Update()
    {
        MyDecalFunc();
    }

    private void MyDecalFunc()
    {
        if (!_fading)
            return;

        _elapsed += Time.deltaTime;
        _myDecal.fadeFactor = Mathf.Lerp(1f, 0f, _elapsed / _lerpDuration);

        if (_elapsed >= _lerpDuration)
        {
            _myDecal.fadeFactor = 1f;
            _fading = false;
            _elapsed = 0f;

            CleanDecal();
            DecalsFactory.Instance.ReturnObj(GenericObjectType.BloodDecal, gameObject);
        }
    }

    private void InvokeDecal()
    {
        _fading = true;
    }

    /// <summary>
    /// Limpia cualquier efecto o partícula hijo antes de devolver el decal al pool.
    /// </summary>
    private void CleanDecal()
    {
        foreach (Transform child in transform)
        {
            ParticleSystem ps = child.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            child.gameObject.SetActive(false);
        }
    }
}

