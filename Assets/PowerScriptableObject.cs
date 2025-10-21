using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "PowerScriptableObject", menuName = "Scriptable Objects/PowerScriptableObject")]
public class PowerScriptableObject : ScriptableObject
{
    [Header("Datos de la carta")]
    public string Name;
    [TextArea(2, 5)] public string Text;
    [Range(0, 100)] public float RareNum;
    public Sprite Image;

    [Header("Tipo de efecto")]
    public EnemyCatalogue KindOfPower;
    public void Execute()
    {
        EventManager.Ejecute(EventManager.KindOfEvent.OnEnemyKilled, KindOfPower);
    }
}
