using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class AccountManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject loginPanel;
    public GameObject signUpPanel;

    [Header("SignUp Input Fields")]
    public TMP_InputField nicknameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField passwordConfirmInput;

    [Header("Agree Tag UI")]
    public Button agreeTagButton;
    public Image agreeTagImage;       // AgreeTag 버튼의 배경 Image Component
    public TextMeshProUGUI agreeTagText; // AgreeTag 버튼 내부 Text Component
    private bool isAgreed = false;

    // 색상 세팅 (동의 전 / 동의 후)
    private Color unagreedBgColor = new Color(0.91f, 0.95f, 0.92f);   // 연한 민트
    private Color agreedBgColor = new Color(0.27f, 0.45f, 0.32f);     // 진한 녹색
    private Color unagreedTextColor = new Color(0.18f, 0.42f, 0.28f); // 진한 녹색 글자
    private Color agreedTextColor = Color.white;                       // 흰색 글자

    private EventSystem eventSystem;

    private void Start()
    {
        eventSystem = EventSystem.current;
        ShowLoginPanel();
        UpdateAgreeTagUI();
    }

    private void Update()
    {
        // 회원가입 화면이 활성화되어 있을 때 Tab 키 입력 처리
        if (Input.GetKeyDown(KeyCode.Tab) && signUpPanel != null && signUpPanel.activeSelf)
        {
            HandleTabKey();
        }
    }

    // Tab 키를 이용한 입력창 포커스 순차 이동
    private void HandleTabKey()
    {
        if (eventSystem == null) eventSystem = EventSystem.current;
        if (eventSystem == null) return;

        GameObject current = eventSystem.currentSelectedGameObject;
        TMP_InputField[] fields = new TMP_InputField[] { nicknameInput, emailInput, passwordInput, passwordConfirmInput };

        int currentIndex = -1;

        if (current != null)
        {
            TMP_InputField currentInput = current.GetComponent<TMP_InputField>();
            if (currentInput != null)
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i] == currentInput)
                    {
                        currentIndex = i;
                        break;
                    }
                }
            }
        }

        // 포커스가 없거나(Scale 영역 등으로 튄 경우) 마지막 필드이면 첫 번째 필드(닉네임)로 지정
        int nextIndex = (currentIndex == -1) ? 0 : (currentIndex + 1) % fields.Length;

        if (fields[nextIndex] != null)
        {
            fields[nextIndex].Select();
            fields[nextIndex].ActivateInputField(); // 텍스트 입력 커서 강제 활성화
        }
    }

    // 1. 로그인 화면 띄우기
    public void ShowLoginPanel()
    {
        if (loginPanel != null) loginPanel.SetActive(true);
        if (signUpPanel != null) signUpPanel.SetActive(false);
    }

    // 2. 회원가입 화면 띄우기
    public void ShowSignUpPanel()
    {
        if (loginPanel != null) loginPanel.SetActive(false);
        if (signUpPanel != null) signUpPanel.SetActive(true);
        ResetSignUpFields();
    }

    // 3. 이용약관 동의 버튼 클릭 토글
    public void OnClickAgreeTag()
    {
        isAgreed = !isAgreed; // 토글 (켜기/끄기)
        UpdateAgreeTagUI();
    }

    private void UpdateAgreeTagUI()
    {
        if (agreeTagImage != null)
        {
            agreeTagImage.color = isAgreed ? agreedBgColor : unagreedBgColor;
        }
        if (agreeTagText != null)
        {
            agreeTagText.color = isAgreed ? agreedTextColor : unagreedTextColor;
            agreeTagText.text = isAgreed ? "✓ 동의함" : "✓ 동의";
        }
    }

    // 4. 가입하기 버튼 클릭 시 처리
    public void OnClickSignUpSubmit()
    {
        if (!isAgreed)
        {
            Debug.LogWarning("[경고] 이용약관에 동의해야 회원가입이 가능합니다.");
            return;
        }

        string nickname = nicknameInput.text;
        string email = emailInput.text;
        string password = passwordInput.text;
        string passwordConfirm = passwordConfirmInput.text;

        if (string.IsNullOrEmpty(nickname) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordConfirm))
        {
            Debug.LogWarning("[경고] 모든 항목을 입력해 주세요.");
            return;
        }

        if (password != passwordConfirm)
        {
            Debug.LogWarning("[경고] 비밀번호가 일치하지 않습니다.");
            return;
        }

        // TODO: 추후 DB 연동 시 INSERT 처리 메서드 호출 위치
        Debug.Log($"[회원가입 성공] DB 저장 완료 - Nickname: {nickname}, Email: {email}");

        // 성공 후 로그인 화면으로 전환
        ShowLoginPanel();
    }

    // 입력 필드 초기화
    private void ResetSignUpFields()
    {
        if (nicknameInput) nicknameInput.text = "";
        if (emailInput) emailInput.text = "";
        if (passwordInput) passwordInput.text = "";
        if (passwordConfirmInput) passwordConfirmInput.text = "";
        isAgreed = false;
        UpdateAgreeTagUI();
    }
}