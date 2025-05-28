
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    CharacterCore core;
    CharacterInfo cinfo;
    Weapon wpn;

    float hAxis;
    float vAxis;

    void Awake()
    {
        core = GetComponent<CharacterCore>();
        cinfo = GetComponent<CharacterInfo>();
        wpn = GetComponent<Weapon>();
    }

    void Update()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Attack") && core.CanAttack())
            core.Attack();
        else if (Input.GetButtonDown("Defence") && core.CanDefence())
            core.Defence();
        else if (Input.GetButtonDown("Dodge") && core.CanDodge())
            core.Dodge();
        else if (core.CanMove())
            core.HandleMovement(hAxis, vAxis);
    }
}
