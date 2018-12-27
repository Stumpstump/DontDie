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
}

public class Targetable : MonoBehaviour
{
    public event EventHandler <DamageEventArgs> ReceiveDamageEvent;
    public int DamageAmplifier = 100;

    public void ReceiveDamage(object sender, DamageEventArgs amount)
    {
        amount.Damage *= DamageAmplifier / 100;
        ReceiveDamageEvent(sender, amount);
    }
}
