using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DamageEventArgs : EventArgs
{
    public DamageEventArgs(float damage)
    {
        Damage = damage;
    }

    public float Damage;
}

public class Targetable : MonoBehaviour
{
    event EventHandler <DamageEventArgs> ReceiveDamageEvent;

    public void ReceiveDamagea(object sender, DamageEventArgs a)
    {
        Debug.Log(a.Damage);
    }
}
