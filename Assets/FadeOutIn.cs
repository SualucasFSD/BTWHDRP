using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutIn : MonoBehaviour
{
    public static FadeOutIn Instance;
    [SerializeField] private Material _FadeMat;
    private void Awake()
    {
        _FadeMat = GetComponent<Material>();
        if(Instance!=null)
        {
            FadeOut();
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.OnChangeScene, FadeIn);
        EventManager.Suscribe(EventManager.KindOfEvent.OnLoadScene, FadeOut);
    }
    private void FadeIn(params object[] p)
    {
        StartCoroutine(FadeRoutine(1));
    }
    private IEnumerator FadeRoutine(int i)
    {
        float j;
        if(i==1)
        {
            _FadeMat.SetFloat("_lerp", 0);
            j = 0;
            while (j<=1)
            {
                j += 0.1f;
                _FadeMat.SetFloat("_lerp", j);
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            j = 1;
            while (j >= 0)
            {
                j -= 0.1f;
                _FadeMat.SetFloat("_lerp", j);
                yield return new WaitForSeconds(0.1f);
            }

        }
    }
    private void FadeOut(params object[] p)
    {
        StartCoroutine(FadeRoutine(0));
    }
    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.OnChangeScene, FadeIn);
        EventManager.Unscribe(EventManager.KindOfEvent.OnLoadScene, FadeOut);
    }
}
