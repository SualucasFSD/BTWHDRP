using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeUpdaterBar : MonoBehaviour
{
    [SerializeField] private Image _lifeBarSlider;

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
        if (_lifeBarSlider != null)
        {
            _lifeBarSlider.fillAmount = (float)p[0];
        }
    }
    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.LifeUpdater, LifeUpdateImage);
    }
}
