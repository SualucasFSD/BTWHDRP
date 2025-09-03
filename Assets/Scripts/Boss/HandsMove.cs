using System.Collections;
using UnityEngine;

public class HandsMove : MonoBehaviour
{
    public bool _onMove;
    private Rigidbody _rigidbody;
    public GameObject _player;
    public float Height;
    private bool _isActive=false;
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        EventManager.Suscribe(EventManager.KindOfEvent.OnDeath, OnPjKilled);
    }
    private void FixedUpdate()
    {
        if(_onMove&&_isActive)
        {
            transform.position = transform.position+ (_player.transform.position - transform.position).normalized* 10 * Time.deltaTime;
            //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-transform.position + new Vector3(_player.transform.position.x, transform.position.y, _player.transform.position.z)), 2 * 0.75f * Time.fixedDeltaTime);
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-transform.position + new Vector3(_player.transform.position.x, transform.position.y, _player.transform.position.z)), 2 * 0.75f * Time.fixedDeltaTime);
    }
   public IEnumerator Initialize()
    {
        while(transform.position.y<Height)
        {
            transform.position +=Vector3.up* Time.deltaTime*2;
            yield return null;
        }
        _isActive = true;
    }
    private void OnPjKilled(params object[] p)
    {
        enabled = false;
    }
    private void OnDestroy()
    {
        EventManager.Unscribe(EventManager.KindOfEvent.OnDeath, OnPjKilled);
    }
}
