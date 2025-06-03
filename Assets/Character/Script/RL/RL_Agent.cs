using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class RL_Agent : Agent
{
    // Episode timer
    private float episodeTimer = 0f;
    public const float MAX_EPISODE_TIME = 90f;

    // Game objects
    Rigidbody rb;
    Bounds floor;
    CharacterCore core;
    CharacterInfo me;
    CharacterCore enemyCore;
    CharacterInfo enemy;

    // Enemy hit
    float oldEnemyHP;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        floor = GameObject.Find("Floor").GetComponent<MeshCollider>().bounds;
        
        core = GetComponent<CharacterCore>();
        me = GetComponent<CharacterInfo>();

        GameObject[] enemyObjs = GameObject.FindGameObjectsWithTag("Character");
        foreach (var obj in enemyObjs)
        {
            if (obj != this.gameObject)
            {
                enemyCore = obj.GetComponent<CharacterCore>();
                enemy = obj.GetComponent<CharacterInfo>();
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        episodeTimer = 0f;
        oldEnemyHP = 100f;

        float randX = Random.Range(-10f, -5f);
        float randZ = Random.Range(-5f, 5f);
        transform.position = new Vector3(randX, transform.position.y, randZ);

        core.Spawn();
        enemyCore.Spawn();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 만약 학습이 느리다면 Position과 Foward에서 불필요한 요소 제거할것
        sensor.AddObservation(me.Position);
        sensor.AddObservation(me.Forward);
        sensor.AddObservation(me.CurrentHP);
        sensor.AddObservation(me.AttackTimer);
        sensor.AddObservation(me.DefenceTimer);
        sensor.AddObservation(me.DodgeTimer);
        sensor.AddObservation((int)me.CurrentState);

        sensor.AddObservation(enemy.Position);
        sensor.AddObservation(enemy.Forward);
        sensor.AddObservation(enemy.CurrentHP);
        sensor.AddObservation(enemy.AttackTimer);
        sensor.AddObservation(enemy.DefenceTimer);
        sensor.AddObservation(enemy.DodgeTimer);
        sensor.AddObservation((int)enemy.CurrentState);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        episodeTimer += Time.deltaTime;
        if (episodeTimer >= MAX_EPISODE_TIME)
        {
            SetReward(0f);
            EndEpisode();
        }

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
                        core.Attack();
                    break;
                case 2:
                    if (core.CanDefence())
                        core.Defence();
                    break;
                case 3:
                    if (core.CanDodge())
                        core.Dodge();
                    break;
                default:
                    break;
            }
        }

        // floor 벗어나면 punishment
        if (floor.Contains(me.Position))
        {
            AddReward(-20.0f);
            EndEpisode();
        }

        // 사망
        if (me.IsDead)
        {
            AddReward(-10.0f);
            EndEpisode();
        }
        else if (enemy.IsDead)
        {
            AddReward(10.0f);
            EndEpisode();
        }

        // 공격/방어/회피 성공 시 보상함수
        if (oldEnemyHP != enemy.CurrentHP)
            AddReward(1.0f);

        // 피격 확인용 적 HP
        oldEnemyHP = enemy.CurrentHP;
    }

    bool isOutofFloor()
    {
        Vector3 chPos = me.Position;
        chPos.y = 0f;

        return floor.Contains(chPos);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            if (core.isDodging)
                AddReward(0.5f);
            else if (core.isBlocking)
                AddReward(0.5f);
            else
                AddReward(-1.0f);
        }
    }
}
