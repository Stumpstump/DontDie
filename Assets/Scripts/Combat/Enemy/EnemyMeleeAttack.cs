using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeAttack : MonoBehaviour
{
    [SerializeField] private bool useDynamicDamage;
    [SerializeField] private int Damage;
    [SerializeField] private int DynamicMaxDamage;
    [SerializeField] private float AttackRange;
    [SerializeField] private Vector3 AttackRangeTransformOffset;
    [SerializeField] private float AttackAngle;
    [SerializeField] private float AttackInterval;

    private GameObject ActivePlayer;
    private Animator animator;

    private float elapsedTimeSinceLastAttack;
    public float angleToPlayer
    {
        get
        {
            var playerDirection = ActivePlayer.transform.position + ActivePlayer.GetComponent<CharacterController>().center - transform.position + AttackRangeTransformOffset;
            return Vector3.Angle(playerDirection, transform.forward);           
        }
    }

    public float distanceToPlayer
    {
        get
        {
            return Vector3.Distance(ActivePlayer.transform.position + ActivePlayer.GetComponent<CharacterController>().center, transform.position);
        }
    }

    public bool isInAttackRange
    {
        get
        {
            return distanceToPlayer < AttackRange;
        }
    }

    private void Awake()
    {
        ActivePlayer = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        elapsedTimeSinceLastAttack += Time.deltaTime;        

        if (elapsedTimeSinceLastAttack > AttackInterval)
        {
            if(distanceToPlayer < AttackRange && angleToPlayer < AttackAngle)
            {
                animator.SetTrigger("Attack");
                elapsedTimeSinceLastAttack = 0f;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position + AttackRangeTransformOffset, AttackRange);
    }

    private void DealDamage()
    {        
        int damage = useDynamicDamage == false ? Damage : Random.Range(Damage, DynamicMaxDamage);
        ActivePlayer.GetComponent<Targetable>().ReceiveDamage(this, new DamageEventArgs(damage));
    }
}
