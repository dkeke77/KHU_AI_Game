using UnityEngine;

public class CooldownManager_Attack : MonoBehaviour
{
    public CooldownSlot attackSlot;
    public CooldownSlot defendSlot;
    public CooldownSlot dodgeSlot;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) attackSlot.StartCooldown();
        if (Input.GetKeyDown(KeyCode.Alpha2)) defendSlot.StartCooldown();
        if (Input.GetKeyDown(KeyCode.Alpha3)) dodgeSlot.StartCooldown();
    }
}
