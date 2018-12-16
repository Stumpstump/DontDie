using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Health : MonoBehaviour
{
    [SerializeField] private int MaxHealth;
    [SerializeField] private int MaxBarrier;
    [ShowOnly] [SerializeField] private int currentHealth;
    [ShowOnly] [SerializeField] private int currentBarrier;
    [SerializeField] GameObject DamageText;

    public int CurrentHealth
    {
        get
        {
            return currentHealth;
        }

        private set
        {
            currentHealth = value;
        }
    }

    public int CurrentBarrier
    {
        get
        {
            return currentBarrier;
        }

        private set
        {
            currentBarrier = value;
        }
    }

    void Awake()
    {
        Reset();
        if(GetComponent<Targetable>())
            GetComponent<Targetable>().ReceiveDamageEvent += ReceiveDamage;
    }

    public void ReceiveDamage(object sender, DamageEventArgs Damage)
    {
        if(DamageText != null)
        {
            if(this.GetComponentInChildren<Canvas>())
            {
                GameObject newDamageText = Instantiate(DamageText, this.GetComponentInChildren<Canvas>().transform);
                newDamageText.GetComponent<Text>().text = Damage.Damage.ToString();
            }
        }

        int DamageAmount = Damage.Damage;
        if(DamageAmount > CurrentBarrier)
        {
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

    //Resets the health and barrier to the Max values 
    public void Reset()
    {
        CurrentHealth = MaxHealth;
        CurrentBarrier = MaxBarrier;
    }

    public void RegenBarrier(int RegenAmount)
    {
        if (CurrentBarrier + RegenAmount > MaxBarrier)
            CurrentBarrier = MaxBarrier;
        else
            CurrentBarrier += RegenAmount;
    }
}
