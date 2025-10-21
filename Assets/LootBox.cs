using UnityEngine;

public class LootBox : InteractuableGeneric
{
    [SerializeField] private GameObject[] _spawneables;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Animator _animator;
   
    private void Start()
    {
        _animator= GetComponent<Animator>();
        InteractManager.Instance.AddInteract(this);

        Physics.Raycast(transform.position, -transform.up, layerMask: _groundLayer, maxDistance: 10, hitInfo: out RaycastHit p);

        if(p.point!=null)
        {
            transform.position=p.point+transform.up*0.5f;
        }
    }

    public override void Interacting()
    {
        Activate();
        InteractManager.Instance.RemoveInteract(this);
    }
    public override void Activate()
    {
       if(_animator!=null)
       {
            _animator.SetBool("Open",true);
       }
    }
    public override void Desactivate()
    {
    }
    public void InstanceItem()
    {
        EventManager.Ejecute(EventManager.KindOfEvent.PowerSelect);
        EventManager.Ejecute(EventManager.KindOfEvent.PauseTime);
    }
}
