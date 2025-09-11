using UnityEngine;
using UnityEngine.SceneManagement;

public class OnPlayCanvas : MonoBehaviour
{
    [SerializeField] private GameObject PauseMenu;
    [SerializeField] private GameObject _deadPanel;
    private void Start()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.OnDeath, OnDeath);
        EventManager.Suscribe(EventManager.KindOfEvent.ResetLevel, RestartScene);
        EventManager.Suscribe(EventManager.KindOfEvent.MainMenu, MainMenuScene);
    }
    private void Update()
    { 
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(GameManager.Instance!=null)
            {
                GameManager.Instance.IsPaused=!GameManager.Instance.IsPaused;
                PauseMenu.SetActive(!PauseMenu.activeInHierarchy);
                if(!PauseMenu.activeInHierarchy)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    EventManager.Ejecute(EventManager.KindOfEvent.PauseGame);
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    EventManager.Ejecute(EventManager.KindOfEvent.PauseGame);
                }
            }
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
    }
}
