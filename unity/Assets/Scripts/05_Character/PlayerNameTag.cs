using UnityEngine;
using TMPro;

public class PlayerNameTag : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2.0f, 0f);
    [SerializeField] private bool faceCameraOnlyY = false;

    private Camera targetCamera;

    private void Awake()
    {
        if (nameText == null)
            nameText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null) return;
        }

        // 부모 회전과 무관하게 머리 위 위치 유지
        if (transform.parent != null)
        {
            transform.position = transform.parent.position + offset;
        }

        // 카메라 방향 정렬 (카메라 forward를 따라가면 2D 화면처럼 평면 유지에 유리)
        if (faceCameraOnlyY)
        {
            Vector3 lookPos = targetCamera.transform.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            transform.forward = targetCamera.transform.forward;
        }
    }

    public void SetName(string playerName)
    {
        if (nameText != null)
        {
            nameText.text = playerName;
        }
    }
}