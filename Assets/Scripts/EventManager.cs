using System.Collections.Generic;

public class EventManager
{
    public enum KindOfEvent
    {
        ChangeLanguage,
        ReloadNodes,
        ReloadPath,
        RefreshCont,
        ReloadEnemyList,
        PauseGame,
        PjAttack,
        OnEnemyKilled,
        ChangeSensibilitieMouse,
        OnDeath,
        MaxLifeReach,
        JumpPj,
        KnightJumpReset,
        KnightExecuteDodge,
        OnLockCamera,
        OnChangeTarget,
        ResetLevel,
        MainMenu,
        LifeUpdater,
        CameraSmooth,
        CameraDistance,
        OnPjChangePosition,
        OnLoadScene,
        OnChangeScene,
        OnChangeResolution,
        MakeCameraShake
    }
    public delegate void MethodToSuscribe(params object[] Parameters);
    static Dictionary<KindOfEvent, MethodToSuscribe> _events;

    public static void Suscribe(KindOfEvent _type, MethodToSuscribe _method)
    {
        if (_events == null) _events = new Dictionary<KindOfEvent, MethodToSuscribe>();
        //Similar a _events ??= new Dictionary<EventType, MethodToSuscribe>();
        if (!_events.ContainsKey(_type))
        {
            _events.Add(_type, null);
        }
        //Similar a: _events.TryAdd(_type, null);
        _events[_type] += _method;
    }

    public static void Unscribe(KindOfEvent _type, MethodToSuscribe _method)
    {
        if (_events == null) return;
        if (!_events.ContainsKey(_type)) { return; }
        _events[_type] -= _method;
    }
    public static void Ejecute(KindOfEvent _type,params object[] parameters)
    {
        if(_events == null) return;
        if(!_events.ContainsKey(_type)) return;
        if(_events[_type]==null) return;
        _events[_type](parameters);
    }
    public static void ResetEvent()
    {
        _events.Clear();
    }
}
