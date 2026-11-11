using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public Toggle publicToggle;
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

    class Room
    {
        public string name, category, map, password;
        public int current, capacity;
        public bool isPrivate, textChat, voiceChat;
    }

    readonly List<Room> rooms = new List<Room>();
    readonly List<GameObject> visibleCards = new List<GameObject>();

    string filter = "전체";
    string confirmedMap = "카페";
    string pendingMap = "카페";
    Room passwordTarget;

    void Start()
    {
        // 목록 화면
        openCreateButton.onClick.AddListener(OpenCreate);
        searchInput.onValueChanged.AddListener(_ => RefreshList());
        allButton.onClick.AddListener(() => SetFilter("전체"));
        codingButton.onClick.AddListener(() => SetFilter("코딩"));
        languageButton.onClick.AddListener(() => SetFilter("어학"));
        certificateButton.onClick.AddListener(() => SetFilter("자격증"));

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
        publicToggle.isOn = true;
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

        string keyword = searchInput.text.Trim();

        foreach (Room room in rooms)
        {
            if (filter != "전체" && room.category != filter) continue;
            if (room.name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            Button card = Instantiate(roomCardTemplate, roomContent);
            card.name = "RoomCard_" + room.name;

            foreach (var label in card.GetComponentsInChildren<TMP_Text>(true))
            {
                if (label.name == "RoomTitle") label.text = room.name;
                if (label.name == "RoomInfo")
                    label.text = $"{room.category} · {room.current}/{room.capacity}명"
                        + (room.isPrivate ? " · 비공개" : "");
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

        if (room.isPrivate && joinPasswordInput.text != room.password)
        {
            listStatusText.text = "비밀번호가 일치하지 않아요.";
            joinPasswordInput.ActivateInputField();
            return;
        }

        ClearPasswordTarget();
        listStatusText.text = $"입장 확인: {room.name} / {room.map} 맵";
        // 팀의 실제 입장 기능은 이 위치에 연결하면 됩니다.
    }
}