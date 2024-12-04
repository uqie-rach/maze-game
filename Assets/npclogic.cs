using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Supercyan.FreeSample;

public class npclogic : MonoBehaviour
{
    public Transform[] waypoints;        // Waypoints untuk patroli
    public float ChaseRange = 10f;       // Jarak untuk mulai mengejar pemain
    public float AttackRange = 2f;       // Jarak untuk menyerang pemain
    public float DamageAmount = 2f;     // Jumlah kerusakan yang diberikan NPC
    public float WalkSpeed = 2f;
    public float RunSpeed = 5f;
    public float attackCooldown = 2f;   // Waktu antara serangan

    private NavMeshAgent agent;
    private Animator anim;
    private Transform target;
    private SimpleSampleCharacterControl playerScript;
    private float distanceToTarget;

    private Coroutine damageCoroutine;
    private int waypointIndex = 0;
    private Vector3 patrolTarget;
    private float lastAttackTime;

    private void Start()
    {
        playerScript = FindObjectOfType<SimpleSampleCharacterControl>();
        if (playerScript != null)
        {
            target = playerScript.transform;
        }
        else
        {
            Debug.LogError("Player with SimpleSampleCharacterControl script not found!");
        }

        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        agent.speed = WalkSpeed;
        UpdatePatrolTarget();
    }

    private void Update()
    {
        if (playerScript == null || target == null)
        {
            Patrol();
            return;
        }

        distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= AttackRange)
        {
            Attack();
        }
        else if (distanceToTarget <= ChaseRange)
        {
            ChaseTarget();
            StopDamage();
        }
        else
        {
            Patrol();
            StopDamage();
        }
    }

    private void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // Jika sudah dekat dengan waypoint, pindah ke waypoint berikutnya
        if (Vector3.Distance(transform.position, patrolTarget) < 1.5f)
        {
            IterateWaypointIndex();
            UpdatePatrolTarget();
        }

        agent.SetDestination(patrolTarget);
        agent.speed = WalkSpeed;
        anim.SetBool("Run", true);
        anim.SetBool("Walk", true);
        anim.SetBool("Attack", false);
    }

    private void UpdatePatrolTarget()
    {
        patrolTarget = waypoints[waypointIndex].position;
    }

    private void IterateWaypointIndex()
    {
        waypointIndex = (waypointIndex + 1) % waypoints.Length;
    }

    private void ChaseTarget()
    {
        agent.SetDestination(target.position);
        agent.speed = RunSpeed;
        anim.SetBool("Run", true);
        anim.SetBool("Walk", false);
        anim.SetBool("Attack", false);
    }

    private void Attack()
    {
        agent.ResetPath();
        anim.SetBool("Run", false);
        anim.SetBool("Walk", false);
        anim.SetBool("Attack", true);

        if (damageCoroutine == null && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            StartDamage();
        }
    }

    private void StartDamage()
    {
        if (damageCoroutine == null)
        {
            damageCoroutine = StartCoroutine(ApplyDamageOverTime());
        }
    }

    private void StopDamage()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
            anim.SetBool("Attack", false);
        }
    }

    private IEnumerator ApplyDamageOverTime()
    {
        while (true)
        {
            playerScript.PlayerGetHit(DamageAmount);
            yield return new WaitForSeconds(attackCooldown);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, ChaseRange);
    }
}
