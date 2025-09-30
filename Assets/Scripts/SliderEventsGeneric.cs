using UnityEngine;
using UnityEngine.UI;

public class SliderEventsGeneric : MonoBehaviour
{
    [SerializeField] private EventManager.KindOfEvent KindOfEvent;
    [SerializeField] private Slider SliderSens;
    private void Start()
    {
        if (SliderSens != null) { SliderSens = GetComponent<Slider>(); }
    }
    public void RefreshSlider()
    {
        EventManager.Ejecute(KindOfEvent, SliderSens.value);
    }
}
