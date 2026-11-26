<<<<<<< HEAD
=======
using System;
>>>>>>> 677b6e94d86198058adb6fb64c3dd0f1ddfa1a40
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
<<<<<<< HEAD

    private void Start()
    {
        eventSystem = EventSystem.current;
        ShowLoginPanel();
        UpdateAgreeTagUI();
=======
    private TMP_InputField loginEmailInput;
    private TMP_InputField loginPasswordInput;
    private TMP_Text statusText;
    private TMP_Text profileText;
    private GameObject profilePanel;
    private Button loginButton;
    private bool isRunning;
    private string pendingUid;
    private string profileUid;

    private void Awake()
    {
        if (signUpPanel != null && transform.IsChildOf(signUpPanel.transform))
            transform.SetParent(signUpPanel.transform.parent, true);
    }

    private async void Start()
    {
        eventSystem = EventSystem.current;
        if (loginPanel == null || signUpPanel == null || nicknameInput == null ||
            emailInput == null || passwordInput == null || passwordConfirmInput == null)
        {
            Debug.LogError("AccountManager: 계정 패널 또는 가입 입력 필드가 연결되지 않았습니다.");
            enabled = false;
            return;
        }

        foreach (var input in loginPanel.GetComponentsInChildren<TMP_InputField>(true))
        {
            if (input.name == "EmailInputField") loginEmailInput = input;
            if (input.name == "PasswordInputField") loginPasswordInput = input;
        }
        foreach (var button in loginPanel.GetComponentsInChildren<Button>(true))
        {
            if (button.name == "LoginButton") loginButton = button;
        }
        if (loginEmailInput == null || loginPasswordInput == null || loginButton == null)
        {
            Debug.LogError("AccountManager: 로그인 입력 필드 또는 버튼을 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        loginButton.onClick.AddListener(OnClickLogin);
        CreateAccountStatusUI();
        var avatarPanel = loginPanel.transform.parent.Find("AvatarSelectPanel");
        if (avatarPanel != null) avatarPanel.gameObject.SetActive(false);
        FirebaseManager.AuthStateChanged += OnAuthStateChanged;
        ShowLoginPanel();
        UpdateAgreeTagUI();
        try
        {
            SetBusy(true);
            SetStatus("Firebase 연결 중...");
            await FirebaseManager.InitializeAsync();
            if (this == null) return;
            if (FirebaseManager.IsSignedIn)
                await LoadProfileAsync(FirebaseManager.CurrentUserId);
            else
                SetStatus("");
        }
        catch (AuthManager.ProfileMissingException)
        {
            if (this != null) BeginProfileRecovery();
        }
        catch (Exception exception)
        {
            if (this != null)
            {
                if (FirebaseManager.IsSignedIn) AuthManager.SignOut();
                SetFailure("Firebase 연결 또는 프로필 조회에 실패했습니다. 다시 시도해 주세요.", exception);
            }
        }
        finally
        {
            if (this != null) SetBusy(false);
        }
    }

    private void OnDestroy()
    {
        FirebaseManager.AuthStateChanged -= OnAuthStateChanged;
        if (loginButton != null) loginButton.onClick.RemoveListener(OnClickLogin);
>>>>>>> 677b6e94d86198058adb6fb64c3dd0f1ddfa1a40
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
<<<<<<< HEAD
=======
        if (!isRunning) ShowLoginPanelCore();
    }

    private void ShowLoginPanelCore()
    {
        if (pendingUid != null && FirebaseManager.CurrentUserId == pendingUid)
            AuthManager.SignOut();
        pendingUid = null;
        profileUid = null;
        if (profilePanel != null) profilePanel.SetActive(false);
>>>>>>> 677b6e94d86198058adb6fb64c3dd0f1ddfa1a40
        if (loginPanel != null) loginPanel.SetActive(true);
        if (signUpPanel != null) signUpPanel.SetActive(false);
    }

    // 2. 회원가입 화면 띄우기
    public void ShowSignUpPanel()
    {
<<<<<<< HEAD
=======
        if (!isRunning) ShowSignUpPanelCore();
    }

    private void ShowSignUpPanelCore()
    {
>>>>>>> 677b6e94d86198058adb6fb64c3dd0f1ddfa1a40
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
<<<<<<< HEAD
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
=======
    public async void OnClickSignUpSubmit()
    {
        if (isRunning) return;
        if (!isAgreed)
        {
            SetStatus("이용약관에 동의해 주세요.");
            return;
        }

        string nickname = nicknameInput.text.Trim();
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        string passwordConfirm = passwordConfirmInput.text;

        if (string.IsNullOrWhiteSpace(nickname) || (pendingUid == null &&
            (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordConfirm))))
        {
            SetStatus("필요한 항목을 모두 입력해 주세요.");
            return;
        }

        if (pendingUid == null && password != passwordConfirm)
        {
            SetStatus("비밀번호가 일치하지 않습니다.");
            return;
        }

        try
        {
            SetBusy(true);
            SetStatus("회원 정보를 저장하는 중...");
            if (pendingUid == null)
            {
                var user = await AuthManager.CreateAccountAsync(email, password);
                if (this == null) return;
                pendingUid = user.UserId;
            }
            await AuthManager.SaveNicknameAsync(pendingUid, nickname);
            if (this == null) return;
            AuthManager.SignOut();
            pendingUid = null;
            passwordInput.text = "";
            passwordConfirmInput.text = "";
            ShowLoginPanelCore();
            SetStatus("회원가입이 완료되었습니다. 로그인해 주세요.");
        }
        catch (Exception exception)
        {
            if (this == null) return;
            string message = pendingUid == null
                ? "회원가입에 실패했습니다. 입력 정보와 네트워크를 확인해 주세요."
                : "계정은 생성되었지만 프로필 저장에 실패했습니다. 다시 가입 버튼을 눌러 주세요.";
            SetFailure(message, exception);
        }
        finally
        {
            if (this != null) SetBusy(false);
        }
    }

    public async void OnClickLogin()
    {
        if (isRunning) return;
        string email = loginEmailInput.text.Trim();
        string password = loginPasswordInput.text;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            SetStatus("이메일과 비밀번호를 입력해 주세요.");
            return;
        }

        try
        {
            SetBusy(true);
            SetStatus("로그인 중...");
            var user = await AuthManager.SignInAsync(email, password);
            if (this == null) return;
            loginPasswordInput.text = "";
            await LoadProfileAsync(user.UserId);
        }
        catch (AuthManager.ProfileMissingException)
        {
            if (this != null) BeginProfileRecovery();
        }
        catch (Exception exception)
        {
            if (this == null) return;
            if (FirebaseManager.IsSignedIn) AuthManager.SignOut();
            SetFailure("로그인 또는 프로필 조회에 실패했습니다. 정보를 확인해 주세요.", exception);
        }
        finally
        {
            if (this != null) SetBusy(false);
        }
    }

    public void OnClickLogout()
    {
        if (isRunning) return;
        AuthManager.SignOut();
        if (loginPasswordInput != null) loginPasswordInput.text = "";
        ShowLoginPanel();
        SetStatus("로그아웃되었습니다.");
    }

    private async System.Threading.Tasks.Task LoadProfileAsync(string uid)
    {
        string nickname = await AuthManager.GetNicknameAsync(uid);
        if (this == null) return;
        profileUid = uid;
        loginPanel.SetActive(false);
        signUpPanel.SetActive(false);
        profileText.text = $"안녕하세요, {nickname}님";
        profilePanel.SetActive(true);
        SetStatus("");
    }

    private void BeginProfileRecovery()
    {
        string uid = FirebaseManager.CurrentUserId;
        if (string.IsNullOrEmpty(uid))
        {
            ShowLoginPanelCore();
            SetStatus("로그인 상태가 변경되었습니다. 다시 로그인해 주세요.");
            return;
        }
        ShowSignUpPanelCore();
        pendingUid = uid;
        SetStatus("프로필이 없습니다. 닉네임과 약관 동의를 입력하고 가입 버튼을 눌러 주세요.");
    }

    private void OnAuthStateChanged(string uid)
    {
        if (profileUid != null && profileUid != uid)
        {
            profileText.text = "";
            ShowLoginPanelCore();
            SetStatus("로그인 상태가 변경되었습니다.");
        }
    }

    private void SetBusy(bool busy)
    {
        isRunning = busy;
        if (loginButton != null) loginButton.interactable = !busy;
    }

    private void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
        if (!string.IsNullOrEmpty(message)) Debug.Log($"[계정] {message}");
    }

    private void SetFailure(string message, Exception exception)
    {
        SetStatus(message);
        Debug.LogError($"[계정] {message} {exception}");
    }

    private void CreateAccountStatusUI()
    {
        var parent = loginPanel.transform.parent;
        var status = new GameObject("AccountStatus", typeof(RectTransform), typeof(TextMeshProUGUI));
        status.transform.SetParent(parent, false);
        statusText = status.GetComponent<TextMeshProUGUI>();
        statusText.font = nicknameInput.textComponent.font;
        statusText.fontSize = 30;
        statusText.color = new Color(0.18f, 0.42f, 0.28f);
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.raycastTarget = false;
        var statusRect = status.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.08f, 0.04f);
        statusRect.anchorMax = new Vector2(0.92f, 0.12f);
        statusRect.offsetMin = statusRect.offsetMax = Vector2.zero;

        profilePanel = new GameObject("AccountProfile", typeof(RectTransform), typeof(Image));
        profilePanel.transform.SetParent(parent, false);
        profilePanel.GetComponent<Image>().color = Color.white;
        var panelRect = profilePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;

        var label = new GameObject("Nickname", typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(profilePanel.transform, false);
        profileText = label.GetComponent<TextMeshProUGUI>();
        profileText.font = statusText.font;
        profileText.fontSize = 48;
        profileText.color = statusText.color;
        profileText.alignment = TextAlignmentOptions.Center;
        var labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.1f, 0.52f);
        labelRect.anchorMax = new Vector2(0.9f, 0.62f);
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;

        var logout = new GameObject("LogoutButton", typeof(RectTransform), typeof(Image), typeof(Button));
        logout.transform.SetParent(profilePanel.transform, false);
        logout.GetComponent<Image>().color = agreedBgColor;
        logout.GetComponent<Button>().onClick.AddListener(OnClickLogout);
        var buttonRect = logout.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.3f, 0.39f);
        buttonRect.anchorMax = new Vector2(0.7f, 0.47f);
        buttonRect.offsetMin = buttonRect.offsetMax = Vector2.zero;

        var buttonLabel = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        buttonLabel.transform.SetParent(logout.transform, false);
        var buttonText = buttonLabel.GetComponent<TextMeshProUGUI>();
        buttonText.font = statusText.font;
        buttonText.text = "로그아웃";
        buttonText.fontSize = 36;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.raycastTarget = false;
        var buttonTextRect = buttonLabel.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = buttonTextRect.offsetMax = Vector2.zero;
        profilePanel.SetActive(false);
        status.transform.SetAsLastSibling();
>>>>>>> 677b6e94d86198058adb6fb64c3dd0f1ddfa1a40
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