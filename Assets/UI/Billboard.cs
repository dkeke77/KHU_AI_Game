using UnityEngine;

// ���� ���� 3D �ؽ�Ʈ�� ī�޶� ���ϵ��� ȸ����Ű��
public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null) return;
        // ���� ī�޶� �ٶ󺸰� ȸ��
        transform.rotation = Quaternion.LookRotation(
            transform.position - Camera.main.transform.position,
            Vector3.up
        );
    }
}

