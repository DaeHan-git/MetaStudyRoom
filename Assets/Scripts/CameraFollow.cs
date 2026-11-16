using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 내 캐릭터(Cat05)를 연결할 변수
    public Vector3 offset = new Vector3(0f, 2.5f, -4f); // 캐릭터 기준 뒤통수 위치 오프셋
    public float smoothSpeed = 10f; // 카메라가 따라오는 부드러움 정도

    void LateUpdate()
    {
        if (target == null) return;

        // 캐릭터의 회전 방향을 반영하여 뒤쪽 목표 위치 계산
        Vector3 targetPosition = target.position + target.TransformDirection(offset);

        // 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // 항상 캐릭터의 상체(머리 부근)를 바라보도록 설정
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}