using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int MaxHealth;
    [SerializeField] private int CurrentHealth;

    [SerializeField] private int MaxBarrier;
    [SerializeField] private int CurrentBarrier;

    void Awake()
    {
        CurrentHealth = MaxHealth;
        CurrentBarrier = MaxBarrier;
    }

    public void DealDamage(int DamageAmount)
    {
        if(DamageAmount > CurrentBarrier)
        {
            Debug.Log("DamageAmount");
            DamageAmount -= CurrentBarrier;

            CurrentBarrier = 0;

            CurrentHealth -= DamageAmount;
        }

        else
        {
            CurrentBarrier -= DamageAmount;
        }

    }

    public void RegenHealth(int RegenAmount)
    {
        if (CurrentHealth + RegenAmount > MaxHealth)
            CurrentHealth = MaxHealth;
        else
            CurrentHealth += RegenAmount;
    }

    public void RegenBarrier(int RegenAmount)
    {
        if (CurrentBarrier + RegenAmount > MaxBarrier)
            CurrentBarrier = MaxBarrier;
        else
            CurrentBarrier += RegenAmount;
    }
}
