using UnityEngine;
using UnityEngine.SceneManagement;

public class OnPlayCanvas : MonoBehaviour
{
    [SerializeField] private GameObject PauseMenu;
    [SerializeField] private string _restartScene;
    [SerializeField] private GameObject _deadPanel;
    private void Start()
    {
        EventManager.Suscribe(EventManager.KindOfEvent.OnDeath, OnDeath);
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
                    //GameManager.Instance.IsPaused=false;
                    EventManager.Ejecute(EventManager.KindOfEvent.PauseGame);
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    //GameManager.Instance.IsPaused = true;
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
    public void RestartScene()
    {
        EventManager.ResetEvent();
        SceneManager.LoadScene(_restartScene);
    }
    public void QuitAplication()
    {
        EventManager.ResetEvent();
        SceneManager.LoadScene("MainMenu");
        //Application.Quit();
    }
}
