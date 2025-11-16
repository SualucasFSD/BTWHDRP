using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeinOut : MonoBehaviour
{
    [SerializeField] CanvasGroup _myBlack;
    [SerializeField] float _fadeTime = 1f;
    
    void Start()
    {
        StartCoroutine(FadeOut());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator FadeOut()
    {
        if (_myBlack == null)
            yield break;

        var elapsed = 0f; 

        while (elapsed <= _fadeTime)
        {
            var t = elapsed / _fadeTime;

            _myBlack.alpha -= t;

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator FadeIn()
    {

        if (_myBlack == null)
            yield break;

        var elapsed = 0f;

        while (elapsed <= _fadeTime)
        {
            var t = elapsed / _fadeTime;

            _myBlack.alpha += t;

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public void StartFadeIn()
    {
        StartCoroutine(FadeIn());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Fade")
            StartFadeIn();
    }
}
