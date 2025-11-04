using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum KindOfReward
{
    Gold,
    Diamonds,
    Power
}

public class Cards : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;

    [Header("Configuración")]
    [SerializeField] private KindOfReward _rewardType;
    [SerializeField] private bool _CantChoose;
    [SerializeField] private PowerScriptableObject[] _powers;
    [SerializeField] private Sprite _sprite;
    private float _amount;
    private PowerScriptableObject _selectedPower;

    public void Randomized()
    {
        if (_CantChoose)
        {
            if (_rewardType == KindOfReward.Gold)
            {
                _amount = Random.Range(0, 3) switch
                {
                    1 => 500,
                    2 => 1000,
                    _ => 1500
                };
                _nameText.text = "Oro";
                _descriptionText.text = $"Otorga un valor de {_amount} oros";
                if (_sprite != null)
                {
                    _image.sprite = _sprite;
                }
            }
            else if (_rewardType == KindOfReward.Diamonds)
            {
                _amount = Random.Range(0, 3) switch
                {
                    1 => 200,
                    2 => 500,
                    _ => 800
                };
                _nameText.text = "Diamantes";
                _descriptionText.text = $"Otorga un valor de {_amount} gemas";
                if (_sprite != null)
                {
                    _image.sprite = _sprite;
                }
            }

            _selectedPower = null;
            return;
        }

        if (_powers == null || _powers.Length == 0)
        {
            Debug.LogWarning($"No hay poderes asignados en {_rewardType}");
            return;
        }

        float dificult = GameManager.Instance.DificultLevel;
        float totalWeight = 0f;
        float[] weights = new float[_powers.Length];

        for (int i = 0; i < _powers.Length; i++)
        {
            var power = _powers[i];
            if (power == null)
            {
                weights[i] = 0;
                continue;
            }

            float adjusted = power.RareNum * (1f + dificult * 0.1f);
            if (adjusted < 1f) adjusted = 1f;
            weights[i] = adjusted;
            totalWeight += adjusted;
        }

        float randomPoint = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        for (int i = 0; i < _powers.Length; i++)
        {
            cumulative += weights[i];
            if (randomPoint <= cumulative)
            {
                _selectedPower = _powers[i];
                break;
            }
        }

        if (_selectedPower == null)
        {
            Debug.LogWarning("No se seleccionó ningún poder válido.");
            return;
        }

        _nameText.text = _selectedPower.Name;
        _descriptionText.text = _selectedPower.Text;
        if (_selectedPower.Image != null)
            _image.sprite = _selectedPower.Image;
    }

    public void Selected()
    {
        if (_rewardType == KindOfReward.Gold)
        {
            SaveSystemManager.instance.GenericSave.Gold += _amount;
            SaveSystemManager.instance.SaveData(4);
        }
        else if (_rewardType == KindOfReward.Diamonds)
        {
            SaveSystemManager.instance.GenericSave.Diamond += _amount;
            SaveSystemManager.instance.SaveData(4);
        }
        else if (_rewardType == KindOfReward.Power && _selectedPower != null)
        {
            _selectedPower.Execute();
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        EventManager.Ejecute(EventManager.KindOfEvent.ResumeTime);
    }
}
