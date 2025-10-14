using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class DecalBehaviour : MonoBehaviour
{
    float _decalDuration;
    float _lerpDuration;
    [SerializeField] DecalProjector _myDecal;
    private float _elapsed=0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _myDecal = GetComponent<DecalProjector>();
    }
    void Start()
    {
        GameManager.Instance.DecalUpdate += MyDecalFunc;
    }
    public void MyDecalFunc()
    {
        if(_myDecal==null)
        {
            return;
        }
        Invoke(nameof(InvokeDecal), _decalDuration);
        /*var x = _myDecal.GetComponent<DecalProjector>();
        if()
        float elapsed = 0f;
        while (elapsed <= _lerpDuration)
        {
            x.fadeFactor = Mathf.Lerp(1, 0, elapsed / _lerpDuration);
            elapsed += 0.1f;
            yield return WaitForSeconds(0.1f);
        }
        Destroy(this.gameObject);*/
    }
    private void InvokeDecal()
    {
        _elapsed += Time.deltaTime;
        _myDecal.fadeFactor=Mathf.Lerp(1, 0, _elapsed / _lerpDuration);
    }
    private void OnDestroy()
    {
        GameManager.Instance.DecalUpdate -= MyDecalFunc;
    }
}
}
