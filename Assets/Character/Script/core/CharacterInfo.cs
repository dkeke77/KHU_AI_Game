using UnityEngine;
using System.Collections.Generic;

public class CharacterInfo : MonoBehaviour
{
    private Dictionary<string, string> jointPaths = new Dictionary<string, string>()
    {
        // 필요에 따라 경로를 수정하세요 (모델 구조에 맞게!)
        { "Head", "Paladin/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head" },
        { "RightHand", "Paladin/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand" },
        { "LeftHand", "Paladin/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand" },
        { "RightArm", "Paladin/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm" },
        { "LeftArm", "Paladin/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm" },
        { "RightFoot", "Paladin/mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg/mixamorig:RightFoot" },
        { "LeftFoot", "Paladin/mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg/mixamorig:LeftFoot" },
        { "Spine", "Paladin/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2" },
        { "Sword", "Paladin/Paladin_J_Nordstrom_Sword" }
        // 원하는 조인트 추가 가능
    };

    CharacterCore core;
    Animator anim;

    // Health
    public int CurrentHP => core.cur_hp;
    public bool IsDead => core.isDead;
    public bool IsCollideWithCharacter => core.isCollideWithCharacter;
    public int DodgeCounter => core.dodgeCounter;
    public int BlockCounter => core.blockCounter;

    // Movement
    public Vector3 Position => transform.position;
    public Vector3 Forward => transform.forward;
    public float speed => core.speed;

    // State
    public PlayerState CurrentState => core.state;
    public bool IsDodging => core.isDodging;
    public bool IsBlocking => core.isBlocking;

    // Cooldowns
    public bool CanAttack => core.CanAttack();
    public bool CanDefence => core.CanDefence();
    public bool CanDodge => core.CanDodge();
    public float AttackTimer => core.attackTimer;
    public float DefenceTimer => core.defenceTimer;
    public float DodgeTimer => core.dodgeTimer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        core = GetComponent<CharacterCore>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Vector3 NormalizedPosition()
    {
        Bounds bnd = core.floor.GetComponent<MeshCollider>().bounds;

        Vector3 relativePos = this.Position - bnd.center;
        float halfX = bnd.extents.x;
        float halfZ = bnd.extents.z;

        float normRelX = Mathf.Clamp(relativePos.x / halfX, -1f, 1f);
        float normRelZ = Mathf.Clamp(relativePos.z / halfZ, -1f, 1f);

        return new Vector3(normRelX, 0, normRelZ);
    }

    public float GetMaxDistOfFloor()
    {
        Bounds bnd = core.floor.GetComponent<MeshCollider>().bounds;
        return Mathf.Sqrt(Mathf.Pow(bnd.size.x, 2) + Mathf.Pow(bnd.size.z, 2));
    }

    public float DistanceToBoundary()
    {
        Bounds bounds = core.floor.GetComponent<MeshCollider>().bounds;

        float dx = Mathf.Min(this.Position.x - bounds.min.x, bounds.max.x - this.Position.x);
        float dz = Mathf.Min(this.Position.z - bounds.min.z, bounds.max.z - this.Position.z);

        return Mathf.Min(dx, dz); // 가장 가까운 방향의 거리
    }

    public Vector3? GetJointPosition(string jointName)
    {
        if (!jointPaths.ContainsKey(jointName))
        {
            Debug.LogWarning($"Unknown joint name: {jointName}");
            return null;
        }

        string path = jointPaths[jointName];
        Transform joint = transform.Find(path);

        if (joint == null)
        {
            Debug.LogWarning($"Joint not found at path: {path}");
            return null;
        }

        return joint.position;
    }
}
