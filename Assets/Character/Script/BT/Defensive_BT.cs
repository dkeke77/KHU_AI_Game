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

    // ��� ���� ���� ���� ���� ����
    public bool enemyIsAttacking = false;
    public float enemyAttackTimer = 0;
    private bool prevEnemyIsAttacking = false;

    // ��� CharacterCore
    private CharacterCore enemyCore;

    // ���� ���� ���� ����
    private bool isFleeing = false;
    private float fleeTimer = 0f;
    private const float FLEE_DURATION = 3.0f;

    void Start()
    {/*
        // ���� ���� ��ġ ����
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
        // �������ڸ��� ����/���/������ �Ұ�
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

        // ��� ���� �ǽ� ���� �޾ƿ���
        enemyAttackTimer -= Time.deltaTime;
        if (enemyAttackTimer < 0f) enemyAttackTimer = 0f;

        if (!prevEnemyIsAttacking && enemyCore.isAttacking)
        {
            enemyAttackTimer += 2.5f;
        }
        prevEnemyIsAttacking = enemyCore.isAttacking;
        enemyIsAttacking = (enemyAttackTimer > 0f) ? true : false;

        // ���� ���� Ÿ�̸�
        if (isFleeing)
        {
            fleeTimer += Time.deltaTime;
            if (fleeTimer >= FLEE_DURATION)
            {
                isFleeing = false;
                fleeTimer = 0f;
            }
        }

        // ����
        HandleState();
    }

    // �Ÿ� ������
    public void KeepDistanceFromEnemy(float minDistance)
    {
        if (enemy == null || core == null)
            return;

        float distance = Vector3.Distance(transform.position, enemy.position);
        if (distance < minDistance)
        {
            // enemy �ݴ� �������� �̵�
            Vector3 awayDir = (transform.position - enemy.position).normalized;
            if (core.CanMove())
                core.HandleMovement(awayDir.x, awayDir.z);

            // NavMeshAgent ��� ��, ���� ��ġ���� minDistance��ŭ ������ �������� ������ ����
            if (agent != null)
            {
                Vector3 targetPos = transform.position + awayDir * (minDistance - distance + 0.5f);
                agent.SetDestination(targetPos);
            }
        }
        else
        {
            // �̹� ����� �ָ� ����
            core.HandleMovement(0, 0);
            if (agent != null)
                agent.ResetPath();
            return;
        }
    }

    void HandleState()
    {
        // �� �� �ϳ� ����� ����
        if (core.isDead || enemyCore.isDead)
            return;

        float distanceToEnemy = Vector3.Distance(transform.position, enemy.position);
        bool inRange = distanceToEnemy <= attackRange;

        if (enemy == null || enemyCore == null)
            return;

        // �̹� ����/���/ȸ�� ���̸� �ƹ��͵� ���� ����
        if (core.state == PlayerState.Attacking ||
            core.state == PlayerState.Defending ||
            core.state == PlayerState.Dodging)
        {
            if (agent != null)
                agent.ResetPath();
            return;
        }

        // ���� ���� ����
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

            // �����߿� ��� ����
            if (enemyIsAttacking)
            {
                // ��� ���� -> ��� �� ��� Ÿ�̸� ����
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
                    // Ÿ�̸� ���� ���� �ʿ�
                }

                // ��� �Ұ��� + ȸ�� ���� -> ȸ��
                else if (!core.CanDefence() && core.CanDodge())
                {
                    core.Dodge();
                    if (agent != null)
                        agent.ResetPath();
                }
            }
            return;
        }

        // ���¿� ���� �ൿ ����
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

                    // ����ġ�� ���� �ƴ�
                    if (!isFleeing)
                    {
                        // ���� ��
                        if (inRange)
                        {
                            // ��밡 ������
                            if (enemyIsAttacking)
                            {
                                // ��� �켱 Ȯ���� 55%�� ������ ���� ��ȸ �� (��� ���� �ÿ� ȸ��/���� ��ȸ�� ���)
                                if (core.CanDefence() && Random.value < 0.55f)
                                {
                                    LookAtEnemy();
                                    core.Defence();
                                    if (agent != null)
                                        agent?.ResetPath();
                                    Flee();
                                    return;
                                }
                                else if (core.CanDodge())
                                {
                                    core.Dodge();
                                    if (agent != null)
                                        agent?.ResetPath();
                                    return;
                                }
                                //// 1. ���� ��� ���� -> ��� �� ���� -> ���� �ڵ�
                                //if (core.CanDefence())
                                //{
                                //    //core.HandleMovement(0, 0);
                                //    LookAtEnemy();
                                //    core.Defence();
                                //    if (agent != null)
                                //        agent.ResetPath();
                                //    Flee();
                                //}

                                // 2. ���� ��� �Ұ��� + ���� ȸ�� ���� -> ȸ��
                                else if (!core.CanDefence() && core.CanDodge())
                                {
                                    core.Dodge();
                                    if (agent != null)
                                        agent.ResetPath();
                                }

                                // 3. ���� ���/ȸ�� �Ұ��� + ���� ���� ���� -> ���� �� ����
                                else if (!core.CanDefence() && !core.CanDodge() && core.CanAttack())
                                {
                                    //core.HandleMovement(0, 0);
                                    LookAtEnemy();
                                    core.Attack();
                                    if (agent != null)
                                        agent.ResetPath();
                                    Flee();
                                }

                                // 4. ���� ���� �Ұ��� -> ����
                                else
                                {
                                    if (agent != null)
                                        agent.ResetPath();
                                    Flee();
                                }
                            }

                            // ��밡 ������ �ƴ�
                            else
                            {
                                // ���� ����� �� �ݰ� ��ȸ (�ݰ� �õ� Ȯ�� 40%�� ����) (���� �� ���� �� ���/ȸ�� ��ȭ)
                                if (!enemyIsAttacking && core.CanAttack() && Random.value < 0.4f)
                                {
                                    LookAtEnemy();
                                    core.Attack();
                                    if (agent != null)
                                        agent?.ResetPath();
                                    Flee();
                                    return;
                                }
                                // ���� �Ұ� �� ���� �Ÿ� ����
                                if (!core.CanAttack())
                                {
                                    if (agent != null)
                                        agent?.ResetPath();
                                    KeepDistanceFromEnemy(attackRange + 0.5f);
                                }
                                // �� �� 15% Ȯ���� ȸ�� �߰� (��� �� ���� Ȯ���� ȸ�� -> ������ �϶�)
                                else if (core.CanDodge() && Random.value < 0.15f)
                                {
                                    core.Dodge();
                                    if (agent != null)
                                        agent?.ResetPath();
                                }

                                // ���� �ڵ� �ּ� ó��
                                //// 5. ���� ���� ���� -> ���� �� ����
                                //if (core.CanAttack())
                                //{
                                //    //core.HandleMovement(0, 0);
                                //    LookAtEnemy();
                                //    core.Attack();
                                //    if (agent != null)
                                //        agent.ResetPath();
                                //    Flee();
                                //}

                                //// 6. ���� ���� �Ұ� -> �Ÿ� ������
                                //else
                                //{
                                //    if (agent != null)
                                //        agent.ResetPath();
                                //    KeepDistanceFromEnemy(3.0f);
                                //}
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

    // ����
    void Flee()
    {
        isFleeing = true;
        fleeTimer = 0f;
    }

    // ����/���� ���� ���� ����
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

    // ����׿�
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