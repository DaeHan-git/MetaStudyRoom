using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerJoystickMovement : MonoBehaviour
{
    [Header("Persistence")]
    [Tooltip("씬 전환 시 플레이어 오브젝트를 파괴하지 않고 유지합니다.")]
    public bool keepAcrossScenes = true;

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


    [Header("Away Mode Reference")]
    [Tooltip("자리비움(true)일 때 앉기 타이머를 일시정지하고, 공부중(false)일 때 재개합니다.")]
    public AwayModeController awayModeController;

    [Header("Sitting Action Buttons UI")]
    [Tooltip("앉아있을 때 화면에 표시할 버튼들(일어서기 버튼 포함 가능)")]
    public List<GameObject> sitActionButtonObjs = new List<GameObject>();

    private CharacterController characterController;
    private Rigidbody rb;
    private Animator animator;
    private bool isUsingCharacterController;
    private bool isUsingRigidbody;

    private bool lastIsMoving = false;
    private bool isSitting = false;

    void Awake()
    {
        if (keepAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    

  

    private void RefreshCameraReferences()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            cameraFollow = cameraTransform.GetComponent<CameraFollow>();
        }
        if (cameraFollow == null)
        {
            cameraFollow = FindFirstObjectByType<CameraFollow>();
        }
        if (cameraTransform == null && cameraFollow != null)
        {
            cameraTransform = cameraFollow.transform;
        }
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        RefreshCameraReferences();

        if (awayModeController == null)
        {
            awayModeController = FindFirstObjectByType<AwayModeController>();
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

        SetSitActionButtonsActive(false);

        if (joystickObj != null)
        {
            joystickObj.SetActive(true);
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        // 1. 앉아있는 상태일 때 (조이스틱 숨김, 자리비움이 아닐 때만 타이머 가동)
        if (isSitting)
        {
            bool isAway = (awayModeController != null && awayModeController.IsAway);
            if (!isAway)
            {
                sittingTime += Time.deltaTime;
                UpdateTimerUI();
            }
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
        joystickInput = Vector2.zero;

        UpdateTimerUI();

        if (joystickObj != null)
        {
            joystickObj.SetActive(false);
        }

        if (sitTimerTextObj != null)
        {
            sitTimerTextObj.SetActive(true);
        }

        SetSitActionButtonsActive(true);

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

        float totalStudyDuration = sittingTime;
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

        SetSitActionButtonsActive(false);

        if (animator != null)
        {
            animator.SetBool("IsSit", false);

            if (totalStudyDuration >= 30f)
            {
                animator.SetTrigger("LevelUp");
                Debug.Log($"실제 공부 시간 30초 이상 달성 후 기립! 누적 시간: {totalStudyDuration:F1}초 -> 레벨업 트리거 발동");
            }
        }

        if (cameraFollow != null)
        {
            cameraFollow.SetSittingState(false);
        }
    }

    private void SetSitActionButtonsActive(bool active)
    {
        for (int i = 0; i < sitActionButtonObjs.Count; i++)
        {
            if (sitActionButtonObjs[i] != null)
            {
                sitActionButtonObjs[i].SetActive(active);
            }
        }
    }

    // 추가 버튼용 예시 메서드 (원하는 기능으로 교체/추가해서 버튼 OnClick에 연결)
    public void OnSitExtraAction1()
    {
        if (!isLocalPlayer || !isSitting) return;
        Debug.Log("앉은 상태 추가 버튼 1 클릭");
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshCameraReferences();
        MoveToSceneSpawnPoint();
    }

    public void MoveToSceneSpawnPoint()
    {
        SceneSpawnPoint spawnPoint = FindFirstObjectByType<SceneSpawnPoint>();
        if (spawnPoint == null)
        {
            Debug.LogWarning($"[PlayerJoystickMovement] 현재 씬('{SceneManager.GetActiveScene().name}')에 SceneSpawnPoint가 없습니다.");
            return;
        }

        // 앉아있는 상태로 씬이 바뀌었다면 앉기 상태 해제
        if (isSitting)
        {
            StandUp();
        }

        // CharacterController / Rigidbody가 좌표를 덮어쓰지 않도록 잠시 비활성화 후 이동
        bool hadCC = (characterController != null && characterController.enabled);
        if (hadCC) characterController.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = spawnPoint.transform.position;
        transform.rotation = spawnPoint.transform.rotation;

        if (hadCC) characterController.enabled = true;

        Debug.Log($"[PlayerJoystickMovement] 스폰 위치('{spawnPoint.name}')로 이동 완료: {spawnPoint.transform.position}");
    }
}