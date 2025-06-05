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

    // ��� ���� ���� ���� ���� ����
    public bool enemyIsBlocking = false;
    public float enemyDefenceTimer = 0;
    private bool prevEnemyIsBlocking = false;

    // ��� CharacterCore
    private CharacterCore enemyCore;

    // ���� ���� ���� ����
    private bool isFleeing = false;
    private float fleeTimer = 0f;
    private const float FLEE_DURATION = 1.0f;

    void Start()
    {
        // ���� ���� ��ġ ����
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

        // �������ڸ��� ����/���/������ �Ұ�
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

        // ��� ���� �ǽ� ���� �޾ƿ���
        enemyDefenceTimer -= Time.deltaTime;
        if (enemyDefenceTimer < 0f) enemyDefenceTimer = 0f;

        if (!prevEnemyIsBlocking && enemyCore.isBlocking)
        {
            enemyDefenceTimer += 2.5f;
        }
        prevEnemyIsBlocking = enemyCore.isBlocking;
        enemyIsBlocking = (enemyDefenceTimer > 0f) ? true : false;

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

    void HandleState()
    {
        // �� �� �ϳ� ����� ����
        if (core.isDead || enemyCore.isDead)
            return;

        float distanceToEnemy = Vector3.Distance(transform.position, enemy.position);
        bool inRange = distanceToEnemy <= attackRange;

        if (enemy == null || enemyCore == null)
            return;

        // �� �� �ϳ� ����� ����
        if (core.state == PlayerState.Dead || enemyCore.state == PlayerState.Dead)
            return;

        // ���� ���� ����
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

        // �̹� ����/���/ȸ�� ���̸� �ƹ��͵� ���� ����
        if (core.state == PlayerState.Attacking ||
            core.state == PlayerState.Defending ||
            core.state == PlayerState.Dodging)
        {
            agent.ResetPath();
            return;
        }

        // ���¿� ���� �ൿ ����
        switch (core.state)
        {
            case PlayerState.Idle:
            case PlayerState.Moving:
                Vector3 dir = (enemy.position - transform.position).normalized;
                core.HandleMovement(dir.x, dir.z);
                if (enemy != null)
                {
                    // 1. ���� �� -> ����
                    if (!inRange && !isFleeing)
                        agent.SetDestination(enemy.position);
                    else
                        core.HandleMovement(0, 0);

                    // ����ġ�� �� �ƴ�
                    if (!isFleeing)
                    {
                        // ���� ��
                        if(inRange)
                        {
                            // ��밡 ����� �ƴ�
                            if (!enemyIsBlocking)
                            {
                                // 2. ���� ���� -> ���� �� ����
                                if (core.CanAttack())
                                {
                                    core.HandleMovement(0, 0);
                                    LookAtEnemy();
                                    core.Attack();
                                    agent.ResetPath();
                                    Flee();
                                }
                                // 3. ���� �Ұ� + ȸ�� ���� -> ȸ��
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

    // ����
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
