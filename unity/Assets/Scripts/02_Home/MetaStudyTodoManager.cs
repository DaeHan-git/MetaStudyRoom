using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MetaStudyTodoManager : MonoBehaviour
{
    [Serializable]
    public class TodoData
    {
        public string text;
        public bool done;
    }

    [Serializable]
    public class TodoList
    {
        public List<TodoData> items = new List<TodoData>();
    }

    public RectTransform content;
    public TMP_FontAsset font;
    public Button editButton;
    public TMP_Text editLabel;
    public GameObject addPanel;
    public TMP_InputField addInput;
    public Button addButton;

    private TodoList data = new TodoList();
    private bool editing;
    private const string SaveKey = "MetaStudy_Todo_v1";

    private Color green = new Color32(66, 115, 88, 255);
    private Color gray = new Color32(120, 134, 145, 255);

    void Start()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            data = JsonUtility.FromJson<TodoList>(
                PlayerPrefs.GetString(SaveKey)
            ) ?? new TodoList();
        }

        editButton.onClick.AddListener(ToggleEdit);
        addButton.onClick.AddListener(AddTodo);

        Refresh();
    }

    void Save()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    void ToggleEdit()
    {
        editing = !editing;
        Save();
        Refresh();
    }

    void AddTodo()
    {
        string value = addInput.text.Trim();

        if (string.IsNullOrEmpty(value))
            return;

        data.items.Add(new TodoData
        {
            text = value,
            done = false
        });

        addInput.text = "";
        Save();
        Refresh();
    }

    void Refresh()
    {
        editLabel.text = editing ? "완료" : "+";
        addPanel.SetActive(editing);

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            GameObject child = content.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }

        foreach (TodoData item in data.items)
            CreateRow(item);
    }

    void CreateRow(TodoData item)
    {
        GameObject row = new GameObject(
            "TodoItem", typeof(RectTransform),
            typeof(HorizontalLayoutGroup),
            typeof(LayoutElement)
        );

        row.transform.SetParent(content, false);
        row.GetComponent<LayoutElement>().preferredHeight = 48;

        HorizontalLayoutGroup layout =
            row.GetComponent<HorizontalLayoutGroup>();

        layout.spacing = 10;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        Button check = MakeButton(
            row.transform, item.done ? "●" : "○",
            36, item.done ? green : gray
        );

        check.onClick.AddListener(() =>
        {
            item.done = !item.done;
            Save();
            check.GetComponentInChildren<TMP_Text>().text =
                item.done ? "●" : "○";
            check.GetComponentInChildren<TMP_Text>().color =
                item.done ? green : gray;
        });

        if (editing)
        {
            TMP_InputField field = MakeInput(
                row.transform, font, item.text
            );

            field.onValueChanged.AddListener(value =>
            {
                item.text = value;
                Save();
            });

            Button remove = MakeButton(
                row.transform, "×", 36,
                new Color32(170, 90, 90, 255)
            );

            remove.onClick.AddListener(() =>
            {
                data.items.Remove(item);
                Save();
                Refresh();
            });
        }
        else
        {
            TMP_Text label = MakeText(
                row.transform, item.text, font, 18, gray
            );

            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Ellipsis;

            LayoutElement element =
                label.gameObject.AddComponent<LayoutElement>();

            element.flexibleWidth = 1;
            element.minWidth = 0;
        }
    }

    Button MakeButton(
        Transform parent, string text, float width, Color color)
    {
        GameObject go = new GameObject(
            "Button", typeof(RectTransform),
            typeof(Image), typeof(Button), typeof(LayoutElement)
        );

        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = Color.clear;

        LayoutElement element = go.GetComponent<LayoutElement>();
        element.preferredWidth = width;
        element.preferredHeight = 42;

        TMP_Text label = MakeText(go.transform, text, font, 30, color);
        label.alignment = TextAlignmentOptions.Center;
        Stretch(label.rectTransform);

        return go.GetComponent<Button>();
    }

    public static TMP_Text MakeText(
        Transform parent, string text, TMP_FontAsset font,
        float size, Color color)
    {
        GameObject go = new GameObject(
            "Text", typeof(RectTransform), typeof(TextMeshProUGUI)
        );

        go.transform.SetParent(parent, false);

        TMP_Text label = go.GetComponent<TMP_Text>();
        label.font = font;
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.raycastTarget = false;

        return label;
    }

    public static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public static TMP_InputField MakeInput(
        Transform parent, TMP_FontAsset font, string value)
    {
        GameObject go = new GameObject(
            "InputField", typeof(RectTransform),
            typeof(Image), typeof(TMP_InputField),
            typeof(LayoutElement)
        );

        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color =
            new Color32(240, 244, 241, 255);

        LayoutElement element = go.GetComponent<LayoutElement>();
        element.flexibleWidth = 1;
        element.preferredHeight = 42;
        element.minWidth = 0;

        GameObject area = new GameObject(
            "Text Area", typeof(RectTransform), typeof(RectMask2D)
        );

        area.transform.SetParent(go.transform, false);

        RectTransform rect = area.GetComponent<RectTransform>();
        Stretch(rect);
        rect.offsetMin = new Vector2(8, 0);
        rect.offsetMax = new Vector2(-8, 0);

        TMP_Text label = MakeText(
            area.transform, "", font, 18, Color.black
        );

        TMP_Text placeholder = MakeText(
            area.transform, "할 일 입력", font, 18, Color.gray
        );

        Stretch(label.rectTransform);
        Stretch(placeholder.rectTransform);

        TMP_InputField field = go.GetComponent<TMP_InputField>();
        field.textViewport = rect;
        field.textComponent = label;
        field.placeholder = placeholder;
        field.lineType = TMP_InputField.LineType.SingleLine;
        field.text = value;

        return field;
    }
}
