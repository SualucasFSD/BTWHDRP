//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class OnPlayCanvas : MonoBehaviour
//{ 
//    [SerializeField] private GameObject PauseMenu;
//    [SerializeField] private GameObject _deadPanel;
//    [SerializeField] private Stack<GameObject> _panels= new Stack<GameObject>();
//    private void Start()
//    {
//        EventManager.Suscribe(EventManager.KindOfEvent.OnDeath, OnDeath);
//        EventManager.Suscribe(EventManager.KindOfEvent.ResetLevel, RestartScene);
//        EventManager.Suscribe(EventManager.KindOfEvent.MainMenu, MainMenuScene);
//    }
//    private void Update()
//    {
//       PauseInput();   
//    }
//    private void PauseInput()
//    {
//        if (Input.GetButtonDown("Pause"))
//        {
//            PauseApp();
//        }
//    }
//    private void PauseApp()
//    {
//        if (GameManager.Instance != null)
//        {
//            if (_panels.Count > 0)
//            {
//                Close();
//            }
//            else
//            {
//                GameManager.Instance.IsPaused = !GameManager.Instance.IsPaused;
//                PauseMenu.SetActive(GameManager.Instance.IsPaused);
//                EventManager.Ejecute(EventManager.KindOfEvent.PauseGame);
//                if (!GameManager.Instance.IsPaused)
//                {
//                    Cursor.lockState = CursorLockMode.Locked;
//                    Cursor.visible = false;
//                }
//                else
//                {
//                    Cursor.lockState = CursorLockMode.None;
//                    Cursor.visible = true;
//                }
//            }
//        }
//    }
//    public void Open(GameObject Panel)
//    {
//        if(Panel==null)
//        { Debug.LogWarning("Falto Asignar Un Panel"); return; }
//        if(!_panels.Contains(Panel))
//        {
//            _panels.Push(Panel);
//            Panel.SetActive(true);
//        }
//    }
//    public void Close()
//    {
//        if(_panels.Count>0)
//        {
//           _panels.Pop().SetActive(false);
//        }
//    }
//    public void OnDeath(params object[] p)
//    {
//        enabled = false;
//        Cursor.lockState = CursorLockMode.None;
//        Cursor.visible = true;
//        PauseMenu.SetActive(false);
//        _deadPanel.SetActive(true);
//    }
//    public void RestartScene(params object[] p)
//    {
//        EventManager.ResetEvent();
//        SceneManager.LoadScene((string)p[0]);
//    }
//    public void MainMenuScene(params object[] p)
//    {
//        EventManager.ResetEvent();
//        SceneManager.LoadScene((string)p[0]);
//    }
//    private void OnDestroy()
//    {
//        EventManager.Unscribe(EventManager.KindOfEvent.OnDeath, OnDeath);
//        EventManager.Unscribe(EventManager.KindOfEvent.ResetLevel, RestartScene);
//        EventManager.Unscribe(EventManager.KindOfEvent.MainMenu, MainMenuScene);
//    }
//}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OnPlayCanvas : MonoBehaviour
{
    [SerializeField] private GameObject PauseMenu;
    [SerializeField] private GameObject _deadPanel;

    private Stack<GameObject> _panels = new Stack<GameObject>();
    private float _buttonSelectedCooldown = 0f;
    private float _holdCancelTime = 0f;

    private void Start()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.OnDeath, OnDeath);
        EventManager.Suscribe(EventManager.KindOfEvent.ResetLevel, RestartScene);
        EventManager.Suscribe(EventManager.KindOfEvent.MainMenu, MainMenuScene);
    }

    private void Update()
    {
        PauseInput();
        CancelInput();
        HandleMenuNavigation();
    }
    private void PauseInput()
    {
        if (Input.GetButtonDown("Pause"))
        {
            TogglePauseMenu();
        }
    }

    private void TogglePauseMenu()
    {
        if (GameManager.Instance == null) return;

        if (_panels.Count > 0)
        {
            Close();
            return;
        }

        GameManager.Instance.IsPaused = !GameManager.Instance.IsPaused;
        EventManager.Ejecute(EventManager.KindOfEvent.PauseGame);

        if (GameManager.Instance.IsPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Open(PauseMenu);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _panels.Clear();
            PauseMenu.SetActive(false);
        }
    }

    private void CancelInput()
    {
        bool cancel = Input.GetButton("Cancel");

        if (cancel)
        {
            _holdCancelTime += Time.deltaTime;

            if (_holdCancelTime > 0.4f)
            {
                Close();
                _holdCancelTime = 0;
            }
        }
        else
        {
            _holdCancelTime = 0;
        }
    }

    public void Open(GameObject panel)
    {
        if (panel == null)
        {
            Debug.LogWarning("Panel no asignado");
            return;
        }

        if (!_panels.Contains(panel))
        {
            _panels.Push(panel);
            panel.SetActive(true);

            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void Close()
    {
        if (_panels.Count == 0) return;

        GameObject closing = _panels.Pop();
        closing.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);

        if (_panels.Count == 0)
        {
            GameManager.Instance.IsPaused = false;
            EventManager.Ejecute(EventManager.KindOfEvent.PauseGame);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void HandleMenuNavigation()
    {
        bool moved =
            Mathf.Abs(Input.GetAxis("Vertical")) > 0.2f ||
            Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f ||
            Input.GetAxis("Mouse ScrollWheel") != 0;

        if (!moved)
        {
            _buttonSelectedCooldown += Time.deltaTime;

            if (_buttonSelectedCooldown > 3f)
            {
                EventSystem.current.SetSelectedGameObject(null);
                _buttonSelectedCooldown = 0;
            }
        }
        else
        {
            _buttonSelectedCooldown = 0;
        }

        if (EventSystem.current.currentSelectedGameObject == null && moved)
        {
            if (_panels.Count > 0)
            {
                GameObject activePanel = _panels.Peek();
                Button first = activePanel.GetComponentInChildren<Button>();

                if (first != null)
                    SelectButton(first);
            }
        }
    }

    private void SelectButton(Button btn)
    {
        if (btn == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(btn.gameObject);
    }

    public void OnDeath(params object[] p)
    {
        enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PauseMenu.SetActive(false);
        _deadPanel.SetActive(true);

        _panels.Clear();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void RestartScene(params object[] p)
    {
        EventManager.ResetEvent();
        SceneManager.LoadScene((string)p[0]);
    }

    public void MainMenuScene(params object[] p)
    {
        EventManager.ResetEvent();
        SceneManager.LoadScene((string)p[0]);
    }

    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.OnDeath, OnDeath);
        EventManager.Unscribe(EventManager.KindOfEvent.ResetLevel, RestartScene);
        EventManager.Unscribe(EventManager.KindOfEvent.MainMenu, MainMenuScene);
    }
}
