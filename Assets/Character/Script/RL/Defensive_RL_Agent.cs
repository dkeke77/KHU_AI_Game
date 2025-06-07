using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;

public class Defensive_RL_Agent : Agent
{
    // Game objects
    Rigidbody rb;
    CharacterCore core;
    CharacterInfo thisInfo;
    public Transform enemy;
    CharacterCore enemyCore;
    CharacterInfo enemyInfo;

    // Enemy hit
    float oldEnemyHP;

    bool attackInProgress = false;
    bool defenceInProgress = false;
    bool dodgeInProgress = false;

    // 틱 기반 타이머
    int attackTimer = 0;
    int defenceTimer = 0;
    int dodgeTimer = 0;

    const int ATTACK_TIME_OUT = 50;
    const int DEFENCE_TIME_OUT = 75;
    const int DODGE_TIME_OUT = 75;

    // 행동 성공 확인용
    int oldAttackSuc = 0;
    int oldDefenceSuc = 0;
    int oldDodgekSuc = 0;

    // 추가 변수
    float lastAttackSuccessTime = -100f;
    bool rewardAfterAttackDistanceGiven = false;
    int prevHP = 0;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();

        core = GetComponent<CharacterCore>();
        thisInfo = GetComponent<CharacterInfo>();

        if (enemy == null)
        {
            GameObject[] enemyObjs = GameObject.FindGameObjectsWithTag("Character");
            foreach (var obj in enemyObjs)
            {
                if (obj != this.gameObject)
                {
                    enemyCore = obj.GetComponent<CharacterCore>();
                    enemyInfo = obj.GetComponent<CharacterInfo>();
                }
            }
        }
        else
        {
            enemyCore = enemy.GetComponent<CharacterCore>();
            enemyInfo = enemy.GetComponent<CharacterInfo>();
        }

        prevHP = 100;
    }

    public override void OnEpisodeBegin()
    {
        attackTimer = 0;
        defenceTimer = 0;
        dodgeTimer = 0;

        oldAttackSuc = 0;
        oldDefenceSuc = 0;
        oldDodgekSuc = 0;

        core.Spawn();
        enemyCore.Spawn();

        Debug.Log("New Episode Begins");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(thisInfo.Position.x);
        sensor.AddObservation(thisInfo.Position.z);
        sensor.AddObservation(thisInfo.Forward.x);
        sensor.AddObservation(thisInfo.Forward.z);
        sensor.AddObservation(thisInfo.CurrentHP / 100);
        sensor.AddObservation(thisInfo.AttackTimer / 2.5f);
        sensor.AddObservation(thisInfo.DefenceTimer / 2.5f);
        sensor.AddObservation(thisInfo.DodgeTimer / 5f);
        sensor.AddObservation((int)thisInfo.CurrentState);

        sensor.AddObservation(enemyInfo.Position.x);
        sensor.AddObservation(enemyInfo.Position.z);
        sensor.AddObservation(enemyInfo.Forward.x);
        sensor.AddObservation(enemyInfo.Forward.z);
        sensor.AddObservation(enemyInfo.CurrentHP / 100);
        sensor.AddObservation(enemyInfo.AttackTimer / 2.5f);
        sensor.AddObservation(enemyInfo.DefenceTimer / 5f);
        sensor.AddObservation(enemyInfo.DodgeTimer);
        sensor.AddObservation((int)enemyInfo.CurrentState);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int disAction = actions.DiscreteActions[0];
        float xAxis = actions.ContinuousActions[0];
        float zAxis = actions.ContinuousActions[1];

        if (disAction == 0)
        {
            if (core.CanMove())
                core.HandleMovement(xAxis, zAxis);
        }
        else
        {
            switch (disAction)
            {
                case 1:
                    if (core.CanAttack())
                    {
                        core.Attack();
                        attackInProgress = true;
                    }
                    break;
                case 2:
                    if (core.CanDefence())
                    {
                        core.Defence();
                        defenceInProgress = true;
                    }
                    break;
                case 3:
                    if (core.CanDodge())
                    {
                        core.Dodge();
                        dodgeInProgress = true;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    void FixedUpdate()
    {
        // 사망 처리
        if (thisInfo.IsDead)
        {
            AddReward(-2.0f);
            EndEpisode();
            return;
        }
        else if (enemyInfo.IsDead)
        {
            AddReward(3.0f);
            EndEpisode();
            return;
        }

        // 적과의 거리 계산
        float dist = Vector3.Distance(thisInfo.Position, enemyInfo.Position);

        // HP 변화 확인 (피격여부 확인)
        prevHP = thisInfo.CurrentHP;

        // 시간 경과 페널티
        AddReward(-0.001f); // 매 프레임마다 소량 페널티 → 빨리 결정을 유도
                            // 보상 조건 1: 방어 가능할 때 방어 성공
        if (core.CanDefence() && core.blockSucCounter > oldDefenceSuc)
        {
            AddReward(+3.0f);
        }

        // 보상 조건 2: 방어 불가할 때 회피 가능해서 회피 성공
        if (!core.CanDefence() && core.CanDodge() && core.dodgeSucCounter > oldDodgekSuc)
        {
            AddReward(+2.5f);
        }

        // 보상 조건 3: 공격 성공 후 거리 벌림
        if (core.attackSucCounter > oldAttackSuc)
        {
            lastAttackSuccessTime = Time.time;
            rewardAfterAttackDistanceGiven = false;
        }
        if (Time.time - lastAttackSuccessTime <= 2.0f && !rewardAfterAttackDistanceGiven && dist > 4.0f)
        {
            AddReward(+2.0f);
            rewardAfterAttackDistanceGiven = true;
        }

        // 조건 4: 아무 것도 못할 때 일정 거리 유지
        if (!core.CanAttack() && !core.CanDefence() && !core.CanDodge() && dist > 3.0f)
        {
            AddReward(+1.0f);
        }

        // 패널티 조건 1: 방어/회피 가능했는데 공격 맞음
        if ((core.CanDefence() || core.CanDodge()) && thisInfo.CurrentHP < prevHP)
        {
            AddReward(-1.0f);
        }

        // 거리 기반 보상/페널티

        if (dist > 10f)
            AddReward(-0.01f); // 너무 멀어지면 페널티
        else if (dist < 3f)
            AddReward(0.005f); // 적절한 거리에선 약간 보상

        var state = thisInfo.CurrentState;
        if (rb.linearVelocity.magnitude < 0.1f &&
            state != PlayerState.Attacking &&
            state != PlayerState.Defending &&
            state != PlayerState.Dodging)
        {
            AddReward(-0.002f); // 가만히 있으므로 페널티
        }


        // 방어 중 회피 성공 시 추가 보상
        if (defenceInProgress && oldDodgekSuc < core.dodgeSucCounter)
        {
            AddReward(1.5f); // 방어->회피 성공이면 추가 보상
        }

        // 기본 행동 성공 평가
        if (attackInProgress)
            EvaluateAttackReward();
        if (defenceInProgress)
            EvaluateDenfenceReward();
        if (dodgeInProgress)
            EvaluateDodgeReward();

        oldAttackSuc = core.attackSucCounter;
        oldDefenceSuc = core.blockSucCounter;
        oldDodgekSuc = core.dodgeSucCounter;
    }

    // 공격 유효 판단 (나중에 리워드/패널티)
    void EvaluateAttackReward()
    {
        attackTimer++;
        if (oldAttackSuc < core.attackSucCounter)
        {
            AddReward(3.0f);
            attackInProgress = false;
            attackTimer = 0;
        }
        else if (attackTimer >= ATTACK_TIME_OUT)
        {
            AddReward(-1.0f);
            attackInProgress = false;
            attackTimer = 0;
        }
    }

    // 방어 유효 판단 (나중에 리워드/패널티)
    void EvaluateDenfenceReward()
    {
        defenceTimer++;
        if (oldDefenceSuc < core.blockSucCounter)
        {
            AddReward(3.0f);
            defenceInProgress = false;
            defenceTimer = 0;
        }
        else if (defenceTimer >= DEFENCE_TIME_OUT)
        {
            AddReward(-1.0f);
            defenceInProgress = false;
            defenceTimer = 0;
        }
    }

    // 회피 유효 판단 (나중에 리워드/패널티)
    void EvaluateDodgeReward()
    {
        dodgeTimer++;
        if (oldDodgekSuc < core.dodgeSucCounter)
        {
            AddReward(3.0f);
            dodgeInProgress = false;
            dodgeTimer = 0;
        }
        else if (dodgeTimer >= DODGE_TIME_OUT)
        {
            AddReward(-1.0f);
            dodgeInProgress = false;
            dodgeTimer = 0;
        }
    }
}
