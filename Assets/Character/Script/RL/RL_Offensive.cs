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

    int curriculumLevel = 0;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        
        core = GetComponent<CharacterCore>();
        thisInfo = GetComponent<CharacterInfo>();
        csvWriter = GetComponentInParent<CSVWriter>();
        
        // ���� �����ؾ��ҵ�...
        curriculumLevel = 5;
        Debug.Log("Now Currinum Level : " + curriculumLevel);

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
        csvWriter.WriteRLCombatDataByRL(winner,GetCumulativeReward());
        EndEpisode();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        switch (curriculumLevel)
        {
            case 0:
                AddObservationWithMasking(sensor, position_enemy:true ,look: true, hp: true, cooltime: true); ;
                break;
            case 1:
                AddObservationWithMasking(sensor, look: true, hp: true, cooltime: true); ;
                break;
            case 2:
                AddObservationWithMasking(sensor, cooltime: true); ;
                break;
            case 3:
            case 4:
            case 5:
                AddObservationWithMasking(sensor); ;
                break;
        }
    }

    void AddObservationWithMasking(VectorSensor sensor, bool position_me = false, bool position_enemy=false, bool look=false, bool hp = false, bool cooltime = false)
    {
        sensor.AddObservation(core.floor.position.x);
        sensor.AddObservation(core.floor.position.y);
        
        sensor.AddObservation(position_me ? 0 : thisInfo.NormalizedPosition().x);
        sensor.AddObservation(position_me ? 0 : thisInfo.NormalizedPosition().z);
        sensor.AddObservation(look ? 1 : thisInfo.Forward.x);
        sensor.AddObservation(look ? 0 : thisInfo.Forward.z);
        sensor.AddObservation(hp ? 1 : thisInfo.CurrentHP / 100);
        sensor.AddObservation(cooltime ? 0 : thisInfo.AttackTimer / 2.5f);
        sensor.AddObservation(cooltime ? 0 : thisInfo.DefenceTimer / 2.5f);
        sensor.AddObservation(cooltime ? 0 : thisInfo.DodgeTimer / 5f);

        sensor.AddObservation(position_enemy ? 0 : DistToEnemy() / thisInfo.GetMaxDistOfFloor());
        sensor.AddObservation(position_enemy ? 0 : AngleToEnemy());
        sensor.AddObservation(look ? 1 : enemyInfo.Forward.x);
        sensor.AddObservation(look ? 0 : enemyInfo.Forward.z);
        sensor.AddObservation(hp ? 1 : enemyInfo.CurrentHP / 100);
        sensor.AddObservation(cooltime ? 0 : enemyInfo.AttackTimer / 2.5f);
        sensor.AddObservation(cooltime ? 0 : enemyInfo.DefenceTimer / 2.5f);
        sensor.AddObservation(cooltime ? 0 : enemyInfo.DodgeTimer / 5f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int disAction = actions.DiscreteActions[0];
        float rotation = actions.ContinuousActions[0];
        rotation = rotation * 360f * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Sin(rotation), 0f, Mathf.Cos(rotation));

        if (curriculumLevel == 0 || curriculumLevel == 1)
            core.HandleMovement(dir.x, dir.z);
        else
        {
            if (disAction == 1)
            {
                if (core.CanMove())
                    core.HandleMovement(dir.x, dir.z);
            }
            else
            {
                switch (disAction)
                {
                    case 2:
                        if (core.CanAttack())
                        {
                            core.Attack();

                            attackInProgress = true;
                            attacked = true;
                        }
                        break;
                    case 3:
                        if (core.CanDefence())
                        {
                            core.Defence();
                            defenceInProgress = true;
                        }
                        break;
                    case 4:
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
    }

    void FixedUpdate()
    {
        /*
        // Ŀ��ŧ�� ������Ʈ - Legacy
        if (StepCount < 700000)
            curriculumLevel = 0;
        else if (StepCount < 1500000)
            curriculumLevel = 1;
        else if (StepCount < 2200000)
            curriculumLevel = 2;
        else if (StepCount < 2700000)
            curriculumLevel = 3;
        else if (StepCount < 3500000)
            curriculumLevel = 4;
        else
            curriculumLevel = 5;
        */

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
        
        // ��迡 �ִ� �ð� ����
        if (core.isInBoundary())
            boundaryTimer++;
        else
            boundaryTimer = 0;

        // Ŀ��ŧ��
        switch (curriculumLevel)
        {
            case 0:
                CurriculumLevel0();
                break;
            case 1:
                CurriculumLevel1();
                break;
            case 2:
                CurriculumLevel2();
                break;
            case 3:
                CurriculumLevel3();
                break;
            case 4:
                CurriculumLevel4();
                break;
            case 5:
                CurriculumLevel5();
                break;
        }

        oldAttackSuc = core.attackSucCounter;
        oldDefenceSuc = core.blockSucCounter;
        oldDodgekSuc = core.dodgeSucCounter;

        attacked = false;
    }

    void CurriculumLevel0()
    {
        // �ٿ���� ȸ��
        if (thisInfo.DistanceToBoundary() < 1.5f)
        {
            float factor = (1.5f - thisInfo.DistanceToBoundary()) / 1.5f;
            AddReward(-0.01f * factor);
        }

        // �߾� ��ȣ
        if (Vector3.Distance(thisInfo.Position, core.floor.position) < 4.0f)
        {
            float factor = (4 - Vector3.Distance(thisInfo.Position, core.floor.position)) / 4;
            
            AddReward(0.05f * factor);
        }
    }

    void CurriculumLevel1()
    {
        // �ٿ���� ȸ��
        if (thisInfo.DistanceToBoundary() < 1.5f)
        {
            float factor = (1.5f - thisInfo.DistanceToBoundary()) / 1.5f;
            AddReward(-0.015f * factor);
        }

        // �߾ӿ��� �־������� �г�Ƽ
        if (Vector3.Distance(thisInfo.Position, core.floor.position) > 4.0f)
        {
            float factor = (Vector3.Distance(thisInfo.Position, core.floor.position) - 4) / 2;
            AddReward(-0.02f * factor);
        }

        // ������ �Ÿ��� ���� ������
        if (DistToEnemy() < 0.8f)
        {
            AddReward(-0.01f);
        }
        else if (DistToEnemy() < 4.0f)
        {
            float factor = (4 - DistToEnemy()) / 4;
            AddReward(0.01f * factor);
        }
    }

    void CurriculumLevel2()
    {
        if (attacked)
        {
            // ���ݹ��� ������ (������ �������� �� ���� ������)
            if (AngleToEnemy() > 0.6f)
                AddReward(0.5f * AngleToEnemy());
            else
                AddReward(-0.1f);
        }
    }

    void CurriculumLevel3()
    {
        //���� ���� ó��
        if (attacked)
        {
            // ���ݹ��� �г�Ƽ
            if (AngleToEnemy() > 0.6f)
            {
                AddReward(0.1f * AngleToEnemy());

                // �� ���/ȸ�� ��Ÿ�� �� ���� �� ������
                if (enemyCore.defenceTimer > 0)
                    AddReward(0.3f);
                if (enemyCore.dodgeTimer > 0)
                    AddReward(0.3f);
            }
            else
                AddReward(-0.2f);
        }

        if (attackInProgress)
            EvaluateAttackReward(fRwd: 1.5f, fPnlt: 0.3f);
    }

    void CurriculumLevel4()
    {
        // �ٿ���� ȸ��
        if (thisInfo.DistanceToBoundary() < 1.5f)
        {
            float factor = (1.5f - thisInfo.DistanceToBoundary()) / 1.5f;
            AddReward(-0.02f * factor);
        }

        // ������ �Ÿ��� ���� ������
        if (DistToEnemy() < 0.8f)
        {
            AddReward(-0.01f);
        }
        else if (DistToEnemy() < 4.0f)
        {
            float factor = (4 - DistToEnemy()) / 4;
            AddReward(0.01f * factor);
        }

        //���� ���� ó��
        if (attacked)
        {
            // ���ݹ��� �г�Ƽ
            if (AngleToEnemy() > 0.6f)
            {
                AddReward(0.1f * AngleToEnemy());
                // �� ���/ȸ�� ��Ÿ�� �� ���� �� ������
                if (enemyCore.defenceTimer > 0)
                    AddReward(0.3f);
                if (enemyCore.dodgeTimer > 0)
                    AddReward(0.3f);
            }
            else
                AddReward(-0.2f);
        }

        if (attackInProgress)
            EvaluateAttackReward(fRwd: 1.5f, fPnlt: 0.6f);
        if (defenceInProgress)
            EvaluateDenfenceReward(fRwd: 1.2f, fPnlt: 0.5f);
        if (dodgeInProgress)
            EvaluateDodgeReward(fRwd: 1.2f, fPnlt: 0.5f);
    }

    void CurriculumLevel5()
    {
        // �ٿ���� ȸ��
        if (thisInfo.DistanceToBoundary() < 1.5f)
        {
            float factor = (1.5f - thisInfo.DistanceToBoundary()) / 1.5f;
            AddReward(-0.03f * factor);
        }

        // ������ �Ÿ��� ���� ������
        if (DistToEnemy() < 0.8f)
        {
            AddReward(-0.02f);
        }
        else if (DistToEnemy() < 4.0f)
        {
            float factor = (4 - DistToEnemy()) / 4;
            AddReward(0.006f * factor);
        }

        //���� ���� ó��
        if (attacked)
        {
            // ���ݹ��� �г�Ƽ
            if (AngleToEnemy() > 0.6f)
            {
                AddReward(0.06f * AngleToEnemy());
                // �� ���/ȸ�� ��Ÿ�� �� ���� �� ������
                if (enemyCore.defenceTimer > 0)
                    AddReward(0.2f);
                if (enemyCore.dodgeTimer > 0)
                    AddReward(0.2f);
            }
            else
                AddReward(-0.4f);
        }

        if (attackInProgress)
            EvaluateAttackReward(fRwd: 1.5f, fPnlt: 1.0f);
        if (defenceInProgress)
            EvaluateDenfenceReward(fRwd: 1.0f, fPnlt: 1.0f);
        if (dodgeInProgress)
            EvaluateDodgeReward(fRwd: 1.0f, fPnlt: 1.0f);
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

    float AngleToEnemy()
    {
        Vector3 toEnemy = enemyInfo.Position - thisInfo.Position;
        return Vector3.Dot(toEnemy.normalized, thisInfo.Forward);
    }

    float DistToEnemy()
    {
        return Vector3.Distance(thisInfo.Position, enemyInfo.Position);
    }
}
