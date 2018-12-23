using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Combat
{
    [RequireComponent(typeof(Health))]
    public class Dummy : MonoBehaviour
    {
        [SerializeField] Health health;
        [SerializeField] bool Invincible;


        private void Update()
        {
            if (Invincible)
                health?.Reset();

            else if (health?.CurrentHealth <= 0)
                Destroy(this.gameObject);
        }


    }

}
