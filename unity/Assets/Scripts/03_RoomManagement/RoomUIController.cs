using System;
using System.Collections.Generic;
using System.Threading;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RoomUIController : MonoBehaviour
{
    [Header("1. 화면 패널")]
    public GameObject roomListPanel;
    public GameObject createRoomPanel;
    public GameObject mapSelectPanel;

    [Header("2. 방 목록 화면")]
    public TMP_InputField searchInput;
    public Button allButton;
    public Button codingButton;
    public Button languageButton;
    public Button certificateButton;
    public Button openCreateButton;
    public TMP_InputField joinPasswordInput;
    public TMP_Text listStatusText;
    public Transform roomContent;
    public Button roomCardTemplate;

    [Header("3. 방 만들기 화면")]
    public Button createBackButton;
    public TMP_InputField roomNameInput;
    public TMP_Dropdown categoryDropdown;
    public TMP_Dropdown maxPeopleDropdown;
    public Toggle privateToggle;
    public GameObject passwordLabel;
    public TMP_InputField createPasswordInput;
    public Toggle textChatToggle;
    public Toggle voiceChatToggle;
    public Button openMapButton;
    public TMP_Text mapButtonText;
    public Button submitRoomButton;
    public TMP_Text createStatusText;

    [Header("4. 맵 선택 화면")]
    public Button mapBackButton;
    public Button cafeCard;
    public Button libraryCard;
    public Button schoolCard;
    public Button confirmMapButton;

    [Header("5. 방 카드 사진 (선택 사항)")]
    public Sprite cafeImage;
    public Sprite libraryImage;
    public Sprite schoolImage;

    [Header("7번 패널: 맵별 씬 이름 (.unity 제외)")]
    [SerializeField] string cafeSceneName = "StudyRoom_Cafe";
    [SerializeField] string librarySceneName = "StudyRoom_Library";
    [SerializeField] string schoolSceneName = "StudyRoom_School";

    [Header("Firestore (선택 사항)")]
    public Button refreshButton;

    const string Coding = "코딩", Language = "어학", Certificate = "자격증";
    readonly List<RoomRecord> rooms = new List<RoomRecord>();
    readonly List<GameObject> visibleCards = new List<GameObject>();
    readonly HashSet<string> closingRoomIds = new HashSet<string>();
    string filter = "전체", confirmedMap = "카페", pendingMap = "카페";
    string observedUid, listMessage = "Firebase 준비 중이에요.";
    ListenerRegistration roomListener;
    SynchronizationContext unityContext;
    int generation;
    bool configured, serverReady, creatingRoom, enteringRoom;

    void Awake()
    {
        unityContext = SynchronizationContext.Current;
        // 빠진 필드는 이름을 Console에 표시하고 시작을 중단합니다.
        var required = new Dictionary<string, UnityEngine.Object>
        {
            { nameof(roomListPanel), roomListPanel }, { nameof(createRoomPanel), createRoomPanel },
            { nameof(mapSelectPanel), mapSelectPanel }, { nameof(searchInput), searchInput },
            { nameof(allButton), allButton }, { nameof(codingButton), codingButton },
            { nameof(languageButton), languageButton }, { nameof(certificateButton), certificateButton },
            { nameof(openCreateButton), openCreateButton }, { nameof(listStatusText), listStatusText },
            { nameof(roomContent), roomContent }, { nameof(roomCardTemplate), roomCardTemplate },
            { nameof(createBackButton), createBackButton }, { nameof(roomNameInput), roomNameInput },
            { nameof(categoryDropdown), categoryDropdown }, { nameof(maxPeopleDropdown), maxPeopleDropdown },
            { nameof(privateToggle), privateToggle }, { nameof(passwordLabel), passwordLabel },
            { nameof(createPasswordInput), createPasswordInput }, { nameof(textChatToggle), textChatToggle },
            { nameof(voiceChatToggle), voiceChatToggle }, { nameof(openMapButton), openMapButton },
            { nameof(mapButtonText), mapButtonText }, { nameof(submitRoomButton), submitRoomButton },
            { nameof(createStatusText), createStatusText }, { nameof(mapBackButton), mapBackButton },
            { nameof(cafeCard), cafeCard }, { nameof(libraryCard), libraryCard },
            { nameof(schoolCard), schoolCard }, { nameof(confirmMapButton), confirmMapButton }
        };
        bool valid = true;
        foreach (var pair in required)
            if (pair.Value == null)
            {
                Debug.LogError($"[방] Inspector 연결 누락: {pair.Key}", this);
                valid = false;
            }
        if (!valid) { enabled = false; return; }

        categoryDropdown.ClearOptions();
        categoryDropdown.AddOptions(new List<string> { Coding, Language, Certificate });
        maxPeopleDropdown.ClearOptions();
        maxPeopleDropdown.AddOptions(new List<string> { "2명", "4명", "6명", "8명" });
        privateToggle.group = null;
        privateToggle.SetIsOnWithoutNotify(false);
        privateToggle.interactable = false; // 비밀번호를 공개 방 문서에 업로드하지 않습니다.
        UpdatePasswordField(false);
        createPasswordInput.contentType = TMP_InputField.ContentType.Password;
        if (joinPasswordInput != null) joinPasswordInput.gameObject.SetActive(false);

        openCreateButton.onClick.AddListener(OpenCreate);
        searchInput.onValueChanged.AddListener(_ => RefreshList());
        allButton.onClick.AddListener(() => SetFilter("전체"));
        codingButton.onClick.AddListener(() => SetFilter(Coding));
        languageButton.onClick.AddListener(() => SetFilter(Language));
        certificateButton.onClick.AddListener(() => SetFilter(Certificate));
        createBackButton.onClick.AddListener(() => Show(roomListPanel));
        openMapButton.onClick.AddListener(OpenMaps);
        submitRoomButton.onClick.AddListener(CreateRoom);
        cafeCard.onClick.AddListener(() => SelectMap("카페"));
        libraryCard.onClick.AddListener(() => SelectMap("도서관"));
        schoolCard.onClick.AddListener(() => SelectMap("학교"));
        mapBackButton.onClick.AddListener(() => Show(createRoomPanel));
        confirmMapButton.onClick.AddListener(ConfirmMap);
        if (refreshButton != null) refreshButton.onClick.AddListener(RefreshRooms);
        roomCardTemplate.gameObject.SetActive(false);
        configured = true;
        Show(roomListPanel);
    }

    void OnEnable()
    {
        if (!configured) return;
        FirebaseManager.AuthStateChanged += OnAuthChanged;
        RefreshRooms();
    }

    void OnDisable()
    {
        FirebaseManager.AuthStateChanged -= OnAuthChanged;
        generation++;
        StopListening();
        serverReady = false;
        observedUid = null;
        rooms.Clear();
        ClearCards();
    }

    void OnAuthChanged(string uid)
    {
        // Firebase 인증 이벤트가 발생한 스레드와 무관하게 UI는 Unity 스레드에서 처리합니다.
        unityContext?.Post(_ =>
        {
            if (this == null || !isActiveAndEnabled) return;
            if (observedUid == FirebaseManager.CurrentUserId) return;
            RoomSelectionContext.Clear();
            createStatusText.text = "";
            Show(roomListPanel);
            RefreshRooms();
        }, null);
    }

    void StopListening()
    {
        roomListener?.Stop();
        roomListener = null;
    }

    bool IsCurrent(int token, string uid)
    {
        return this != null && isActiveAndEnabled && token == generation
            && FirebaseManager.CurrentUserId == uid;
    }

    [ContextMenu("Firestore 방 목록 새로고침")]
    public async void RefreshRooms()
    {
        if (!configured || !isActiveAndEnabled) return;
        int token = ++generation;
        StopListening();
        rooms.Clear();
        serverReady = false;
        listMessage = "서버에서 방 목록을 확인하고 있어요.";
        RefreshList();
        UpdateControls();
        try
        {
            await FirebaseManager.InitializeAsync();
            if (this == null || !isActiveAndEnabled || token != generation) return;
            observedUid = FirebaseManager.CurrentUserId;
            if (string.IsNullOrEmpty(observedUid))
            {
                RoomSelectionContext.Clear();
                listMessage = "로그인 후 방 목록을 이용해 주세요.";
                RefreshList();
                return;
            }
            string uid = observedUid;
            roomListener = RoomFirestoreService.ActivePublicRooms()
                .Listen(MetadataChanges.Include, snapshot =>
                {
                    if (!IsCurrent(token, uid)) return;
                    try
                    {
                        // 캐시/미승인 로컬 쓰기를 서버 저장 성공으로 표시하지 않습니다.
                        if (snapshot.Metadata.IsFromCache || snapshot.Metadata.HasPendingWrites)
                        {
                            serverReady = false;
                            listMessage = "서버 확인 대기 중이에요. 인터넷 연결을 확인해 주세요.";
                            RefreshList();
                            UpdateControls();
                            return;
                        }
                        var loaded = new List<RoomRecord>();
                        foreach (DocumentSnapshot doc in snapshot.Documents)
                            loaded.Add(RoomRecord.FromSnapshot(doc));
                        loaded.Sort((a, b) => b.CreatedAtUtc.CompareTo(a.CreatedAtUtc));
                        rooms.Clear();
                        rooms.AddRange(loaded);
                        serverReady = true;
                        listMessage = "";
                        RefreshList();
                        UpdateControls();
                    }
                    catch (Exception e) { ListFailed(e); }
                });
            await WatchListenerAsync(roomListener, token, uid);
        }
        catch (Exception e)
        {
            if (this != null && isActiveAndEnabled && token == generation) ListFailed(e);
        }
    }

    async System.Threading.Tasks.Task WatchListenerAsync(ListenerRegistration listener, int token, string uid)
    {
        try { await listener.ListenerTask; }
        catch (Exception e)
        {
            if (!IsCurrent(token, uid)) return;
            StopListening();
            ListFailed(e);
        }
    }

    void ListFailed(Exception e)
    {
        serverReady = false;
        rooms.Clear();
        listMessage = "목록 조회 실패. 규칙·연결을 확인하고 전체 또는 새로고침을 눌러 주세요.";
        Debug.LogError($"[방 목록] {e}", this);
        RefreshList();
        UpdateControls();
    }

    bool CanUseRooms()
    {
        if (serverReady && FirebaseManager.IsSignedIn) return true;
        listStatusText.text = "로그인과 서버 목록 조회가 완료되어야 해요.";
        return false;
    }

    void UpdateControls()
    {
        bool available = serverReady && FirebaseManager.IsSignedIn;
        openCreateButton.interactable = available && !creatingRoom && !enteringRoom;
        submitRoomButton.interactable = available && !creatingRoom && !enteringRoom;
        // 전송 중에도 입력값을 바꾸어 다른 방을 저장했다고 오해하지 않도록 합니다.
        roomNameInput.interactable = !creatingRoom;
        categoryDropdown.interactable = !creatingRoom;
        maxPeopleDropdown.interactable = !creatingRoom;
        textChatToggle.interactable = !creatingRoom;
        voiceChatToggle.interactable = !creatingRoom;
        openMapButton.interactable = !creatingRoom;
        createBackButton.interactable = !creatingRoom;
        if (refreshButton != null) refreshButton.interactable = !creatingRoom && !enteringRoom;
    }

    void Show(GameObject panel)
    {
        roomListPanel.SetActive(panel == roomListPanel);
        createRoomPanel.SetActive(panel == createRoomPanel);
        mapSelectPanel.SetActive(panel == mapSelectPanel);
    }

    void OpenCreate()
    {
        if (!CanUseRooms()) return;
        Show(createRoomPanel);
        roomNameInput.text = "";
        createPasswordInput.text = "";
        privateToggle.SetIsOnWithoutNotify(false);
        categoryDropdown.value = 0;
        maxPeopleDropdown.value = 2; // 2명, 4명, 6명, 8명 순서
        textChatToggle.isOn = true;
        voiceChatToggle.isOn = false;
        confirmedMap = "카페";
        UpdateMapLabel();
        UpdatePasswordField(privateToggle.isOn);
        createStatusText.text = "현재 공개 방 생성만 지원해요. 채팅 옵션은 설정만 저장해요.";
    }

    void UpdatePasswordField(bool isPrivate)
    {
        passwordLabel.SetActive(isPrivate);
        createPasswordInput.gameObject.SetActive(isPrivate);
        if (!isPrivate) createPasswordInput.text = "";
    }

    void OpenMaps()
    {
        Show(mapSelectPanel);
        SelectMap(confirmedMap);
    }

    void SelectMap(string map)
    {
        pendingMap = map;
        SetOutline(cafeCard, map == "카페");
        SetOutline(libraryCard, map == "도서관");
        SetOutline(schoolCard, map == "학교");
    }

    void SetOutline(Button card, bool selected)
    {
        var outline = card.GetComponent<UnityEngine.UI.Outline>();
        if (outline == null) return;

        outline.effectColor = selected
            ? new Color32(50, 122, 85, 255)
            : new Color32(215, 220, 216, 255);
        outline.effectDistance = selected
            ? new Vector2(4, -4) : new Vector2(1, -1);
    }

    void ConfirmMap()
    {
        confirmedMap = pendingMap;
        UpdateMapLabel();
        Show(createRoomPanel); // OpenCreate()를 부르면 입력값이 초기화됨
    }

    void UpdateMapLabel()
    {
        mapButtonText.text = $"맵 선택     {confirmedMap}   ›";
    }

    async void CreateRoom()
    {
        if (creatingRoom || enteringRoom || !CanUseRooms()) return;
        if (privateToggle.isOn)
        {
            createStatusText.text = "이번 버전에서는 공개 방을 선택해 주세요.";
            return;
        }
        string title = roomNameInput.text.Trim();
        if (title.Length == 0 || title.Length > 60)
        {
            createStatusText.text = "방 제목을 1~60자로 입력해 주세요.";
            roomNameInput.ActivateInputField();
            return;
        }
        string uid = FirebaseManager.CurrentUserId;
        int token = generation;
        creatingRoom = true;
        UpdateControls();
        createStatusText.text = "서버에 저장 중이에요. 인터넷 연결을 유지해 주세요.";
        try
        {
            string[] categories = { "coding", "language", "certificate" };
            int[] capacities = { 2, 4, 6, 8 };
            string mapId = confirmedMap == "학교" ? "school" : confirmedMap == "도서관" ? "library" : "cafe";
            string id = await RoomFirestoreService.CreateAsync(uid, title,
                categories[categoryDropdown.value], mapId, capacities[maxPeopleDropdown.value],
                textChatToggle.isOn, voiceChatToggle.isOn);
            Debug.Log($"[방 생성] 서버 저장 완료: rooms/{id}");
            if (!IsCurrent(token, uid)) return;
            filter = "전체";
            searchInput.SetTextWithoutNotify("");
            Show(roomListPanel);
            RefreshList();
            listStatusText.text = $"'{title}' 방을 서버에 저장했어요.";
        }
        catch (Exception e)
        {
            Debug.LogError($"[방 생성] {e}");
            if (IsCurrent(token, uid)) createStatusText.text = "저장 실패: " + e.Message;
        }
        finally
        {
            creatingRoom = false;
            if (this != null && configured && isActiveAndEnabled) UpdateControls();
        }
    }

    void SetFilter(string value)
    {
        filter = value;
        if (!serverReady && !creatingRoom && !enteringRoom) RefreshRooms();
        else RefreshList();
    }

    static string CategoryLabel(string id)
    {
        switch (id)
        {
            case "coding": return Coding;
            case "language": return Language;
            case "certificate": return Certificate;
            default: return id;
        }
    }

    void ClearCards()
    {
        foreach (var card in visibleCards)
            if (card != null) { card.SetActive(false); Destroy(card); }
        visibleCards.Clear();
    }

    void RefreshList()
    {
        ClearCards();
        listStatusText.text = listMessage;
        if (!serverReady) return;
        string keyword = searchInput.text.Trim();
        foreach (RoomRecord room in rooms)
        {
            string category = CategoryLabel(room.CategoryId);
            if (filter != "전체" && category != filter) continue;
            if (room.Title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0) continue;
            Button card = Instantiate(roomCardTemplate, roomContent);
            card.name = "RoomCard_" + room.Id;
            foreach (var label in card.GetComponentsInChildren<TMP_Text>(true))
            {
                if (label.name == "RoomTitle") label.text = room.Title;
                if (label.name == "RoomInfo")
                    label.text = $"{category} · 정원 {room.MaxMembers}명 · 공개"
                        + (room.OwnerUid == FirebaseManager.CurrentUserId ? " · 내 방" : "");
            }
            Sprite photo = room.MapId == "school" ? schoolImage
                : room.MapId == "library" ? libraryImage : cafeImage;
            if (photo != null)
                foreach (var img in card.GetComponentsInChildren<Image>(true))
                    if (img.name == "Thumbnail") img.sprite = photo;
            card.onClick.RemoveAllListeners();
            card.onClick.AddListener(() => SelectRoom(room));
            // 템플릿 안에 CloseButton을 추가하면 본인 방에만 종료 버튼을 표시합니다.
            foreach (var button in card.GetComponentsInChildren<Button>(true))
                if (button != card && button.name == "CloseButton")
                {
                    button.gameObject.SetActive(room.OwnerUid == FirebaseManager.CurrentUserId);
                    button.interactable = !closingRoomIds.Contains(room.Id);
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => CloseRoom(room));
                }
            card.gameObject.SetActive(true);
            visibleCards.Add(card.gameObject);
        }
        if (visibleCards.Count == 0) listStatusText.text = "조건에 맞는 스터디룸이 없어요.";
    }

    async void SelectRoom(RoomRecord room)
    {
        if (enteringRoom || creatingRoom || closingRoomIds.Count > 0 || !CanUseRooms()) return;
        enteringRoom = true;
        UpdateControls();
        int token = generation;
        string uid = FirebaseManager.CurrentUserId;
        listStatusText.text = "서버에서 방 상태를 확인하고 있어요.";
        try
        {
            RoomRecord latest = await RoomFirestoreService.GetActiveAsync(uid, room.Id);
            if (!IsCurrent(token, uid)) return;
            string sceneName;
            switch (latest.MapId)
            {
                case "cafe": sceneName = cafeSceneName; break;
                case "library": sceneName = librarySceneName; break;
                case "school": sceneName = schoolSceneName; break;
                default: throw new InvalidOperationException("알 수 없는 맵이에요.");
            }
            sceneName = (sceneName ?? "").Trim();
            if (sceneName.Length == 0 || !Application.CanStreamedLevelBeLoaded(sceneName))
                throw new InvalidOperationException("씬 이름과 Build Profiles의 Scene List를 확인해 주세요: " + sceneName);
            RoomSelectionContext.Select(latest, uid);
            // 씬 이동만 담당합니다. 실제 입장 인원/좌석은 다음 단계에서 등록합니다.
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null) throw new InvalidOperationException("씬 로드를 시작하지 못했어요.");
            while (!operation.isDone)
            {
                await System.Threading.Tasks.Task.Yield();
                if (!IsCurrent(token, uid)) return;
            }
        }
        catch (Exception e)
        {
            if (IsCurrent(token, uid))
            {
                RoomSelectionContext.Clear();
                listStatusText.text = "맵 이동 실패: " + e.Message;
            }
            Debug.LogError($"[방 선택] {e}");
        }
        finally
        {
            enteringRoom = false;
            if (this != null && configured && isActiveAndEnabled) UpdateControls();
        }
    }

    async void CloseRoom(RoomRecord room)
    {
        if (enteringRoom || creatingRoom || !CanUseRooms()) return;
        string uid = FirebaseManager.CurrentUserId;
        if (room.OwnerUid != uid || !closingRoomIds.Add(room.Id)) return;
        int token = generation;
        RefreshList();
        listStatusText.text = "방을 종료하고 있어요.";
        try
        {
            await RoomFirestoreService.CloseAsync(uid, room.Id);
            if (IsCurrent(token, uid)) listStatusText.text = "방을 종료했어요.";
        }
        catch (Exception e)
        {
            Debug.LogError($"[방 종료] {e}");
            if (IsCurrent(token, uid)) listStatusText.text = "종료 실패: " + e.Message;
        }
        finally
        {
            closingRoomIds.Remove(room.Id);
            if (IsCurrent(token, uid))
            {
                // 실패 후에도 종료 버튼을 다시 누를 수 있도록 갱신합니다.
                string message = listStatusText.text;
                RefreshList();
                listStatusText.text = message;
            }
        }
    }
}
