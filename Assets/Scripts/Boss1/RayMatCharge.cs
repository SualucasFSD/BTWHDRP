using UnityEngine;
using System.Collections;
public class RayMatCharge : MonoBehaviour
{
    [SerializeField] private string _matValueName;
    private Material _mat;

    public void Active()
    {
        if (TryGetComponent<Renderer>(out var Compo))
        {
            _mat = Compo.material;
            StartCoroutine(Initialize());
        }

    }
    IEnumerator Initialize()
    {
        if (_mat != null)
        {
            float t = 0f;
            float start = _mat.GetFloat(_matValueName);

            while (t < 1f)
            {
                t += Time.deltaTime;
                float val = Mathf.Lerp(start, 1f, t);
                _mat.SetFloat(_matValueName, val);
                yield return null;
            }
        }
    }
    public void Reinicio()
    {
        if (_mat != null)
        {
            _mat.SetFloat(_matValueName, 0);
        }
    }
}
