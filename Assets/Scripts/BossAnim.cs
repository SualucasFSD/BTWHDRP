using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAnim : MonoBehaviour
{
    FirstBoss _boss;
    private void Awake()
    {
        _boss= GetComponentInParent<FirstBoss>();
    }
    public void Golpe()
    {
        _boss.GolpeDamage();
    }
    public void Burst()
    {
        _boss.FrontBurst();
    }
}
