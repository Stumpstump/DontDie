using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyMeleeAttack))]
public class EnemyMeleeMovement : MonoBehaviour
{
    [SerializeField] private float visionRange;
    private NavMeshAgent localAgent;
    private EnemyMeleeAttack attackScript;
    private GameObject activePlayer;
    private Animator animator;
    private Combat.Health health;

    private void Awake()
    {
        localAgent = this.GetComponent<NavMeshAgent>();
        activePlayer = GameObject.FindGameObjectWithTag("Player");
        attackScript = this.GetComponent<EnemyMeleeAttack>();
        animator = this.GetComponent<Animator>();

        health = GetComponent<Combat.Health>();

        if (!health)
            health = GetComponentInChildren<Combat.Health>();

        health.dyingEvent += OnDyingEvent;

        Targetable[] targetables = GetComponentsInChildren<Targetable>();
        for(int i = 0; i < targetables.Length; i++)
        {
            targetables[i].ReceiveDamageEvent += OnReceivedDamage;
        }
    }

    private void Update()
    {
        animator.SetFloat("Speed", localAgent.velocity.magnitude);
        animator.SetInteger("Health", health.CurrentHealth);

        if (attackScript.isInAttackRange)
        {
            Vector3 playerLookAtPosition = activePlayer.transform.position + activePlayer.GetComponent<CharacterController>().center;
            playerLookAtPosition.y = transform.forward.y;
            localAgent.isStopped = true;
            transform.LookAt(playerLookAtPosition);
        }

        else if (attackScript.distanceToPlayer > visionRange)
            localAgent.isStopped = true;

        else
        {
            localAgent.SetDestination(activePlayer.transform.position);
            localAgent.isStopped = false;
        }
    }

    private void OnDyingEvent(object sender, EventArgs args)
    {
        animator.SetTrigger("Dying");
        attackScript.enabled = false;
        localAgent.enabled = false;
        this.enabled = false;
    }

    private void OnReceivedDamage(object sender, DamageEventArgs args)
    {
        animator.SetTrigger("TookDamage");
    }

    private void DestroyEnemy()
    {
        GameObject.Destroy(this.gameObject);
    }

}
