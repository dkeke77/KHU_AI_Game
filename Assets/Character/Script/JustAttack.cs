using UnityEngine;

public class JustAttack : MonoBehaviour
{
    CharacterCore core;


    void Start()
    {
        core = GetComponent<CharacterCore>();

    }

    // Update is called once per frame
    void Update()
    {
        if (core.CanAttack())
            core.Attack();
    }
}
