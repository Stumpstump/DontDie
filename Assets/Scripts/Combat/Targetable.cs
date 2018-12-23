using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DamageEventArgs : EventArgs
{
    public DamageEventArgs(int damage)
    {
        Damage = damage;
    }

    public int Damage;

    public float DamageAmplifier = 100;
}

public class Targetable : MonoBehaviour
{
    public event EventHandler <DamageEventArgs> ReceiveDamageEvent;

    public void ReceiveDamage(object sender, DamageEventArgs amount)
    {
        ReceiveDamageEvent(sender, amount);
    }
}
