using UnityEngine;

public class CooldownManager_Defense : MonoBehaviour
{
    public CooldownSlot attackSlot;
    public CooldownSlot defendSlot;
    public CooldownSlot dodgeSlot;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4)) attackSlot.StartCooldown();
        if (Input.GetKeyDown(KeyCode.Alpha5)) defendSlot.StartCooldown();
        if (Input.GetKeyDown(KeyCode.Alpha6)) dodgeSlot.StartCooldown();
    }
}
