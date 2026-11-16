using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform background;
    private RectTransform handle;
    
    [Header("Movement Reference")]
    public PlayerJoystickMovement playerMovement; // 플레이어 스크립트 연결

    void Start()
    {
        background = GetComponent<RectTransform>();
        handle = transform.GetChild(0).GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out position))
        {
            // 조이스틱 크기 기준으로 정규화 (-1 ~ 1)
            position.x = (position.x / background.sizeDelta.x);
            position.y = (position.y / background.sizeDelta.y);

            Vector2 inputVector = new Vector2(position.x * 2, position.y * 2);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // 손잡이 시각적 이동 위치 제한
            handle.anchoredPosition = new Vector2(inputVector.x * (background.sizeDelta.x / 2), inputVector.y * (background.sizeDelta.y / 2));

            // 플레이어 스크립트로 입력값 전달!
            if (playerMovement != null)
            {
                playerMovement.SetJoystickInput(inputVector);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 손을 떼면 원위치 및 정지
        handle.anchoredPosition = Vector2.zero;
        if (playerMovement != null)
        {
            playerMovement.SetJoystickInput(Vector2.zero);
        }
    }
}