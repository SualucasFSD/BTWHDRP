using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MainMenuIntro : MonoBehaviour
{
    [SerializeField] CanvasGroup _myBlack;
    [SerializeField] float _fadeTime = 1f;

    [SerializeField] GameObject _B, _T, _W;

    [SerializeField] float _keyTime = .1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(IntroCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    IEnumerator IntroCoroutine()
    {
        _B.SetActive(true);
        yield return new WaitForSeconds(_keyTime);
        _T.SetActive(true);
        yield return new WaitForSeconds(_keyTime);
        _W.SetActive(true);
        yield return new WaitForSeconds(_keyTime);
        StartCoroutine(FadeOut());
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
