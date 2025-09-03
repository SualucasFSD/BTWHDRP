using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem _trail;
    [SerializeField] private Sword _sword;
    public void MakeDamage(int Dmg)
   {
       if(_trail!=null)
       {
            _trail.gameObject.SetActive(true);
            _trail.Play();
       }
       _sword.ResetDicctionary();
       _sword.Dmg = Dmg;
       EventManager.Ejecute(EventManager.KindOfEvent.PjAttack);
   }
    public void TurnOffTrail()
    {
        if (_trail != null)
        {
            _trail.gameObject.SetActive(false);
            _trail.Stop();
        }
        _sword.Dmg = 0;
    }
}
