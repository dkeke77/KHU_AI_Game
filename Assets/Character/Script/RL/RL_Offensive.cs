using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class RL_Offensive : Agent
{
    // Game objects
    Rigidbody rb;
    CharacterCore core;
    CharacterInfo thisInfo;
    public Transform enemy;
    CharacterCore enemyCore;
    CharacterInfo enemyInfo;

    CSVWriter csvWriter;
    public bool activateWriteCSV = false;

    // Enemy hit
    float oldEnemyHP;

    bool attackInProgress = false;
    bool defenceInProgress = false;
    bool dodgeInProgress = false;

    bool attacked = false;

    // ƽ ��� Ÿ�̸�
    int attackTimer = 0;
    int defenceTimer = 0;
    int dodgeTimer = 0;    
    int boundaryTimer = 0;    
    
    const int ATTACK_TIME_OUT = 50;
    const int DEFENCE_TIME_OUT = 75;
    const int DODGE_TIME_OUT = 75;

    // �ൿ ���� Ȯ�ο�
    int oldAttackSuc = 0;
    int oldDefenceSuc = 0;
    int oldDodgekSuc = 0;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        
        core = GetComponent<CharacterCore>();
        thisInfo = GetComponent<CharacterInfo>();
        csvWriter = GetComponentInParent<CSVWriter>();

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

    void ExecuteEpisodeEnd()
    {
        string winner = "";
        if (core.isDead)
            winner = enemy.name;
        else if (enemyCore.isDead)
            winner = this.name;
        csvWriter.WriteRLCombatData(winner);
        EndEpisode();
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
                        Debug.Log(CalcAngle());

                        attackInProgress = true;
                        attacked = true;
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
        // ���ó��
        if (thisInfo.IsDead)
        {
            AddReward(-3.0f);
            ExecuteEpisodeEnd();
            return;
        }
        else if (enemyInfo.IsDead)
        {
            AddReward(5.0f);
            ExecuteEpisodeEnd();
            return;
        }

        // �⺻ �н�
        // ���� �ʹ� �����ʰ�
        if (Vector3.Distance(thisInfo.Position, enemyInfo.Position) < 0.8f)
        {
            AddReward(-0.01f);
        }
        // ��迡 �ʹ� ���� ���� ��
        if (core.isInBoundary())
        {
            boundaryTimer++;

            if (boundaryTimer > 100)
            {
                AddReward(-0.001f);
            }
            else if (boundaryTimer > 10)
                AddReward(-0.0005f);
        }
        else
            boundaryTimer = 0;

        // Ŀ��ŧ��
        if (StepCount < 600000)
        {
            curriculumLevel0();
            if (attackInProgress)
                EvaluateAttackReward(fRwd:1.2f,fPnlt:0.5f);
            if (defenceInProgress)
                EvaluateDenfenceReward(fRwd: 1.2f, fPnlt: 0.8f);
            if (dodgeInProgress)
                EvaluateDodgeReward(fRwd: 1.2f, fPnlt: 0.8f);
        }
        else if (StepCount < 1500000)
        {
            curriculumLevel1();
            if (attackInProgress)
                EvaluateAttackReward(fRwd: 1.2f, fPnlt: 1.0f);
            if (defenceInProgress)
                EvaluateDenfenceReward(fRwd: 1.2f, fPnlt: 1.0f);
            if (dodgeInProgress)
                EvaluateDodgeReward(fRwd: 1.2f, fPnlt: 1.0f);
        }
        else
        {
            curriculumLevel2();
            if (attackInProgress)
                EvaluateAttackReward(fRwd: 1.0f, fPnlt: 1.2f);
            if (defenceInProgress)
                EvaluateDenfenceReward(fRwd: 1.0f, fPnlt: 1.2f);
            if (dodgeInProgress)
                EvaluateDodgeReward(fRwd: 1.0f, fPnlt: 1.2f);
        }

        oldAttackSuc = core.attackSucCounter;
        oldDefenceSuc = core.blockSucCounter;
        oldDodgekSuc = core.dodgeSucCounter;

        attacked = false;
    }

    void curriculumLevel0()
    {
        // �Ÿ� ������ ������
        if (Vector3.Distance(thisInfo.Position, enemyInfo.Position) < 5.0f)
        {
            AddReward(0.0005f);
        }
        // ���ݹ��� ������
        if (attacked && CalcAngle() > 0.6f)
        {
            AddReward(0.5f);
        }
        if (!core.isInBoundary())
        {
            AddReward(0.000001f);
        }
    }

    void curriculumLevel1()
    {
        // �Ÿ� ������ ������
        if (Vector3.Distance(thisInfo.Position, enemyInfo.Position) < 4.0f)
        {
            AddReward(0.00001f);
        }
        //���� ���� ó��
        if (attacked)
        {
            // ���ݹ��� �г�Ƽ
            if (CalcAngle() < 0.6f)
                AddReward(-0.1f);

            // �� ���/ȸ�� ��Ÿ�� �� ���� �� ������
            if (enemyCore.defenceTimer > 0)
                AddReward(0.3f);
            if (enemyCore.dodgeTimer > 0)
                AddReward(0.3f);
        }
    }

    void curriculumLevel2()
    {
        // �Ÿ� �ָ� �г�Ƽ
        if (Vector3.Distance(thisInfo.Position, enemyInfo.Position) > 6.0f)
        {
            AddReward(-0.00005f);
        }
        //���� ���� ó��
        if (attacked)
        {
            // ���ݹ��� �г�Ƽ
            if (CalcAngle() < 0.6f)
                AddReward(-0.5f);

            // �� ���/ȸ�� ��Ÿ�� �� ���� �� ������
            if (enemyCore.defenceTimer > 0)
                AddReward(0.25f);
            if (enemyCore.dodgeTimer > 0)
                AddReward(0.25f);
        }
    }

    // ���� ��ȿ �Ǵ� (���߿� ������/�г�Ƽ)
    void EvaluateAttackReward(float fRwd = 1.0f, float fPnlt = 1.0f)
    {
        attackTimer++;
        if (oldAttackSuc < core.attackSucCounter)
        {
            AddReward(3.0f * fRwd);
            attackInProgress = false;
            attackTimer = 0;
        }
        else if (attackTimer >= ATTACK_TIME_OUT)
        {
            AddReward(-1.0f * fPnlt);
            attackInProgress = false;
            attackTimer = 0;
        }
    }

    // ��� ��ȿ �Ǵ� (���߿� ������/�г�Ƽ)
    void EvaluateDenfenceReward(float fRwd = 1.0f, float fPnlt = 1.0f)
    {
        defenceTimer++;
        if (oldDefenceSuc < core.blockSucCounter)
        {
            AddReward(3.0f * fRwd);
            defenceInProgress = false;
            defenceTimer = 0;
        }
        else if (defenceTimer >= DEFENCE_TIME_OUT)
        {
            AddReward(-1.0f * fPnlt);
            defenceInProgress = false;
            defenceTimer = 0;
        }
    }

    // ȸ�� ��ȿ �Ǵ� (���߿� ������/�г�Ƽ)
    void EvaluateDodgeReward(float fRwd=1.0f, float fPnlt = 1.0f)
    {
        dodgeTimer++;
        if (oldDodgekSuc < core.dodgeSucCounter)
        {
            AddReward(3.0f * fRwd);
            dodgeInProgress = false;
            dodgeTimer = 0;
        }
        else if (dodgeTimer >= DODGE_TIME_OUT)
        {
            AddReward(-1.0f * fPnlt);
            dodgeInProgress = false;
            dodgeTimer = 0;
        }
    }

    float CalcAngle()
    {
        Vector3 toEnemy = enemyInfo.Position - thisInfo.Position;
        return Vector3.Dot(toEnemy.normalized, thisInfo.Forward);
    }
}
