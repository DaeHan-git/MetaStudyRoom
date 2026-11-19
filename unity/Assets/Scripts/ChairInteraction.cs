using UnityEngine;
using UnityEngine.EventSystems;

public class ChairInteraction : MonoBehaviour
{
    [Header("Sitting Settings")]
    public Transform sitPosition; // 앉을 자리 (비워두면 의자 위치)
    public float interactionDistance = 5f; // 상호작용 가능한 거리 (테스트를 위해 넉넉하게 5로 설정)

    private Transform player;
    private Camera cam;

    void Start()
    {
        // 1. 플레이어 찾기 (태그가 Player여야 합니다)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        if (sitPosition == null)
        {
            sitPosition = transform;
        }

        cam = Camera.main;
    }

    // 유니티 3D 오브젝트에서 마우스 클릭/터치를 가장 확실하게 잡아주는 내장 함수
    void OnMouseDown()
    {
        // 만약 조이스틱 UI 위를 누른 거라면 무시
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        TrySit();
    }

    void TrySit()
    {
        if (player == null)
        {
            Debug.LogWarning("플레이어를 찾지 못했습니다! 캐릭터의 태그가 'Player'인지 확인하세요.");
            return;
        }

        // 플레이어와 의자의 거리 계산
        float distance = Vector3.Distance(player.position, sitPosition.position);

        if (distance <= interactionDistance)
        {
            PlayerJoystickMovement movement = player.GetComponent<PlayerJoystickMovement>();
            if (movement != null)
            {
                movement.SitOnChair(sitPosition);
                Debug.Log("의자에 앉았습니다!");
            }
        }
        else
        {
            Debug.Log("의자가 너무 멀리 있습니다! 가까이 가서 클릭해보세요.");
        }
    }
}