using UnityEngine;

public class MainMenuButton : MonoBehaviour
{
    [SerializeField] private string _sceneName;
    public void MainMenuScene()
    {
        EventManager.Ejecute(EventManager.KindOfEvent.MainMenu, _sceneName);
    }
}
