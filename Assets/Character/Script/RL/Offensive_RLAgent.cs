using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class Offensive_RLAgent : Agent
{
    public CharacterCore core;
    public Transform enemy;

    private CharacterCore enemyCore;

    public float attackRange = 2f;

    private float lastDistanceToEnemy = Mathf.Infinity;

    public override void Initialize()
    {
        if (core == null)
            core = GetComponent<CharacterCore>();

        // enemyCore 연결
        if (enemy != null && enemyCore == null)
        {
            enemyCore = enemy.GetComponent<CharacterCore>();
        }
    }

    public override void OnEpisodeBegin()
    {
        // 캐릭터 초기화
        ResetCharacter(core);
        ResetCharacter(enemyCore);

        // 랜덤 위치
        transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0f, Random.Range(-4f, 4f));
        if (enemy != null)
            enemy.localPosition = new Vector3(Random.Range(-4f, 4f), 0f, Random.Range(-4f, 4f));

        // 거리 초기화
        lastDistanceToEnemy = Vector3.Distance(transform.position, enemy.position);
    }

    private void ResetCharacter(CharacterCore ch)
    {
        if (ch == null) return;

        ch.cur_hp = CharacterCore.MAX_HP;
        ch.attackTimer = 0f;
        ch.defenceTimer = 0f;
        ch.dodgeTimer = 0f;
        ch.isDead = false;
        ch.state = PlayerState.Idle;

        Collider col = ch.GetComponent<Collider>();
        if (col != null) col.enabled = true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (core == null || enemy == null || enemyCore == null)
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
            return;
        }

        // 내 상태
        sensor.AddObservation(transform.localPosition / 10f);  // 정규화
        sensor.AddObservation(core.cur_hp / 100f);
        sensor.AddObservation(core.attackTimer);
        sensor.AddObservation(core.defenceTimer);
        sensor.AddObservation(core.dodgeTimer);

        // 적 상태
        sensor.AddObservation(enemy.localPosition / 10f); // 정규화
        sensor.AddObservation(enemyCore.cur_hp / 100f);
        sensor.AddObservation(enemyCore.isBlocking ? 1f : 0f);

        float dist = Vector3.Distance(transform.position, enemy.position);
        sensor.AddObservation(dist / 10f);

        Vector3 directionToEnemy = (enemy.position - transform.position).normalized;
        sensor.AddObservation(directionToEnemy);  // (x, y, z)
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (core == null || enemy == null || enemyCore == null)
            return;

        // 죽음 체크 먼저 처리
        if (core.isDead)
        {
            SetReward(-1f); // 내가 죽으면 -1점
            EndEpisode();
            return;
        }

        if (enemyCore.isDead)
        {
            SetReward(+1f); // 적이 죽으면 +1점
            EndEpisode();
            return;
        }

        int action = actions.DiscreteActions[0];

        // 거리 보상
        float prevDistance = lastDistanceToEnemy;
        float currentDistance = Vector3.Distance(transform.position, enemy.position);

        if (currentDistance < prevDistance)
            AddReward(+0.02f);
        else
            AddReward(-0.01f);

        lastDistanceToEnemy = currentDistance;

        // 행동 처리
        switch (action)
        {
            case 0: core.HandleMovement(0, 1); break;  // forward
            case 1: core.HandleMovement(0, -1); break; // back
            case 2: core.HandleMovement(-1, 0); break; // left
            case 3: core.HandleMovement(1, 0); break;  // right

            case 4: // 공격
                if (core.CanAttack() && currentDistance <= attackRange)
                {
                    core.Attack();

                    if (!enemyCore.isBlocking)
                    {
                        AddReward(+2.0f);  // 성공 공격
                    }
                    else
                    {
                        AddReward(-1.0f);  // 방어당함
                    }
                }
                else
                {
                    AddReward(-0.2f); // 범위 밖 or 불필요한 공격
                }
                break;

            case 5: // 회피
                if (core.CanDodge())
                {
                    core.Dodge();
                    AddReward(-0.05f); // 회피 자제
                }
                break;

            case 6: // 방어
                if (core.CanDefence())
                {
                    core.Defence();
                    AddReward(-0.05f); // 수동적 행동 감점
                }
                break;
        }

        // 소극적일 경우 경미한 감점
        AddReward(-0.001f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var d = actionsOut.DiscreteActions;
        d[0] = 0; // 앞으로 이동 (디버깅용)
    }
}
