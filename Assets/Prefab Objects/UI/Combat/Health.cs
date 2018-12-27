using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


namespace Combat
{
    public class Health : MonoBehaviour
    {
        public event EventHandler dyingEvent;

        [SerializeField] private int MaxHealth;
        [SerializeField] private int MaxBarrier;
        [ShowOnly] [SerializeField] private int currentHealth;
        [ShowOnly] [SerializeField] private int currentBarrier;
        [SerializeField] GameObject DamageText;

        [Header("Player death state values")]
        [SerializeField] private float _timeInBleedingOutState;
        [SerializeField] private float _timeInDeadState;

        public float TimeInBleedingOutState
        {
            get
            {
                return _timeInBleedingOutState;
            }
        }

        public float TimeInDeadState
        {
            get
            {
                return _timeInDeadState;
            }
        }


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

            Targetable[] targetables = GetComponentsInChildren<Targetable>();
            for (int i = 0; i < targetables.Length; i++)
            {
                Debug.Log("a");
                targetables[i].ReceiveDamageEvent += ReceiveDamage;
            }
        }

        public void ReceiveDamage(object sender, DamageEventArgs Damage)
        {
            if (currentHealth <= 0) return;

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

            if (CurrentHealth <= 0 && dyingEvent != null) dyingEvent(this, new EventArgs());
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

}
