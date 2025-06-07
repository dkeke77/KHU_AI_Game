using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Defensive_BT : MonoBehaviour
{
    [Header("References")]
    public CharacterCore core;
    public Transform enemy;
    public NavMeshAgent agent;

    [Header("Combat Settings")]
    public float attackRange = 2f;

    // 상대 공격 가능 여부 관리 변수
    public bool enemyIsAttacking = false;
    public float enemyAttackTimer = 0;
    private bool prevEnemyIsAttacking = false;

    // 상대 CharacterCore
    private CharacterCore enemyCore;

    // 도망 상태 관리 변수
    private bool isFleeing = false;
    private float fleeTimer = 0f;
    private const float FLEE_DURATION = 3.0f;

    void Start()
    {/*
        // 랜덤 스폰 위치 설정
        float randX = Random.Range(-10f, -5f);
        float randZ = Random.Range(-5f, 5f);
        transform.position = new Vector3(randX, transform.position.y, randZ);*/

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

        if (enemy != null && enemyCore == null)
        {
            //enemy = enemy.transform.root;
            enemyCore = enemy.GetComponent<CharacterCore>();
        }

        if (agent != null && core != null)
            agent.speed = core.speed;
        /*
        // 시작하자마자 공격/방어/구르기 불가
        core.attackTimer = 1.0f;
        core.defenceTimer = 1.0f;
        core.dodgeTimer = 1.0f;*/
    }

    void Update()
    {
        Debug.Log(core.speed);
        if (core.isDead)
        {
            agent.ResetPath();
            return;
        }

        // 상대 공격 실시 여부 받아오기
        enemyAttackTimer -= Time.deltaTime;
        if (enemyAttackTimer < 0f) enemyAttackTimer = 0f;

        if (!prevEnemyIsAttacking && enemyCore.isAttacking)
        {
            enemyAttackTimer += 2.5f;
        }
        prevEnemyIsAttacking = enemyCore.isAttacking;
        enemyIsAttacking = (enemyAttackTimer > 0f) ? true : false;

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

    // 거리 벌리기
    public void KeepDistanceFromEnemy(float minDistance)
    {
        if (enemy == null || core == null)
            return;

        float distance = Vector3.Distance(transform.position, enemy.position);
        if (distance < minDistance)
        {
            // enemy 반대 방향으로 이동
            Vector3 awayDir = (transform.position - enemy.position).normalized;
            if (core.CanMove())
                core.HandleMovement(awayDir.x, awayDir.z);

            // NavMeshAgent 사용 시, 현재 위치에서 minDistance만큼 떨어진 지점으로 목적지 설정
            if (agent != null)
            {
                Vector3 targetPos = transform.position + awayDir * (minDistance - distance + 0.5f);
                agent.SetDestination(targetPos);
            }
        }
        else
        {
            // 이미 충분히 멀면 멈춤
            core.HandleMovement(0, 0);
            if (agent != null)
                agent.ResetPath();
            return;
        }
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

        // 이미 공격/방어/회피 중이면 아무것도 하지 않음
        if (core.state == PlayerState.Attacking ||
            core.state == PlayerState.Defending ||
            core.state == PlayerState.Dodging)
        {
            if (agent != null)
                agent.ResetPath();
            return;
        }

        // 도망 방향 설정
        if (isFleeing)
        {
            if (enemy != null)
            {
                Vector3 Center = Vector3.zero;
                Vector3 awayFromEnemy = (transform.position - enemy.position).normalized;
                Vector3 toCenter = (Center - transform.position).normalized;
                float centerWeight = 0.3f;
                Vector3 fleeDir = (awayFromEnemy * (1f - centerWeight) + toCenter * centerWeight).normalized;
                if (core.CanMove())
                    core.HandleMovement(fleeDir.x, fleeDir.z);
                if (agent != null)
                    agent.ResetPath();
            }

            // 도망중에 상대 공격
            if (enemyIsAttacking)
            {
                // 방어 가능 -> 방어 후 방어 타이머 리필
                if (core.CanDefence())
                {
                    //core.HandleMovement(0, 0);
                    LookAtEnemy();
                    core.Defence();
                    if (agent != null)
                        agent.ResetPath();
                    isFleeing = false;
                    fleeTimer = 0f;
                    return;
                    // 타이머 리필 구현 필요
                }

                // 방어 불가능 + 회피 가능 -> 회피
                else if (!core.CanDefence() && core.CanDodge())
                {
                    core.Dodge();
                    if (agent != null)
                        agent.ResetPath();
                }
            }
            return;
        }

        // 상태에 따라 행동 결정
        switch (core.state)
        {
            case PlayerState.Idle:
            case PlayerState.Moving:
                Vector3 dir = (enemy.position - transform.position).normalized;
                if (core.CanMove() && distanceToEnemy > 0.9f)
                    core.HandleMovement(dir.x, dir.z);
                if (enemy != null)
                {
                    if (!inRange && !isFleeing)
                    {
                        if (agent != null)
                            agent.SetDestination(enemy.position);
                    }
                    //else
                        //core.HandleMovement(0, 0);

                    // 도망치는 중이 아님
                    if (!isFleeing)
                    {
                        // 범위 안
                        if (inRange)
                        {
                            // 상대가 공격중
                            if (enemyIsAttacking)
                            {
                                // 1. 내가 방어 가능 -> 방어 후 도망
                                if (core.CanDefence())
                                {
                                    //core.HandleMovement(0, 0);
                                    LookAtEnemy();
                                    core.Defence();
                                    if (agent != null)
                                        agent.ResetPath();
                                    Flee();
                                }

                                // 2. 내가 방어 불가능 + 내가 회피 가능 -> 회피
                                else if (!core.CanDefence() && core.CanDodge())
                                {
                                    core.Dodge();
                                    if (agent != null)
                                        agent.ResetPath();
                                }

                                // 3. 내가 방어/회피 불가능 + 내가 공격 가능 -> 공격 후 도망
                                else if(!core.CanDefence() && !core.CanDodge() && core.CanAttack())
                                {
                                    //core.HandleMovement(0, 0);
                                    LookAtEnemy();
                                    core.Attack();
                                    if (agent != null)
                                        agent.ResetPath();
                                    Flee();
                                }

                                // 4. 내가 전부 불가능 -> 도망
                                else
                                {
                                    if (agent != null)
                                        agent.ResetPath();
                                    Flee();
                                }
                            }

                            // 상대가 공격중 아님
                            else 
                            {
                                // 5. 내가 공격 가능 -> 공격 후 도망
                                if (core.CanAttack())
                                {
                                    //core.HandleMovement(0, 0);
                                    LookAtEnemy();
                                    core.Attack();
                                    if (agent != null)
                                        agent.ResetPath();
                                    Flee();
                                }

                                // 6. 내가 공격 불가 -> 거리 벌리기
                                else
                                {
                                    if (agent != null)
                                        agent.ResetPath();
                                    KeepDistanceFromEnemy(3.0f);
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
                if (agent != null)
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

    // 공격/방어시 상대와 방향 맞춤
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

    // 디버그용
    void OnGUI()
    {/*
        GUI.Label(new Rect(70, 10, 300, 20), $"Defensive: {core.cur_hp}");
        GUI.Label(new Rect(70, 30, 300, 20), $"Defensive isBlocking: {core.isBlocking}");
        GUI.Label(new Rect(70, 50, 300, 20), $"Defensive isAttacking: {core.isAttacking}");
        GUI.Label(new Rect(70, 70, 300, 20), $"Defensive isDodging: {core.isDodging}");
        GUI.Label(new Rect(70, 90, 300, 20), $"Defensive attackTimer: {core.attackTimer}");
        GUI.Label(new Rect(70, 110, 300, 20), $"Defensive defenceTimer: {core.defenceTimer}");
        GUI.Label(new Rect(70, 130, 300, 20), $"Defensive canDefence: {core.CanDefence()}");
        GUI.Label(new Rect(70, 150, 300, 20), $"Defensive canAttack: {core.CanAttack()}");
        GUI.Label(new Rect(70, 170, 300, 20), $"Defensive canDodge: {core.CanDodge()}");*/
    }
}

