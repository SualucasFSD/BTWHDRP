using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;
public enum Language
{
    Spanish,
    English
}
public class LocalizationManager : MonoBehaviour
{
    private bool _isFirstTime = true;
    public bool IsFinish=false;
    public static LocalizationManager Instance;
    // Link: https://docs.google.com/spreadsheets/d/e/2PACX-1vS-OwHX3z5DLHT9v8bQW8wynM4g_M70MypL6g3_7b2NGYXXonEBka2JKxClkjoNDqA-c4MocjRQdOlg/pub?output=csv
    [SerializeField] private string _webUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vS-OwHX3z5DLHT9v8bQW8wynM4g_M70MypL6g3_7b2NGYXXonEBka2JKxClkjoNDqA-c4MocjRQdOlg/pub?output=csv";
    [SerializeField] private Language _currentLanguage;
    public Language CurrentLanguage {  get { return _currentLanguage; }}
    Dictionary<Language, Dictionary<string, string>> _localization;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        StartCoroutine(DownloadInf(_webUrl));
    }
    public void ChangeIdiom()
    {
        _currentLanguage=_currentLanguage==Language.Spanish? Language.English: Language.Spanish;
        EventManager.Ejecute(EventManager.KindOfEvent.ChangeLanguage);
    }
    IEnumerator DownloadInf(string url)
    {
        UnityWebRequest unityWebRequest = new UnityWebRequest(url);
        unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
        //unityWebRequest.Abort();
        yield return unityWebRequest.SendWebRequest();
        if(unityWebRequest.result== UnityWebRequest.Result.Success)
        {
            string result = unityWebRequest.downloadHandler.text;
            _localization = SheetSplit.LoadCsv(result, "web");
            SaveToDisk(nameof(LocalizationManager), result);
        }
        else
        {
            string result = LoadFromDisk(nameof(LocalizationManager));
            _localization = SheetSplit.LoadCsv(result, "disk");
        }
        if(_isFirstTime)
        {
            EventManager.Ejecute(EventManager.KindOfEvent.ChangeLanguage);
        }
        else
        {
            ChangeIdiom();
        }
        IsFinish=true;
        //Debug.Log(result);
    }
    public string TranslateText(string id)
    {
        Dictionary<string,string> idsDictionary = _localization[_currentLanguage];
        idsDictionary.TryGetValue(id, out var result);
        return result;
    }
    void SaveToDisk(string fileName, string Content)
    {
#if UNITY_EDITOR
        string diskPath = Application.dataPath+"/"+fileName;
#endif
#if !UNITY_EDITOR
        string diskPath = Application.persistentDataPath+"/"+fileName;
#endif
        try
        {
            File.WriteAllText(diskPath, Content);
            Debug.Log($"File save OK at: {diskPath}");
        }
        catch(Exception e)
        {
            Debug.LogWarning($"Fail to save file: {e}");
        }
    }

    public string LoadFromDisk(string fileName)
    {
#if UNITY_EDITOR
        string diskPath = Application.dataPath + "/" + fileName;
#endif
#if !UNITY_EDITOR
        string diskPath = Application.persistentDataPath+"/"+fileName;
#endif
        try
        {
            if(File.Exists(diskPath))
            {
                string content=File.ReadAllText(diskPath);
                Debug.Log($"File load Successfully from: {diskPath}");
                return content;
            }
            else
            {
                Debug.LogError("File not found");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Fail to load file: {e}");
            return null;
        }
    }
}
