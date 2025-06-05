using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Offensive_BT : MonoBehaviour
{
    [Header("References")]
    public CharacterCore core;
    public Transform enemy;
    public NavMeshAgent agent;

    [Header("Combat Settings")]
    public float attackRange = 2f;

    // 상대 공격 가능 여부 관리 변수
    public bool enemyIsBlocking = false;
    public float enemyDefenceTimer = 0;
    private bool prevEnemyIsBlocking = false;

    // 상대 CharacterCore
    private CharacterCore enemyCore;

    // 도망 상태 관리 변수
    private bool isFleeing = false;
    private float fleeTimer = 0f;
    private const float FLEE_DURATION = 1.0f;

    void Start()
    {
        // 랜덤 스폰 위치 설정
        float randX = Random.Range(5f, 10f);
        float randZ = Random.Range(-5f, 5f);
        transform.position = new Vector3(randX, transform.position.y, randZ);

        if (core == null)
            core = GetComponent<CharacterCore>();

        if (enemy == null)
        {
            GameObject[] enemyObjs = GameObject.FindGameObjectsWithTag("Character");
            foreach (var obj in enemyObjs)
            {
                if (obj != this.gameObject)
                {
                    var root = obj.transform.root;
                    var core = root.GetComponent<CharacterCore>();
                    if (core != null && root != this.transform)
                    {
                        enemy = root;
                        enemyCore = core;
                        break;
                    }

                }
            }
        }

        if (enemy != null)
        {
            //enemy = enemy.transform.root;
            enemyCore = enemy.GetComponent<CharacterCore>();
        }

        if (agent != null && core != null)
            agent.speed = core.speed;

        // 시작하자마자 공격/방어/구르기 불가
        core.attackTimer = 1.0f;
        core.defenceTimer = 1.0f;
        core.dodgeTimer = 1.0f;
    }

    void Update()
    {
        if (core.isDead)
        {
            agent.ResetPath();
            return;
        }

        // 상대 공격 실시 여부 받아오기
        enemyDefenceTimer -= Time.deltaTime;
        if (enemyDefenceTimer < 0f) enemyDefenceTimer = 0f;

        if (!prevEnemyIsBlocking && enemyCore.isBlocking)
        {
            enemyDefenceTimer += 2.5f;
        }
        prevEnemyIsBlocking = enemyCore.isBlocking;
        enemyIsBlocking = (enemyDefenceTimer > 0f) ? true : false;

        // 도망 상태 타이머
        if (isFleeing)
        {
            fleeTimer += Time.deltaTime;
            if (fleeTimer >= FLEE_DURATION)
            {
                isFleeing = false;
                fleeTimer = 0f;
            }
        }

        // 동작
        HandleState();
    }

    void HandleState()
    {
        // 둘 중 하나 사망시 종료
        if (core.isDead || enemyCore.isDead)
            return;

        float distanceToEnemy = Vector3.Distance(transform.position, enemy.position);
        bool inRange = distanceToEnemy <= attackRange;

        if (enemy == null || enemyCore == null)
            return;

        // 둘 중 하나 사망시 종료
        if (core.state == PlayerState.Dead || enemyCore.state == PlayerState.Dead)
            return;

        // 도망 방향 설정
        if (isFleeing && !core.isAttacking)
        {
            if (enemy != null)
            {
                Vector3 Center = Vector3.zero;
                Vector3 awayFromEnemy = (transform.position - enemy.position).normalized;
                Vector3 toCenter = (Center - transform.position).normalized;
                float centerWeight = 0.3f;
                Vector3 fleeDir = (awayFromEnemy * (1f - centerWeight) + toCenter * centerWeight).normalized;
                core.HandleMovement(fleeDir.x, fleeDir.z);
                agent.ResetPath();
            }
            return;
        }

        // 이미 공격/방어/회피 중이면 아무것도 하지 않음
        if (core.state == PlayerState.Attacking ||
            core.state == PlayerState.Defending ||
            core.state == PlayerState.Dodging)
        {
            agent.ResetPath();
            return;
        }

        // 상태에 따라 행동 결정
        switch (core.state)
        {
            case PlayerState.Idle:
            case PlayerState.Moving:
                Vector3 dir = (enemy.position - transform.position).normalized;
                core.HandleMovement(dir.x, dir.z);
                if (enemy != null)
                {
                    // 1. 범위 밖 -> 접근
                    if (!inRange && !isFleeing)
                        agent.SetDestination(enemy.position);
                    else
                        core.HandleMovement(0, 0);

                    // 도망치는 중 아님
                    if (!isFleeing)
                    {
                        // 범위 안
                        if(inRange)
                        {
                            // 상대가 방어중 아님
                            if (!enemyIsBlocking)
                            {
                                // 2. 공격 가능 -> 공격 후 도망
                                if (core.CanAttack())
                                {
                                    core.HandleMovement(0, 0);
                                    LookAtEnemy();
                                    core.Attack();
                                    agent.ResetPath();
                                    Flee();
                                }
                                // 3. 공격 불가 + 회피 가능 -> 회피
                                else if (!core.CanAttack() && core.CanDodge())
                                {
                                    core.Dodge();
                                    agent.ResetPath();
                                }
                            }
                        }
                    }
                }
                break;
            case PlayerState.Defending:
            case PlayerState.Attacking:
            case PlayerState.Dodging:
            case PlayerState.Dead:
                agent.ResetPath();
                break;
        }
    }

    // 도망
    void Flee()
    {
        isFleeing = true;
        fleeTimer = 0f;
    }
    void LookAtEnemy()
    {
        if (enemy == null) return;
        Vector3 direction = (enemy.position - transform.position).normalized;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(1500, 10, 300, 20), $"Offensive: {core.cur_hp}");
        GUI.Label(new Rect(1500, 30, 300, 20), $"Offensive isBlocking: {core.isBlocking}");
        GUI.Label(new Rect(1500, 50, 300, 20), $"Offensive isAttacking: {core.isAttacking}");
        GUI.Label(new Rect(1500, 70, 300, 20), $"Offensive isDodging: {core.isDodging}");
        GUI.Label(new Rect(1500, 90, 300, 20), $"Offensive attackTimer: {core.attackTimer}");
        GUI.Label(new Rect(1500, 110, 300, 20), $"Offensive defenceTimer: {core.defenceTimer}");
        GUI.Label(new Rect(1500, 130, 300, 20), $"Offensive canDefence: {core.CanDefence()}");
        GUI.Label(new Rect(1500, 150, 300, 20), $"Offensive canAttack: {core.CanAttack()}");
        GUI.Label(new Rect(1500, 170, 300, 20), $"Offensive canDodge: {core.CanDodge()}");

    }
}
