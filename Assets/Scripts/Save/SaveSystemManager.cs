using System.IO;
using UnityEngine;

public class SaveSystemManager : MonoBehaviour
{
    public static SaveSystemManager instance;
    public DataSave[] _saveDatas = new DataSave[4];
    public DataSave GenericSave;
    private string _path;

    private string[] _fileName = {
        "/SaveData0.sv",
        "/SaveData1.sv",
        "/SaveData2.sv",
        "/SaveData3.sv",
        "/SaveDataGeneric.sv"
    };

    private void Awake()
    {
        GenericSave = new DataSave();
        for (int i = 0; i < _saveDatas.Length; i++)
        {
            _saveDatas[i] = new DataSave();
        }

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

#if UNITY_EDITOR
        _path = Application.dataPath;
#else
        _path = Application.persistentDataPath;
#endif

        Directory.CreateDirectory(_path);
        FirstLoad();
    }
    public void LoadAll()
    {
        for(int i = 0; i < _saveDatas.Length;i++)
        {
            LoadData(i);
        }
    }
    public void SaveData(int i)
    {
        if (i >= _saveDatas.Length)
        {
            Debug.LogWarning("Index fuera de rango al guardar");
            return;
        }

        _saveDatas[i].Save = true;
        string json = JsonUtility.ToJson(_saveDatas[i], true);
        File.WriteAllText(_path + _fileName[i], json);
    }

    public void FirstLoad()
    {
        if (File.Exists(_path + _fileName[4]))
        {
            string json = File.ReadAllText(_path + _fileName[4]);
            JsonUtility.FromJsonOverwrite(json, GenericSave);
            Debug.Log("Cargue Perfecto Generic");
        }
    }

    public void LoadData(int i)
    {
        if (i >= _saveDatas.Length)
        {
            Debug.LogWarning("Index fuera de rango al cargar");
            return;
        }

        if (File.Exists(_path + _fileName[i]))
        {
            string json = File.ReadAllText(_path + _fileName[i]);
            JsonUtility.FromJsonOverwrite(json, _saveDatas[i]);
            Debug.Log("Cargue Perfecto Save " + i);
        }
    }

    public void DeleteData(int i)
    {
        if (i >= _saveDatas.Length)
        {
            Debug.LogWarning("Index fuera de rango al borrar");
            return;
        }
        _saveDatas[i].Save=false;
        string filePath = _path + _fileName[i];
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("Archivo eliminado: " + filePath);
        }
    }
}
