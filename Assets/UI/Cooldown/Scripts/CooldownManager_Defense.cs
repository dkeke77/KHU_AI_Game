using UnityEngine;

public class CooldownManager_Defense : MonoBehaviour
{
    public CooldownSlot attackSlot;
    public CooldownSlot defendSlot;
    public CooldownSlot dodgeSlot;

    //void Update() // �׽�Ʈ�� �ӽ� ������Ʈ �Լ�
    //{
    //    if (Input.GetKeyDown(KeyCode.Alpha4)) attackSlot.StartCooldown();
    //    if (Input.GetKeyDown(KeyCode.Alpha5)) defendSlot.StartCooldown();
    //    if (Input.GetKeyDown(KeyCode.Alpha6)) dodgeSlot.StartCooldown();
    //}

    // �ܺο��� ȣ���� �� �ֵ��� ���� �޼���
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