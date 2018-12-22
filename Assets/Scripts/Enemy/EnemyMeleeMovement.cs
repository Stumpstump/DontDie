using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyMeleeAttack))]
public class EnemyMeleeMovement : MonoBehaviour
{
    [SerializeField] private float visionRange;
    private NavMeshAgent localAgent;
    private EnemyMeleeAttack attackScript;

    private GameObject activePlayer;

    private void Awake()
    {
        localAgent = this.GetComponent<NavMeshAgent>();
        activePlayer = GameObject.FindGameObjectWithTag("Player");
        attackScript = this.GetComponent<EnemyMeleeAttack>();
    }

    private void Update()
    {
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
}
