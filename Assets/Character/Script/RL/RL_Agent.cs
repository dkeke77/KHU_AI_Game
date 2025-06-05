using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class RL_Agent : Agent
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
                core.HandleMovement(xAxis,zAxis);
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
        // 사망처리
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
