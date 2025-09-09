using UnityEngine;

public class ComboManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem _trail;
    [SerializeField] private Sword _sword;
    [SerializeField] private LayerMask _hitLayer;
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
    public void AddForceToEnemy()
    {
        Collider[] c = Physics.OverlapSphere(transform.position, 3f, _hitLayer);
        foreach(Collider collider in c)
        {
            SkeletonEnemyModel l =collider.GetComponent<SkeletonEnemyModel>();
            if (l!=null)
            {
                l.FlyFunct(4);
            }
            else
            {
                continue;
            }
        }
    }
}
