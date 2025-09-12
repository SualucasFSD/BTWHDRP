using UnityEngine;

public class RestartButon : MonoBehaviour
{
    [SerializeField] private string _sceneName;
    public void RestartScene()
    {
        EventManager.Ejecute(EventManager.KindOfEvent.ResetLevel, _sceneName);
    }
}
