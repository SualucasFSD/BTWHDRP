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
    [SerializeField] private GameObject _optionPanel;
    [SerializeField] private Button[] _buttons;
    [SerializeField] private Stack<GameObject> _panels = new Stack<GameObject>();
    public enum ActionEjecute
    {
       Play,
       Exit,
       Credits,
       Options,
       Close
    }
    private void Start()
    {
        _dict.Add(ActionEjecute.Play,PlayButton);
        _dict.Add(ActionEjecute.Credits, CreditsButton);
        _dict.Add(ActionEjecute.Options, OptionsButton);
        _dict.Add(ActionEjecute.Close, CloseButton);
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
        if (_optionPanel != null)
        {
            _optionPanel.SetActive(true);
            _panels.Push(_optionPanel);
        }
  }
  private void CloseButton()
  {
        if(_panels.Count>0)
        {
            _panels.Pop().SetActive(false);
        }
  }
  private void ExitButton()
  {
        print("Exit");
        Application.Quit();
  }
    private IEnumerator Wait(Action FunctToDelay)
    {
        yield return new WaitForSeconds(0.5f);
        FunctToDelay();
    }
    private IEnumerator PauseButtons()
    {
        foreach (var button in _buttons) { button.enabled = false; }
        yield return new WaitForSeconds(1.6f);
        foreach (var button in _buttons) { button.enabled = true; }
    }
}
