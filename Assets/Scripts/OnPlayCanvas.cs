using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OnPlayCanvas : MonoBehaviour
{
    [SerializeField] private GameObject PauseMenu;
    [SerializeField] private GameObject _deadPanel;
    [SerializeField] private Stack<GameObject> _panels= new Stack<GameObject>();
    private void Start()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.OnDeath, OnDeath);
        EventManager.Suscribe(EventManager.KindOfEvent.ResetLevel, RestartScene);
        EventManager.Suscribe(EventManager.KindOfEvent.MainMenu, MainMenuScene);
        //EventManager.Suscribe(EventManager.KindOfEvent.PauseGame, PauseApp);
    }
    private void Update()
    {
       PauseInput();   
    }
    private void PauseInput()
    {
        if (Input.GetButtonDown("Pause"))
        {
            PauseApp();
            EventManager.Ejecute(EventManager.KindOfEvent.PauseGame);
        }
    }
    private void PauseApp()
    {
        if (GameManager.Instance != null)
        {
            if(_panels.Count>0)
            {
                Close();
                return;
            }
            GameManager.Instance.IsPaused = !GameManager.Instance.IsPaused;
            PauseMenu.SetActive(!PauseMenu.activeInHierarchy);
            if (!PauseMenu.activeInHierarchy)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                PauseMenu.SetActive(false);
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
    public void Open(GameObject Panel)
    {
        if(Panel==null)
        { Debug.LogWarning("Falto Asignar Un Panel"); return; }
        if(!_panels.Contains(Panel))
        {
            _panels.Push(Panel);
            Panel.SetActive(true);
        }
    }
    public void Close()
    {
        if(_panels.Count>0)
        {
           _panels.Pop().SetActive(false);
        }
    }
    public void OnDeath(params object[] p)
    {
        enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PauseMenu.SetActive(false);
        _deadPanel.SetActive(true);
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
       // EventManager.Unscribe(EventManager.KindOfEvent.PauseGame, PauseApp);
    }
}
