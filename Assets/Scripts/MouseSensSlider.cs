using UnityEngine;
using UnityEngine.UI;

public class MouseSensSlider : MonoBehaviour
{
    [SerializeField] private Slider SliderSens;
    private void Start()
    {
       if (SliderSens != null) { SliderSens = GetComponent<Slider>(); }
    }
    public void MouseSens()
    {
        EventManager.Ejecute(EventManager.KindOfEvent.ChangeSensibilitieMouse,SliderSens.value);
    }
}
