using UnityEngine;
using System.Collections.Generic;

public class CharacterInfo : MonoBehaviour
{
    private Dictionary<string, string> jointPaths = new Dictionary<string, string>()
    {
        // �ʿ信 ���� ��θ� �����ϼ��� (�� ������ �°�!)
        { "Head", "Paladin/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head" },
        { "RightHand", "Paladin/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand" },
        { "LeftHand", "Paladin/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand" },
        { "RightArm", "Paladin/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm" },
        { "LeftArm", "Paladin/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm" },
        { "RightFoot", "Paladin/mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg/mixamorig:RightFoot" },
        { "LeftFoot", "Paladin/mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg/mixamorig:LeftFoot" },
        { "Spine", "Paladin/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2" },
        { "Sword", "Paladin/Paladin_J_Nordstrom_Sword" }
        // ���ϴ� ����Ʈ �߰� ����
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
