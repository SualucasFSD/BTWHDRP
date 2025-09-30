using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LifeUpdaterBar : MonoBehaviour
{
    [SerializeField] private Image _lifeBarSlider;
    [SerializeField] private Image _yellowBar;
    [SerializeField] private float _yellowSpeed = 1f;

    private Coroutine _yellowRoutine;

    private void Start()
    {
        if (_lifeBarSlider == null)
        {
            _lifeBarSlider = GetComponent<Image>();
        }

        if (_yellowBar == null)
        {
            Debug.LogWarning("Yellow bar no asignada en LifeUpdaterBar");
        }

        EventManager.Suscribe(EventManager.KindOfEvent.LifeUpdater, LifeUpdateImage);
    }

    private void LifeUpdateImage(params object[] p)
    {
        float target = (float)p[0];

        if (_lifeBarSlider != null)
        {
            _lifeBarSlider.fillAmount = target;
        }

        if (_yellowRoutine != null)
        {
            StopCoroutine(_yellowRoutine);
        }

        if (_yellowBar != null)
        {
            _yellowRoutine = StartCoroutine(YellowRoutine(target));
        }
    }

    private IEnumerator YellowRoutine(float target)
    {
        while (!Mathf.Approximately(_yellowBar.fillAmount, target))
        {
            _yellowBar.fillAmount = Mathf.MoveTowards(_yellowBar.fillAmount, target, _yellowSpeed * Time.deltaTime);
            yield return null;
        }

        _yellowRoutine = null;
    }

    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.LifeUpdater, LifeUpdateImage);
    }
}
