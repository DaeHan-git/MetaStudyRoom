using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
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

    const string Coding = "코딩", Language = "어학", Certificate = "자격증";
    bool enteringRoom;

    // 이전 버전에서 드롭다운에 사용하던 프로그래밍 표기도 같은 탭에서 찾는다.
    static string NormalizeCategory(string category)
    {
        string label = (category ?? "").Trim();
        return label.Replace(" ", "") == "IT/프로그래밍" ? Coding : label;
    }

    [Serializable]
    class Room
    {
        public string name, category, map;
        public int current, capacity;
        public bool isPrivate, textChat, voiceChat;

        // 비공개 방은 비밀번호 원문 대신 확인용 값만 저장해요.
        public string passwordSalt;
        public string passwordHash;

        // 기존 CreateRoom()의 password = ... 코드를 그대로 사용할 수 있어요.
        public string password
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    passwordSalt = "";
                    passwordHash = "";
                    return;
                }

                byte[] salt = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                    rng.GetBytes(salt);

                passwordSalt = Convert.ToBase64String(salt);
                passwordHash = MakeHash(value, salt);
            }
        }

        public bool MatchesPassword(string input)
        {
            if (string.IsNullOrEmpty(passwordSalt) ||
                string.IsNullOrEmpty(passwordHash))
                return false;

            byte[] salt = Convert.FromBase64String(passwordSalt);
            return MakeHash(input, salt) == passwordHash;
        }

        static string MakeHash(string input, byte[] salt)
        {
            using (var key = new Rfc2898DeriveBytes(
                input, salt, 100000, HashAlgorithmName.SHA256))
            {
                return Convert.ToBase64String(key.GetBytes(32));
            }
        }
    }

    [Serializable]
    class RoomSaveData
    {
        public List<Room> rooms;
    }

    readonly List<Room> rooms = new List<Room>();
    readonly List<GameObject> visibleCards = new List<GameObject>();

    string filter = "전체";
    string confirmedMap = "카페";
    string pendingMap = "카페";
    Room passwordTarget;

    string RoomSavePath =>
        Path.Combine(Application.persistentDataPath, "study_rooms.json");

    bool roomLoadFailed;

    bool SaveRooms()
    {
        try
        {
            var data = new RoomSaveData { rooms = rooms };
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(RoomSavePath, json);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"방 목록 저장 실패: {e.Message}");
            return false;
        }
    }

    void LoadRooms()
    {
        rooms.Clear();
        roomLoadFailed = false;

        // 처음 실행한 경우에만 예시 방을 넣어요.
        if (!File.Exists(RoomSavePath))
        {
            AddExampleRooms();
            return;
        }

        try
        {
            string json = File.ReadAllText(RoomSavePath);
            var data = JsonUtility.FromJson<RoomSaveData>(json);

            if (data == null || data.rooms == null)
                throw new InvalidDataException("방 목록 형식이 올바르지 않습니다.");

            rooms.AddRange(data.rooms);
        }
        catch (Exception e)
        {
            // 읽지 못한 파일을 빈 목록으로 덮어쓰지 않도록 해요.
            roomLoadFailed = true;
            Debug.LogError($"방 목록 불러오기 실패: {RoomSavePath}\n{e.Message}");
        }
    }

    void AddExampleRooms()
    {
        rooms.Add(new Room
        {
            name = "코딩 집중방",
            category = "코딩",
            map = "카페",
            current = 3,
            capacity = 6
        });

        rooms.Add(new Room
        {
            name = "조용한 도서관",
            category = "자격증",
            map = "도서관",
            current = 5,
            capacity = 8
        });

        rooms.Add(new Room
        {
            name = "우리끼리 스터디",
            category = "어학",
            map = "학교",
            current = 1,
            capacity = 4,
            isPrivate = true,
            password = "1234"
        });
    }
    void Start()
    {
        categoryDropdown.ClearOptions();
        categoryDropdown.AddOptions(new List<string> { Coding, Language, Certificate });
        privateToggle.group = null;

        // 목록 화면
        openCreateButton.onClick.AddListener(OpenCreate);
        searchInput.onValueChanged.AddListener(_ => RefreshList());
        allButton.onClick.AddListener(() => SetFilter("전체"));
        codingButton.onClick.AddListener(() => SetFilter(Coding));
        languageButton.onClick.AddListener(() => SetFilter(Language));
        certificateButton.onClick.AddListener(() => SetFilter(Certificate));

        // 방 만들기 화면
        createBackButton.onClick.AddListener(() => Show(roomListPanel));
        privateToggle.onValueChanged.AddListener(UpdatePasswordField);
        openMapButton.onClick.AddListener(OpenMaps);
        submitRoomButton.onClick.AddListener(CreateRoom);

        // 맵 선택 화면
        cafeCard.onClick.AddListener(() => SelectMap("카페"));
        libraryCard.onClick.AddListener(() => SelectMap("도서관"));
        schoolCard.onClick.AddListener(() => SelectMap("학교"));
        mapBackButton.onClick.AddListener(() => Show(createRoomPanel));
        confirmMapButton.onClick.AddListener(ConfirmMap);

        roomCardTemplate.gameObject.SetActive(false);
        joinPasswordInput.gameObject.SetActive(false);
        listStatusText.text = "";

        LoadRooms();
        if (roomLoadFailed) openCreateButton.interactable = false;

        Show(roomListPanel);
        RefreshList();
    }

    void Show(GameObject panel)
    {
        roomListPanel.SetActive(panel == roomListPanel);
        createRoomPanel.SetActive(panel == createRoomPanel);
        mapSelectPanel.SetActive(panel == mapSelectPanel);
    }

    void OpenCreate()
    {
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
        createStatusText.text = "";
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

    void CreateRoom()
    {
        string name = roomNameInput.text.Trim();
        if (name.Length == 0)
        {
            createStatusText.text = "방 이름을 입력해 주세요.";
            roomNameInput.ActivateInputField();
            return;
        }

        if (privateToggle.isOn &&
            string.IsNullOrWhiteSpace(createPasswordInput.text))
        {
            createStatusText.text = "비공개 방의 비밀번호를 입력해 주세요.";
            createPasswordInput.ActivateInputField();
            return;
        }

        int[] capacities = { 2, 4, 6, 8 };
        int selectedIndex = maxPeopleDropdown.value;
        int capacity = capacities[selectedIndex];

        rooms.Insert(0, new Room
        {
            name = name,
            category = categoryDropdown.options[categoryDropdown.value].text,
            map = confirmedMap,
            current = 1,
            capacity = capacity,
            isPrivate = privateToggle.isOn,
            password = privateToggle.isOn ? createPasswordInput.text : "",
            textChat = textChatToggle.isOn,
            voiceChat = voiceChatToggle.isOn
        });

        if (!SaveRooms())
        {
            rooms.RemoveAt(0);
            createStatusText.text = "방을 저장하지 못했어요. 다시 시도해 주세요.";
            return;
        }

        filter = "전체";
        searchInput.SetTextWithoutNotify("");
        ClearPasswordTarget();
        Show(roomListPanel);
        RefreshList();
        listStatusText.text = $"'{name}' 방을 만들었어요.";
    }

    void SetFilter(string value)
    {
        filter = value;
        RefreshList();
    }

    void RefreshList()
    {
        ClearPasswordTarget();
        listStatusText.text = "";

        foreach (var card in visibleCards)
        {
            card.SetActive(false);
            Destroy(card);
        }
        visibleCards.Clear();

        if (roomLoadFailed)
        {
            listStatusText.text = "방 목록을 불러오지 못했어요. Console을 확인해 주세요.";
            return;
        }

        string keyword = searchInput.text.Trim();

        foreach (Room room in rooms)
        {
            string category = NormalizeCategory(room.category);
            if (filter != "전체" && category != filter) continue;
            if (room.name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            Button card = Instantiate(roomCardTemplate, roomContent);
            card.name = "RoomCard_" + room.name;

            foreach (var label in card.GetComponentsInChildren<TMP_Text>(true))
            {
                if (label.name == "RoomTitle") label.text = room.name;
                if (label.name == "RoomInfo")
                    label.text = $"{category} · {room.current}/{room.capacity}명"
                        + (room.isPrivate ? " · 비공개" : " · 공개");
            }

            Sprite photo = room.map == "학교" ? schoolImage
                         : room.map == "도서관" ? libraryImage : cafeImage;
            if (photo != null)
            {
                foreach (var img in card.GetComponentsInChildren<Image>(true))
                    if (img.name == "Thumbnail") img.sprite = photo;
            }

            card.onClick.AddListener(() => SelectRoom(room));
            card.gameObject.SetActive(true);
            visibleCards.Add(card.gameObject);
        }

        if (visibleCards.Count == 0)
            listStatusText.text = "조건에 맞는 스터디룸이 없어요.";
    }

    void ClearPasswordTarget()
    {
        passwordTarget = null;
        joinPasswordInput.SetTextWithoutNotify("");
        joinPasswordInput.gameObject.SetActive(false);
    }

    void SelectRoom(Room room)
    {
        if (enteringRoom) return;

        if (room.current >= room.capacity)
        {
            ClearPasswordTarget();
            listStatusText.text = "이미 인원이 가득 찬 방이에요.";
            return;
        }

        if (room.isPrivate && passwordTarget != room)
        {
            passwordTarget = room;
            joinPasswordInput.SetTextWithoutNotify("");
            joinPasswordInput.gameObject.SetActive(true);
            joinPasswordInput.ActivateInputField();
            listStatusText.text = "비밀번호를 입력한 뒤 같은 방을 다시 눌러 주세요.";
            return;
        }

        if (room.isPrivate && !room.MatchesPassword(joinPasswordInput.text))
        {
            listStatusText.text = "비밀번호가 일치하지 않아요.";
            joinPasswordInput.ActivateInputField();
            return;
        }

        ClearPasswordTarget();

        string sceneName;
        switch (room.map)
        {
            case "카페": sceneName = cafeSceneName; break;
            case "도서관": sceneName = librarySceneName; break;
            case "학교": sceneName = schoolSceneName; break;
            default:
                listStatusText.text = $"알 수 없는 맵이에요: {room.map}";
                return;
        }

        sceneName = (sceneName ?? "").Trim();
        if (sceneName.Length == 0 || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            listStatusText.text = $"{room.map} 씬 이름과 Scene List 등록을 확인해 주세요: {sceneName}";
            return;
        }

        enteringRoom = true;
        listStatusText.text = $"{room.map}에 입장하는 중이에요...";
        SceneManager.LoadSceneAsync(sceneName);
    }
}
