using UnityEngine;

public class SceneSpawnPoint : MonoBehaviour
{
    [Tooltip("이 스폰 포인트의 식별자 (기본값: Default)")]
    public string spawnId = "Default";

    void OnDrawGizmos()
    {
        // Scene 뷰에서 스폰 위치와 바라보는 방향을 쉽게 보기 위한 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.35f);
        Gizmos.DrawRay(transform.position, transform.forward * 1.0f);
    }
}