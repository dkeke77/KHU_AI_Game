using UnityEngine;

// 월드 공간 3D 텍스트가 카메라를 향하도록 회전시키기
public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null) return;
        // 라벨이 카메라를 바라보게 회전
        transform.rotation = Quaternion.LookRotation(
            transform.position - Camera.main.transform.position,
            Vector3.up
        );
    }
}

