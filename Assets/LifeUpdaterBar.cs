using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeUpdaterBar : MonoBehaviour
{
    [SerializeField] private Image _lifeBarSlider;
    [SerializeField] private Image _yellowBar;
    private void Start()
    {
        if(_lifeBarSlider==null)
        {
            _lifeBarSlider = GetComponent<Image>();
        }
        EventManager.Suscribe(EventManager.KindOfEvent.LifeUpdater, LifeUpdateImage);
    }

    private void LifeUpdateImage(params object[] p)
    {
        StartCoroutine(YellowRouine((float)p[0]));
        if (_lifeBarSlider != null)
        {
            _lifeBarSlider.fillAmount = (float)p[0];
        }
        //if(_yellowBar.)
    }
    IEnumerator YellowRouine(float i)
    {
        if(_yellowBar.fillAmount<i)
        {
            while (_yellowBar.fillAmount < i)
            {
              _yellowBar.fillAmount += Time.deltaTime/25;
                yield return null;
            }
        }
        else
        {
            while (_yellowBar.fillAmount > i)
            {
                _yellowBar.fillAmount -= Time.deltaTime/25;
                yield return null;
            }
        }
    }
    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.LifeUpdater, LifeUpdateImage);
    }
}
