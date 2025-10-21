using UnityEngine;

public class PowerPanel : MonoBehaviour
{
    [SerializeField] private Cards[] _selectables;

    private void Start()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.PowerSelect, Activate);
        EventManager.Suscribe(EventManager.KindOfEvent.ResumeTime, Desactivate);
        gameObject.SetActive(false);
    }

    private void Activate(params object[] p)
    {
        gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        foreach (Cards card in _selectables)
        {
            card.Randomized();
        }
    }

    private void Desactivate(params object[] p)
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.PowerSelect, Activate);
        EventManager.Unscribe(EventManager.KindOfEvent.ResumeTime, Desactivate);
    }
}
