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

    // ƽ ��� Ÿ�̸�
    int attackTimer = 0;
    int defenceTimer = 0;
    int dodgeTimer = 0;

    const int ATTACK_TIME_OUT = 50;
    const int DEFENCE_TIME_OUT = 75;
    const int DODGE_TIME_OUT = 75;

    // �ൿ ���� Ȯ�ο�
    int oldAttackSuc = 0;
    int oldDefenceSuc = 0;
    int oldDodgekSuc = 0;

    // �߰� ����
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
        // ��� ó��
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

        // ������ �Ÿ� ���
        float dist = Vector3.Distance(thisInfo.Position, enemyInfo.Position);

        // HP ��ȭ Ȯ�� (�ǰݿ��� Ȯ��)
        prevHP = thisInfo.CurrentHP;

        // �ð� ��� ���Ƽ
        AddReward(-0.001f); // �� �����Ӹ��� �ҷ� ���Ƽ �� ���� ������ ����
                            // ���� ���� 1: ��� ������ �� ��� ����
        if (core.CanDefence() && core.blockSucCounter > oldDefenceSuc)
        {
            AddReward(+3.0f);
        }

        // ���� ���� 2: ��� �Ұ��� �� ȸ�� �����ؼ� ȸ�� ����
        if (!core.CanDefence() && core.CanDodge() && core.dodgeSucCounter > oldDodgekSuc)
        {
            AddReward(+2.5f);
        }

        // ���� ���� 3: ���� ���� �� �Ÿ� ����
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

        // ���� 4: �ƹ� �͵� ���� �� ���� �Ÿ� ����
        if (!core.CanAttack() && !core.CanDefence() && !core.CanDodge() && dist > 3.0f)
        {
            AddReward(+1.0f);
        }

        // �г�Ƽ ���� 1: ���/ȸ�� �����ߴµ� ���� ����
        if ((core.CanDefence() || core.CanDodge()) && thisInfo.CurrentHP < prevHP)
        {
            AddReward(-1.0f);
        }

        // �Ÿ� ��� ����/���Ƽ

        if (dist > 10f)
            AddReward(-0.01f); // �ʹ� �־����� ���Ƽ
        else if (dist < 3f)
            AddReward(0.005f); // ������ �Ÿ����� �ణ ����

        var state = thisInfo.CurrentState;
        if (rb.linearVelocity.magnitude < 0.1f &&
            state != PlayerState.Attacking &&
            state != PlayerState.Defending &&
            state != PlayerState.Dodging)
        {
            AddReward(-0.002f); // ������ �����Ƿ� ���Ƽ
        }


        // ��� �� ȸ�� ���� �� �߰� ����
        if (defenceInProgress && oldDodgekSuc < core.dodgeSucCounter)
        {
            AddReward(1.5f); // ���->ȸ�� �����̸� �߰� ����
        }

        // �⺻ �ൿ ���� ��
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

    // ���� ��ȿ �Ǵ� (���߿� ������/�г�Ƽ)
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

    // ��� ��ȿ �Ǵ� (���߿� ������/�г�Ƽ)
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

    // ȸ�� ��ȿ �Ǵ� (���߿� ������/�г�Ƽ)
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
