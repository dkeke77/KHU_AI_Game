using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.AI;

/// <summary>
/// CharacterCore.cs 의 변수명/메서드명을 그대로 사용하여
/// “공격형 RL 에이전트” 구현.
/// </summary>
[RequireComponent(typeof(CharacterCore))]
[RequireComponent(typeof(NavMeshAgent))]
public class OffensiveRLAgent : Agent
{
    [Header("References (Drag & Drop)")]
    public CharacterCore core;          // 캐릭터 동작 제어
    public Transform enemyTransform;    // 상대 오브젝트 Transform (DefensiveAgent)
    public NavMeshAgent agent;          // NavMeshAgent
    private CharacterCore enemyCore;    // 상대 CharacterCore (Defensive_BT)

    [Header("Parameters")]
    public float maxMapExtent = 10f;    // 맵 절반 크기 (정규화 용)
    public float attackRange = 2f;      // 근접 공격 범위

    // 내부 상태 추적용
    private float lastEnemyHp;          // 지난 스텝 적 체력
    private float lastSelfHp;           // 지난 스텝 내 체력

    /// <summary>
    /// 학습 초기화: core, agent, enemyCore 자동 할당
    /// </summary>
    public override void Initialize()
    {
        if (core == null) core = GetComponent<CharacterCore>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (enemyTransform != null && enemyCore == null)
            enemyCore = enemyTransform.GetComponent<CharacterCore>();
    }

    /// <summary>
    /// 에피소드가 시작될 때마다 호출: 위치·체력·상태 리셋
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // 1) 나(공격형) 위치 리셋: x = [5 ~ 10], z = [-5 ~ 5]
        float randX = Random.Range(5f, 10f);
        float randZ = Random.Range(-5f, 5f);
        transform.position = new Vector3(randX, transform.position.y, randZ);
        agent.Warp(transform.position);
        agent.ResetPath();

        // 2) 상대(Defensive_BT) 위치 리셋: x = [-10 ~ -5], z = [-5 ~ 5]
        if (enemyTransform != null)
        {
            float eX = Random.Range(-10f, -5f);
            float eZ = Random.Range(-5f, 5f);
            enemyTransform.position = new Vector3(eX, enemyTransform.position.y, eZ);

            NavMeshAgent enemyAgent = enemyTransform.GetComponent<NavMeshAgent>();
            if (enemyAgent != null)
                enemyAgent.Warp(enemyTransform.position);
        }

        // 3) 체력 초기화
        core.cur_hp = CharacterCore.MAX_HP;
        if (enemyCore != null)
            enemyCore.cur_hp = CharacterCore.MAX_HP;

        // 4) 상태 및 타이머 초기화
        core.attackTimer  = 1.0f;
        core.defenceTimer = 1.0f;
        core.dodgeTimer   = 1.0f;
        core.state        = PlayerState.Idle;
        core.isAttacking  = false;
        core.isBlocking   = false;
        core.isDodging    = false;
        core.isDead       = false;

        if (enemyCore != null)
        {
            enemyCore.attackTimer  = 1.0f;
            enemyCore.defenceTimer = 1.0f;
            enemyCore.dodgeTimer   = 1.0f;
            enemyCore.state        = PlayerState.Idle;
            enemyCore.isAttacking  = false;
            enemyCore.isBlocking   = false;
            enemyCore.isDodging    = false;
            enemyCore.isDead       = false;
        }

        // 5) 체력 비교용 초기값 저장
        lastEnemyHp = (enemyCore != null) ? enemyCore.cur_hp : 0;
        lastSelfHp  = core.cur_hp;
    }

    /// <summary>
    /// CollectObservations: VectorSensor에 Self / Enemy / Env 정보 추가
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        if (enemyCore == null && enemyTransform != null)
            enemyCore = enemyTransform.GetComponent<CharacterCore>();

        // --- 1) Self State --- //
        sensor.AddObservation(core.cur_hp / (float)CharacterCore.MAX_HP);         // 1.1 체력 비율

        const float ATTACK_COOLDOWN_MAX  = 2.5f;
        const float DEFENCE_COOLDOWN_MAX = 2.5f;
        const float DODGE_COOLDOWN_MAX   = 5.0f;

        sensor.AddObservation(core.attackTimer  / ATTACK_COOLDOWN_MAX);           // 1.2 공격 타이머 정규화
        sensor.AddObservation(core.defenceTimer / DEFENCE_COOLDOWN_MAX);          // 1.2 방어 타이머 정규화
        sensor.AddObservation(core.dodgeTimer   / DODGE_COOLDOWN_MAX);            // 1.2 회피 타이머 정규화

        // 1.3 행동 상태(One-Hot, 총 6개: Idle, Moving, Attacking, Defending, Dodging, Dead)
        int enumCount = 6;
        int selfStateIdx = (int)core.state;
        for (int i = 0; i < enumCount; i++)
            sensor.AddObservation(i == selfStateIdx ? 1f : 0f);

        // 1.4 위치(x,z) 정규화 및 회전 Y 정규화
        sensor.AddObservation(transform.position.x / maxMapExtent);
        sensor.AddObservation(transform.position.z / maxMapExtent);
        sensor.AddObservation(transform.eulerAngles.y / 360f);

        // --- 2) Enemy State --- //
        if (enemyCore != null)
        {
            sensor.AddObservation(enemyCore.cur_hp / (float)CharacterCore.MAX_HP);  // 2.1 적 체력 비율
            sensor.AddObservation(enemyCore.attackTimer  / ATTACK_COOLDOWN_MAX);    // 2.2 적 타이머들
            sensor.AddObservation(enemyCore.defenceTimer / DEFENCE_COOLDOWN_MAX);
            sensor.AddObservation(enemyCore.dodgeTimer   / DODGE_COOLDOWN_MAX);

            int eStateIdx = (int)enemyCore.state;
            for (int i = 0; i < enumCount; i++)
                sensor.AddObservation(i == eStateIdx ? 1f : 0f);                   // 2.3 적 행동 상태(One-Hot)

            Vector3 delta = enemyTransform.position - transform.position;
            sensor.AddObservation(delta.x / maxMapExtent);                        // 2.4 상대적 위치(x)
            sensor.AddObservation(delta.z / maxMapExtent);                        // 2.4 상대적 위치(z)
        }
        else
        {
            // enemyCore가 null인 경우에는 모두 “0f” 혹은 “1f”로 채워서 빈 값 처리
            sensor.AddObservation(1f); // HP 비율
            sensor.AddObservation(1f); // attackTimer
            sensor.AddObservation(1f); // defenceTimer
            sensor.AddObservation(1f); // dodgeTimer
            for (int i = 0; i < enumCount; i++)
                sensor.AddObservation(0f); // 행동 상태 One-Hot 대신 모두 0
            sensor.AddObservation(1f); // delta.x
            sensor.AddObservation(1f); // delta.z
        }

        // --- 3) Environment 정보 --- //
        // 3.1 StepCount / 2000 (예상 최대 스텝 2000)
        sensor.AddObservation((float)StepCount / 2000f);
    }

    /// <summary>
    /// OnActionReceived: Discrete 액션(0~7)을 해석하고, 보상을 계산하여 AddReward
    /// </summary>
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (enemyCore == null && enemyTransform != null)
            enemyCore = enemyTransform.GetComponent<CharacterCore>();

        int action = actionBuffers.DiscreteActions[0];
        float reward = 0f;

        // 0~3: 이동, 4: 공격, 5: 방어, 6: 회피, 7: Idle
        Vector3 moveDir = Vector3.zero;
        if (action == 0) moveDir = transform.forward;      // 앞쪽
        else if (action == 1) moveDir = -transform.forward; // 뒤쪽
        else if (action == 2) moveDir = -transform.right;   // 왼쪽
        else if (action == 3) moveDir = transform.right;    // 오른쪽

        // --- 1) 이동(0~3) --- //
        if (action >= 0 && action <= 3)
        {
            Vector3 targetPos = transform.position + moveDir.normalized * core.speed * Time.fixedDeltaTime;
            agent.SetDestination(targetPos);

            // 거리 기반 보상(너무 멀면 +0.01, 너무 가까우면 -0.01)
            if (enemyTransform != null)
            {
                float dist = Vector3.Distance(transform.position, enemyTransform.position);
                if (dist > attackRange * 2f) reward += 0.01f;
                if (dist < attackRange * 0.5f) reward -= 0.01f;
            }
        }
        else
        {
            // 이동 외 액션(공격/방어/회피/Idle) 시, 경로 초기화
            agent.ResetPath();
        }

        // --- 2) 공격(4) --- //
        if (action == 4)
        {
            if (core.CanAttack())
            {
                // 적 바라보기
                if (enemyTransform != null)
                {
                    Vector3 dir = (enemyTransform.position - transform.position).normalized;
                    dir.y = 0f;
                    if (dir.sqrMagnitude > 0.001f)
                        transform.rotation = Quaternion.LookRotation(dir);
                }

                // 공격 직전 체력 저장
                float prevHp = (enemyCore != null) ? enemyCore.cur_hp : 0;
                core.Attack();

                // 공격 애니메이션이 끝나고 데미지가 적용된 뒤, OnTriggerEnter에서 cur_hp가 감소됨
                if (enemyCore != null)
                {
                    float postHp = enemyCore.cur_hp;
                    float dmg = prevHp - postHp;
                    if (dmg > 0f)
                    {
                        // 데미지 비율 ×0.1 보상
                        float frac = dmg / CharacterCore.MAX_HP;
                        reward += 0.1f * frac;
                    }
                    else
                    {
                        // 방어당했거나 회피당했을 때 패널티
                        reward -= 0.2f;
                    }
                }
            }
            else
            {
                // 쿨타임 중 공격 시도 → 소량 패널티
                reward -= 0.05f;
            }
        }

        // --- 3) 방어(5) --- //
        if (action == 5)
        {
            if (core.CanDefence())
            {
                // 적 바라보기
                if (enemyTransform != null)
                {
                    Vector3 dir = (enemyTransform.position - transform.position).normalized;
                    dir.y = 0f;
                    if (dir.sqrMagnitude > 0.001f)
                        transform.rotation = Quaternion.LookRotation(dir);
                }

                core.Defence();
                // 실제 블록 성공 여부는 OnTriggerEnter에서 처리 → 그 시점에 보상 없음
            }
            else
            {
                reward -= 0.05f;
            }
        }

        // --- 4) 회피(6) --- //
        if (action == 6)
        {
            if (core.CanDodge())
            {
                core.Dodge();
                // 실제 회피 성공 여부는 OnTriggerEnter에서 처리 → 그 시점에 보상 없음
            }
            else
            {
                reward -= 0.05f;
            }
        }

        // --- 5) Idle(7) --- //
        // 아무 행동도 하지 않음 → 추가 보상/페널티 없음

        // --- 6) 스텝 페널티 --- //
        reward += -0.001f;

        // --- 7) 방어/회피 성공 보상 --- //
        if (enemyCore != null && enemyCore.isAttacking && core.isBlocking)
        {
            // 상대가 공격 중이고 내가 블록 중이면 +0.15
            reward += 0.15f;
        }
        if (enemyCore != null && enemyCore.isAttacking && core.isDodging)
        {
            // 상대가 공격 중이고 내가 회피 중이면 +0.10
            reward += 0.10f;
        }

        // --- 8) 피해 입었을 때 패널티 --- //
        float curSelfHp = core.cur_hp;
        if (curSelfHp < lastSelfHp)
        {
            float dmgTaken = lastSelfHp - curSelfHp;
            float frac = dmgTaken / CharacterCore.MAX_HP;
            reward -= 0.1f * frac;
        }
        lastSelfHp = curSelfHp;

        // --- 9) 적 피해 누적 보상 --- //
        float curEnemyHp = (enemyCore != null) ? enemyCore.cur_hp : 0;
        if (curEnemyHp < lastEnemyHp)
        {
            float dmgDone = lastEnemyHp - curEnemyHp;
            float frac = dmgDone / CharacterCore.MAX_HP;
            reward += 0.1f * frac;
        }
        lastEnemyHp = curEnemyHp;

        // --- 10) 종료 조건(승/패 & 범위 벗어남) --- //
        // 10-1. 내 체력 0 → 패배
        if (core.cur_hp <= 0)
        {
            reward -= 5.0f;
            AddReward(reward);
            EndEpisode();
            return;
        }
        // 10-2. 적 체력 0 → 승리
        if (enemyCore != null && enemyCore.cur_hp <= 0)
        {
            reward += 5.0f;
            AddReward(reward);
            EndEpisode();
            return;
        }
        // 10-3. 맵 범위 벗어남 → 패널티
        Vector3 pos = transform.position;
        if (Mathf.Abs(pos.x) > maxMapExtent || Mathf.Abs(pos.z) > maxMapExtent)
        {
            reward -= 2.0f;
            AddReward(reward);
            EndEpisode();
            return;
        }

        // --- 11) 최종 보상 반영 --- //
        AddReward(reward);
    }

    /// <summary>
    /// Heuristic: 디버깅용 키보드 입력 매핑
    /// </summary>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W))            discreteActionsOut[0] = 0; // Move Up
        else if (Input.GetKey(KeyCode.S))       discreteActionsOut[0] = 1; // Move Down
        else if (Input.GetKey(KeyCode.A))       discreteActionsOut[0] = 2; // Move Left
        else if (Input.GetKey(KeyCode.D))       discreteActionsOut[0] = 3; // Move Right
        else if (Input.GetKeyDown(KeyCode.J))   discreteActionsOut[0] = 4; // Attack
        else if (Input.GetKeyDown(KeyCode.K))   discreteActionsOut[0] = 5; // Defence
        else if (Input.GetKeyDown(KeyCode.L))   discreteActionsOut[0] = 6; // Dodge
        else                                    discreteActionsOut[0] = 7; // Idle
    }
}
