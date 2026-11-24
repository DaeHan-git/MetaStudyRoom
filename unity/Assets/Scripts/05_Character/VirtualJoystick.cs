using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Joystick UI")]
    [SerializeField] private RectTransform background; // 조이스틱 배경 (처음엔 꺼두세요)
    [SerializeField] private RectTransform handle;     // 조이스틱 손잡이 (background의 자식)

    [Header("Movement Reference")]
    public PlayerJoystickMovement playerMovement;

    private RectTransform touchArea;
    private Vector2 inputVector;

    void Awake()
    {
        touchArea = GetComponent<RectTransform>();

        if (background != null)
        {
            background.gameObject.SetActive(false);
        }

        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (background == null || handle == null) return;

        // 1. 터치한 화면 좌표를 touchArea 로컬 좌표로 변환해 배경 위치 이동
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                touchArea, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            background.anchoredPosition = localPoint;
        }

        // 2. 조이스틱 표시 및 손잡이 초기화
        background.gameObject.SetActive(true);
        handle.anchoredPosition = Vector2.zero;
        inputVector = Vector2.zero;

        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (background == null || handle == null || !background.gameObject.activeSelf) return;

        // 배경(background) 기준 로컬 좌표 계산
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out Vector2 position))
        {
            float radiusX = background.sizeDelta.x * 0.5f;
            float radiusY = background.sizeDelta.y * 0.5f;

            position.x /= radiusX;
            position.y /= radiusY;

            inputVector = new Vector2(position.x, position.y);
            if (inputVector.magnitude > 1.0f)
            {
                inputVector = inputVector.normalized;
            }

            // 손잡이 위치 제한
            handle.anchoredPosition = new Vector2(inputVector.x * radiusX, inputVector.y * radiusY);

            if (playerMovement != null)
            {
                playerMovement.SetJoystickInput(inputVector);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;

        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }

        // 손을 떼면 조이스틱 숨김
        if (background != null)
        {
            background.gameObject.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.SetJoystickInput(Vector2.zero);
        }
    }
}