using TMPro;
using UnityEngine;

public class PlayerJoystickMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Control Ownership")]
    [Tooltip("로컬 플레이어(내 캐릭터)만 조이스틱 입력 및 착석 UI 제어를 허용합니다.")]
    public bool isLocalPlayer = true;

    [Header("Joystick Reference")]
    public GameObject joystickObj; // 앉을 때 숨기고 일어날 때 다시 띄울 조이스틱 오브젝트
    [HideInInspector] public Vector2 joystickInput;

    [Header("Camera Reference")]
    public Transform cameraTransform;
    public CameraFollow cameraFollow; // 착석 시 카메라 구도 전환용

    [Header("Sit Timer UI")]
    public GameObject sitTimerTextObj;
    private TextMeshProUGUI sitTimerText;
    private float sittingTime = 0f;
    private float sitStartTime = 0f;

    [Header("Stand Up Button UI")]
    public GameObject standUpButtonObj; // 일어서기 버튼 GameObject

    private CharacterController characterController;
    private Rigidbody rb;
    private Animator animator;
    private bool isUsingCharacterController;
    private bool isUsingRigidbody;

    private bool lastIsMoving = false;
    private bool isSitting = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraFollow == null && cameraTransform != null)
        {
            cameraFollow = cameraTransform.GetComponent<CameraFollow>();
        }
        if (cameraFollow == null)
        {
            cameraFollow = FindFirstObjectByType<CameraFollow>();
        }

        isUsingCharacterController = (characterController != null);
        isUsingRigidbody = (rb != null);

        if (isUsingRigidbody)
        {
            rb.freezeRotation = true;
        }

        if (sitTimerTextObj != null)
        {
            sitTimerText = sitTimerTextObj.GetComponent<TextMeshProUGUI>();
            sitTimerTextObj.SetActive(false);
        }

        if (standUpButtonObj != null)
        {
            standUpButtonObj.SetActive(false);
        }

        if (joystickObj != null)
        {
            joystickObj.SetActive(true);
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        // 1. 앉아있는 상태일 때 (조이스틱 숨김, 타이머만 작동)
        if (isSitting)
        {
            sittingTime += Time.deltaTime;
            UpdateTimerUI();
            return;
        }

        // 2. 평상시 이동 및 회전 처리
        Vector3 rawInput = new Vector3(joystickInput.x, 0f, joystickInput.y);
        bool isMoving = rawInput.magnitude > 0.1f;

        if (animator != null && isMoving != lastIsMoving)
        {
            animator.SetBool("IsMoving", isMoving);
            lastIsMoving = isMoving;
        }

        if (isMoving && cameraTransform != null)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = (camForward * rawInput.z + camRight * rawInput.x).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (!isUsingCharacterController && !isUsingRigidbody)
            {
                transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);
            }
        }
    }

    void UpdateTimerUI()
    {
        if (sitTimerText != null)
        {
            int minutes = Mathf.FloorToInt(sittingTime / 60f);
            int seconds = Mathf.FloorToInt(sittingTime % 60f);
            sitTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        if (isSitting || cameraTransform == null) return;

        Vector3 rawInput = new Vector3(joystickInput.x, 0f, joystickInput.y);
        if (rawInput.magnitude > 0.1f)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = (camForward * rawInput.z + camRight * rawInput.x).normalized;

            if (isUsingRigidbody)
            {
                Vector3 targetVelocity = moveDir * moveSpeed;
                targetVelocity.y = rb.linearVelocity.y;
                rb.linearVelocity = targetVelocity;
            }
            else if (isUsingCharacterController)
            {
                Vector3 targetVelocity = moveDir * moveSpeed;
                if (!characterController.isGrounded)
                {
                    targetVelocity.y += Physics.gravity.y * Time.fixedDeltaTime;
                }
                characterController.Move(targetVelocity * Time.fixedDeltaTime);
            }
        }
        else
        {
            if (isUsingRigidbody)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }
    }

    public void SetJoystickInput(Vector2 input)
    {
        if (!isLocalPlayer || isSitting) return;
        joystickInput = input;
    }

    public void SitOnChair(Transform chairSitPos)
    {
        if (!isLocalPlayer) return;

        isSitting = true;
        sittingTime = 0f;
        sitStartTime = Time.time;
        joystickInput = Vector2.zero;

        if (joystickObj != null)
        {
            joystickObj.SetActive(false);
        }

        if (sitTimerTextObj != null)
        {
            sitTimerTextObj.SetActive(true);
        }

        if (standUpButtonObj != null)
        {
            standUpButtonObj.SetActive(true);
        }

        transform.position = chairSitPos.position;
        transform.rotation = chairSitPos.rotation;

        if (rb != null) rb.isKinematic = true;
        if (characterController != null) characterController.enabled = false;

        if (animator != null)
        {
            animator.SetBool("IsSit", true);
            animator.SetBool("IsMoving", false);
        }

        if (cameraFollow != null)
        {
            cameraFollow.SetSittingState(true, chairSitPos);
        }
    }

    // UI 버튼의 OnClick() 이벤트에 이 메서드를 연결하세요
    public void StandUp()
    {
        if (!isLocalPlayer) return;
        if (!isSitting) return;

        float totalSitDuration = Time.time - sitStartTime;
        isSitting = false;
        joystickInput = Vector2.zero;

        if (joystickObj != null)
        {
            joystickObj.SetActive(true);
        }

        if (rb != null) rb.isKinematic = false;
        if (characterController != null) characterController.enabled = true;

        if (sitTimerTextObj != null)
        {
            sitTimerTextObj.SetActive(false);
        }

        if (standUpButtonObj != null)
        {
            standUpButtonObj.SetActive(false);
        }

        if (animator != null)
        {
            animator.SetBool("IsSit", false);

            if (totalSitDuration >= 30f)
            {
                animator.SetTrigger("LevelUp");
                Debug.Log($"30초 이상 앉아있다가 일어남! 총 시간: {totalSitDuration:F1}초 -> 레벨업 트리거 발동");
            }
        }

        if (cameraFollow != null)
        {
            cameraFollow.SetSittingState(false);
        }
    }
}