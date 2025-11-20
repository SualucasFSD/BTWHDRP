using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerPanel : MonoBehaviour, Iinitializers
{
    [SerializeField] private Cards[] _selectables;

    private float _cooldown = 0f;
    private bool _panelActive = false;
    public void Initialize()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.PowerSelect, Activate);
        EventManager.Suscribe(EventManager.KindOfEvent.ResumeTime, Desactivate);
    }
    /*private void Start()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.PowerSelect, Activate);
        EventManager.Suscribe(EventManager.KindOfEvent.ResumeTime, Desactivate);
        gameObject.SetActive(false);
    }*/

    private void Update()
    {
        if (!_panelActive) return;

        HandleNavigation();
    }

    private void HandleNavigation()
    {
        bool moved =
            Mathf.Abs(Input.GetAxis("Vertical")) > 0.2f ||
            Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f ||
            Input.GetAxis("Mouse ScrollWheel") != 0;

        if (!moved)
        {
            _cooldown += Time.deltaTime;

            if (_cooldown > 3f)
            {
                EventSystem.current.SetSelectedGameObject(null);
                _cooldown = 3f;
            }
        }
        else
        {
            _cooldown = 0f;

            if (EventSystem.current.currentSelectedGameObject == null)
            {
                Button first = GetFirstButton();
                if (first != null)
                {
                    SelectButton(first);
                }
            }
        }
    }

    private Button GetFirstButton()
    {
        foreach (var card in _selectables)
        {
            Button btn = card.GetComponentInChildren<Button>();
            if (btn != null) return btn;
        }
        return null;
    }

    private void SelectButton(Button btn)
    {
        if (btn == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(btn.gameObject);
    }

    private void Activate(params object[] p)
    {
        _panelActive = true;
        gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        foreach (Cards card in _selectables)
            card.Randomized();

        Button first = GetFirstButton();
        if (first != null)
            SelectButton(first);
    }

    private void Desactivate(params object[] p)
    {
        _panelActive = false;

        gameObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.PowerSelect, Activate);
        EventManager.Unscribe(EventManager.KindOfEvent.ResumeTime, Desactivate);
    }

}
