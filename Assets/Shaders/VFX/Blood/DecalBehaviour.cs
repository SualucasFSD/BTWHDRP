using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class DecalBehaviour : MonoBehaviour
{
    [SerializeField] float _decalDuration;
    [SerializeField] float _lerpDuration;
    [SerializeField] DecalProjector _myDecal;
    private float _elapsed=0;
    [SerializeField] bool _fading = false;

    private void Awake()
    {
        _myDecal = GetComponent<DecalProjector>();
    }
    void Start()
    {
        Invoke(nameof(InvokeDecal), _decalDuration);
    }

    private void Update()
    {
        MyDecalFunc();
    }

    public void MyDecalFunc()
    {
        if(!_fading)
        {
            return;
        }

        _elapsed += Time.deltaTime;

        _myDecal.fadeFactor = Mathf.Lerp(1, 0, _elapsed / _lerpDuration);

        if(_elapsed >= _lerpDuration)
        {
            Destroy(gameObject);
        }
    }
    void InvokeDecal()
    {
        _fading = true;
    }
}

