using UnityEngine;

public class ComboManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem _trail;
    [SerializeField] private GameObject _swordModel;
    [SerializeField] private LayerMask _hitLayer;
    [SerializeField] private float _dmgMultiply=1;
    private Vector3 _lastPosition;
    private Vector3 _velocity;
    public float SwordDistance;
    public float Dmg;
    public float SwordArea;
    private void Update()
    {
        _velocity = (_swordModel.transform.position - _lastPosition) / Time.deltaTime;
        _lastPosition = _swordModel.transform.position;
    }
    public void MakeDamage(int Dmg)
   {
       if(_trail!=null)
       {
            _trail.gameObject.SetActive(true);
            _trail.Play();
       }
       EventManager.Ejecute(EventManager.KindOfEvent.PjAttack);
   }
    public void TurnOffTrail()
    {
        if (_trail != null)
        {
            _trail.gameObject.SetActive(false);
            _trail.Stop();
        }
    }
    public void CauseDamage()
    {
        if (_trail != null)
        {
            _trail.gameObject.SetActive(true);
            _trail.Play();
        }
        Collider[] c = Physics.OverlapSphere(transform.position, SwordDistance, _hitLayer);
        foreach (Collider collider in c)
        {   if(collider.gameObject==gameObject)
            {
                continue;
            }
            Idamageable l= collider.GetComponent<Idamageable>();
            if (l != null)
            {
                l.TakeDamage(Dmg*_dmgMultiply,Dmg*_dmgMultiply/2,_velocity.normalized);
            }
            else
            {
                continue;
            }
        }
    }
    public void AddForceToEnemy()
    {
        Collider[] c = Physics.OverlapSphere(transform.position, SwordArea, _hitLayer);
        foreach(Collider collider in c)
        {
            if(collider.gameObject==gameObject)
            {
                continue;
            }
            Entity l =collider.GetComponent<Entity>();
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
