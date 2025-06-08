using UnityEngine;

public class CooldownManager_Defense : MonoBehaviour
{
    public CooldownSlot attackSlot;
    public CooldownSlot defendSlot;
    public CooldownSlot dodgeSlot;

    //void Update() // 테스트용 임시 업데이트 함수
    //{
    //    if (Input.GetKeyDown(KeyCode.Alpha4)) attackSlot.StartCooldown();
    //    if (Input.GetKeyDown(KeyCode.Alpha5)) defendSlot.StartCooldown();
    //    if (Input.GetKeyDown(KeyCode.Alpha6)) dodgeSlot.StartCooldown();
    //}

    // 외부에서 호출할 수 있도록 공개 메서드
    public void TriggerCooldown(int index)
    {
        switch (index)
        {
            case 0: attackSlot.StartCooldown(); break;
            case 1: defendSlot.StartCooldown(); break;
            case 2: dodgeSlot.StartCooldown(); break;
        }
    }
}