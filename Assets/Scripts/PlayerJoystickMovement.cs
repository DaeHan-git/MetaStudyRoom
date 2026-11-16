using Unity.Netcode;
using TMPro;
using UnityEngine;

public class PlayerJoystickMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Joystick Reference")]
    [HideInInspector] public Vector2 joystickInput;

    [Header("Camera Reference")]
    public Transform cameraTransform;

    [Header("Sit Timer UI")]
    public GameObject sitTimerTextObj;
    private TextMeshProUGUI sitTimerText;
    private float sittingTime = 0f;
    private float sitStartTime = 0f;

    private CharacterController characterController;
    private Rigidbody rb;
    private Animator animator;
    private bool isUsingCharacterController;
    private bool isUsingRigidbody;

    private bool lastIsMoving = false;
    private bool isSitting = false;

    void Start()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.StartHost();
        }

        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
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
    }

    void Update()
    {
        if (!IsOwner) return;

        // 1. 앉아있는 상태일 때 (조이스틱으로 일어나지 않고 타이머만 작동)
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
        if (!IsOwner) return;
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
        if (!IsOwner) return;
        joystickInput = input;
    }

    public void SitOnChair(Transform chairSitPos)
    {
        if (!IsOwner) return;

        isSitting = true;
        sittingTime = 0f;
        sitStartTime = Time.time;

        if (sitTimerTextObj != null)
        {
            sitTimerTextObj.SetActive(true);
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
    }

    public void StandUp()
    {
        if (!IsOwner) return;

        float totalSitDuration = Time.time - sitStartTime;
        isSitting = false;

        if (rb != null) rb.isKinematic = false;
        if (characterController != null) characterController.enabled = true;

        if (sitTimerTextObj != null)
        {
            sitTimerTextObj.SetActive(false);
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
    }
}