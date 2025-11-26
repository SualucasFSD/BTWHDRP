using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuIntro : MonoBehaviour
{
    [SerializeField] CanvasGroup _myBlack;
    [SerializeField] float _fadeTime = 1f;

    [SerializeField] GameObject _B, _T, _W;

    [SerializeField] float _keyTime = .1f;

    float _iti = 0;
    private Coroutine _fade=null;
    /*void Start()
    {
        StartCoroutine(IntroCoroutine());
    }*/

    private void Update()
    {
        _iti += Time.deltaTime;
        if(_iti > 1&&!_B.activeInHierarchy)
        {
            _B.SetActive(true);
        }
        if(_iti > 2 && !_T.activeInHierarchy)
        {
            _T.SetActive(true);
        }
        if(_iti > 3 && !_W.activeInHierarchy)
        {
            _W.SetActive(true);
        }
        if(_iti>4&&_fade==null)
        {
            _fade= StartCoroutine(FadeOut());
        }
    }
    /*IEnumerator IntroCoroutine()
    {
        _B.SetActive(true);
        yield return new WaitForSeconds(_keyTime);
        _T.SetActive(true);
        yield return new WaitForSeconds(_keyTime);
        _W.SetActive(true);
        yield return new WaitForSeconds(_keyTime);
        StartCoroutine(FadeOut());
    }*/

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
