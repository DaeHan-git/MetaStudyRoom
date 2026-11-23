using UnityEngine;
using UnityEngine.EventSystems;

public class ChairInteraction : MonoBehaviour
{
    [Header("Sitting Settings")]
    public Transform sitPosition; // 앉을 자리 (비워두면 의자 위치)
    public float interactionDistance = 5f; // 상호작용 가능한 거리
    public LayerMask raycastLayerMask = ~0; // 레이캐스트 대상 레이어

    private Transform player;
    private Camera cam;

    void Start()
    {
        if (sitPosition == null)
        {
            sitPosition = transform;
        }

        RefreshReferences();
    }

    void RefreshReferences()
    {
        if (cam == null || !cam.isActiveAndEnabled)
        {
            cam = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Update()
    {
        Vector2 pointerPos;
        if (!TryGetPointerDownPosition(out pointerPos))
            return;

        // UI(조이스틱, 버튼 등) 위를 터치/클릭한 경우 무시
        if (IsPointerOverUI(pointerPos))
            return;

        RefreshReferences();
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(pointerPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, raycastLayerMask, QueryTriggerInteraction.Ignore))
        {
            // 의자 본인 또는 자식 오브젝트에 맞았는지 확인
            ChairInteraction hitChair = hit.collider.GetComponentInParent<ChairInteraction>();
            if (hitChair == this)
            {
                TrySit();
            }
        }
    }

    bool TryGetPointerDownPosition(out Vector2 pos)
    {
        // 1. 모바일 터치 입력 우선
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                pos = touch.position;
                return true;
            }
        }

        // 2. 에디터 / 마우스 클릭 입력
        if (Input.GetMouseButtonDown(0))
        {
            pos = Input.mousePosition;
            return true;
        }

        pos = Vector2.zero;
        return false;
    }

    bool IsPointerOverUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;

        // 터치 ID가 있으면 해당 터치 기준, 없으면 마우스 기준 검사
        if (Input.touchCount > 0)
        {
            int fingerId = Input.GetTouch(0).fingerId;
            return EventSystem.current.IsPointerOverGameObject(fingerId);
        }

        return EventSystem.current.IsPointerOverGameObject();
    }

    void TrySit()
    {
        if (player == null)
        {
            Debug.LogWarning("[ChairInteraction] 'Player' 태그를 가진 오브젝트를 찾지 못했습니다.");
            return;
        }

        float distance = Vector3.Distance(player.position, sitPosition.position);
        if (distance <= interactionDistance)
        {
            PlayerJoystickMovement movement = player.GetComponentInParent<PlayerJoystickMovement>();
            if (movement == null)
            {
                movement = player.GetComponentInChildren<PlayerJoystickMovement>();
            }

            if (movement != null)
            {
                movement.SitOnChair(sitPosition);
                Debug.Log($"[ChairInteraction] '{name}' 의자에 앉았습니다. (거리: {distance:F2})");
            }
            else
            {
                Debug.LogWarning("[ChairInteraction] Player에 PlayerJoystickMovement 컴포넌트가 없습니다.");
            }
        }
        else
        {
            Debug.Log($"[ChairInteraction] 의자가 너무 멀리 있습니다. (현재 거리: {distance:F2}, 허용 거리: {interactionDistance})");
        }
    }
}