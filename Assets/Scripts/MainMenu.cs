using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Config")]
    [SerializeField] private string _sceneName;
    [SerializeField] private GameObject _loadImg;

    [Header("Panels")]
    [SerializeField] private GameObject _optionPanel;
    [SerializeField] private Stack<GameObject> _panels = new Stack<GameObject>();

    [Header("Buttons")]
    [SerializeField] private Button[] _buttons;
    [SerializeField] private Button _defaultButton;

    private Dictionary<ActionEjecute, Action> _dict = new Dictionary<ActionEjecute, Action>();
    private Dictionary<Button, float> _buttonCooldowns = new Dictionary<Button, float>();

    [SerializeField] private float _buttonCooldownTime = 0.5f;
    private float _buttonSelectedCooldown;
    private float _cancelPress=0;

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
        _buttonSelectedCooldown = 0;
        _dict.Add(ActionEjecute.Play, PlayButton);
        _dict.Add(ActionEjecute.Credits, CreditsButton);
        _dict.Add(ActionEjecute.Options, OptionsButton);
        _dict.Add(ActionEjecute.Close, CloseButton);
        _dict.Add(ActionEjecute.Exit, ExitButton);

        foreach (var b in _buttons)
        {
            _buttonCooldowns[b] = 0f;
        }

        //SelectButton(_defaultButton);
    }

    /* private void Update()
     {
         if (EventSystem.current.currentSelectedGameObject == null)
         {
             if (_panels.Count > 0)
             {
                 GameObject activePanel = _panels.Peek();
                 Button first = activePanel.GetComponentInChildren<Button>();
                 if (first != null)
                     SelectButton(first);
             }
             else if (_defaultButton != null)
             {
                 SelectButton(_defaultButton);
             }
         }

         if (Input.GetButton("Cancel"))
         {
             _cancelPress += Time.deltaTime;
             if (_cancelPress > 0.5f)
             {
                 CloseButton();
                 _cancelPress = 0f;
             }
         }
         else
         {
             _cancelPress = 0;
         }
     }*/
    private void Update()
    {
        bool moved =Mathf.Abs(Input.GetAxis("Vertical")) > 0.2f ||Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f ||Input.GetAxis("Mouse ScrollWheel") != 0;
        if (!moved)
        {
            _buttonSelectedCooldown += Time.deltaTime;
            if(_buttonSelectedCooldown>3f)
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
            else if (_defaultButton != null)
            {
                SelectButton(_defaultButton);
            }
        }
        if (Input.GetButton("Cancel"))
        {
            _cancelPress += Time.deltaTime;
            if (_cancelPress > 0.5f)
            {
                CloseButton();
                _cancelPress = 0f;
            }
        }
        else
        {
            _cancelPress = 0;
        }
    }
    public void Ejecute(int Index)
    {
        Button sender = EventSystem.current.currentSelectedGameObject?.GetComponent<Button>();

        if (sender != null)
        {
            if (Time.time < _buttonCooldowns[sender])
                return;

            _buttonCooldowns[sender] = Time.time + _buttonCooldownTime;
        }

        if (Index == 0)
        {
            _loadImg.SetActive(true);
        }

        StartCoroutine(Wait(_dict[(ActionEjecute)Index]));
    }

    private void PlayButton()
    {
        Invoke(nameof(InvokePlayButton), 2);
        if(TryGetComponent<MainMenuIntro>(out var compo))
        {
            print("Rutina");
            compo.StartFadeIn();
        }
    }
    private void InvokePlayButton()
    {
        SceneManager.LoadSceneAsync(_sceneName);
    }

    private void CreditsButton()
    {
        Debug.Log("Credits (pendiente implementar)");
    }

    private void OptionsButton()
    {
        if (_optionPanel != null)
        {
            _optionPanel.SetActive(true);
            _panels.Push(_optionPanel);

            Button first = _optionPanel.GetComponentInChildren<Button>();
            if (first != null)
                SelectButton(first);
        }
    }

    private void CloseButton()
    {
        if (_panels.Count > 0)
        {
            GameObject closed = _panels.Pop();
            closed.SetActive(false);

            SelectButton(_defaultButton);
        }
    }

    private void ExitButton()
    {
        Debug.Log("Exit");
        Application.Quit();
    }

    private IEnumerator Wait(Action FunctToDelay)
    {
        yield return new WaitForSeconds(0.2f);
        FunctToDelay();
    }

    private void SelectButton(Button btn)
    {
        if (btn == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(btn.gameObject);
    }
}
