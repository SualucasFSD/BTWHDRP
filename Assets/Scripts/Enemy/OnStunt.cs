using System;
using UnityEngine;

public class OnStunt : IState
{
    private float _stuntTime;
    private float _stuntProgres;
    private Action _afterStunt;
    private Animator _animator;
    private string _triggerName;
    public OnStunt(float stuntTime,Action afterStunt, Animator anim,string triggerName)
    {
        _stuntTime = stuntTime;
        _afterStunt = afterStunt;
        _animator = anim;
        _triggerName = triggerName;
    }
    public void OnEnter()
    {
        _stuntProgres = 0;
        //_animator.SetTrigger(_triggerName);
    }

    public void OnExit()
    {
        //_animator.ResetTrigger(_triggerName);
    }

    public void OnUpdate()
    {
        _stuntProgres += Time.deltaTime;
        if ( _stuntProgres>=_stuntTime  )
        {
            _afterStunt();
        }
    }
}
