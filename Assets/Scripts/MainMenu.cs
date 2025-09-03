using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string _sceneName;
    [SerializeField] private GameObject _loadImg;
    private Dictionary<ActionEjecute, Action> _dict=new Dictionary<ActionEjecute, Action>();
    [SerializeField] private Button[] _buttons;
    public enum ActionEjecute
    {
       Play,
       Exit,
       Credits,
       Options
    }
    private void Start()
    {
        _dict.Add(ActionEjecute.Play,PlayButton);
        _dict.Add(ActionEjecute.Credits, CreditsButton);
        _dict.Add(ActionEjecute.Options, OptionsButton);
        _dict.Add(ActionEjecute.Exit, ExitButton);
    }
  public void Ejecute(int Index)
  {
        if (Index == 0) { _loadImg.SetActive(true); }
        StartCoroutine(PauseButtons());
        StartCoroutine(Wait(_dict[(ActionEjecute)Index]));
  }
  private void PlayButton()
  {
        SceneManager.LoadSceneAsync(_sceneName);
  }
  private void CreditsButton()
  {

  }
  private void OptionsButton()
  {

  }
  private void ExitButton()
  {
        print("Exit");
        Application.Quit();
  }
    private IEnumerator Wait(Action FunctToDelay)
    {
        yield return new WaitForSeconds(1.5f);
        FunctToDelay();
    }
    private IEnumerator PauseButtons()
    {
        foreach (var button in _buttons) { button.enabled = false; }
        yield return new WaitForSeconds(1.6f);
        foreach (var button in _buttons) { button.enabled = true; }
    }
}
