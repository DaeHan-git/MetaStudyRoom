using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class MetaStudyTodoInstaller
{
    static Color green = new Color32(66, 115, 88, 255);

    [MenuItem("MetaStudy/Install Todo List")]
    public static void Install()
    {
        if (Application.isPlaying)
            return;

        GameObject card = GameObject.Find("Canvas/Page/TodoCard");

        if (card == null)
        {
            Debug.LogError("Home 씬의 TodoCard를 찾을 수 없습니다.");
            return;
        }

        Transform title = card.transform.Find("TodoTitle");

        if (title == null || title.GetComponent<TMP_Text>() == null)
        {
            Debug.LogError("TodoTitle의 TMP 폰트를 확인해주세요.");
            return;
        }

        TMP_FontAsset font = title.GetComponent<TMP_Text>().font;

        Undo.RegisterFullObjectHierarchyUndo(card, "Install Todo");

        Transform previous = card.transform.Find("TodoDynamicUI");

        if (previous != null)
            Undo.DestroyObjectImmediate(previous.gameObject);

        foreach (Transform child in card.transform)
        {
            if (child.name.StartsWith("Todo_"))
                child.gameObject.SetActive(false);
        }

        MetaStudyTodoManager manager =
            card.GetComponent<MetaStudyTodoManager>();

        if (manager == null)
            manager = Undo.AddComponent<MetaStudyTodoManager>(card);

        manager.font = font;

        RectTransform root = CreateRect(
            "TodoDynamicUI", card.transform
        );

        MetaStudyTodoManager.Stretch(root);

        RectTransform edit = CreateRect("EditButton", root);
        edit.anchorMin = edit.anchorMax = new Vector2(1, 1);
        edit.pivot = new Vector2(1, 1);
        edit.sizeDelta = new Vector2(100, 50);
        edit.anchoredPosition = new Vector2(-18, -12);

        edit.gameObject.AddComponent<Image>().color = Color.clear;
        manager.editButton = edit.gameObject.AddComponent<Button>();

        manager.editLabel = MetaStudyTodoManager.MakeText(
            edit, "+", font, 28, green
        );

        manager.editLabel.alignment = TextAlignmentOptions.Center;
        MetaStudyTodoManager.Stretch(manager.editLabel.rectTransform);

        RectTransform scroll = CreateRect("Scroll View", root);
        MetaStudyTodoManager.Stretch(scroll);
        scroll.offsetMin = new Vector2(20, 65);
        scroll.offsetMax = new Vector2(-20, -65);

        ScrollRect scrollRect = scroll.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 30;

        RectTransform viewport = CreateRect("Viewport", scroll);
        MetaStudyTodoManager.Stretch(viewport);
        viewport.gameObject.AddComponent<Image>().color = Color.clear;
        viewport.gameObject.AddComponent<RectMask2D>();

        RectTransform content = CreateRect("Content", viewport);
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(1, 1);
        content.pivot = new Vector2(0.5f, 1);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        VerticalLayoutGroup group =
            content.gameObject.AddComponent<VerticalLayoutGroup>();

        group.spacing = 5;
        group.childControlHeight = true;
        group.childControlWidth = true;
        group.childForceExpandHeight = false;
        group.childForceExpandWidth = true;

        ContentSizeFitter fitter =
            content.gameObject.AddComponent<ContentSizeFitter>();

        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewport;
        scrollRect.content = content;
        manager.content = content;

        RectTransform panel = CreateRect("AddPanel", root);
        panel.anchorMin = new Vector2(0, 0);
        panel.anchorMax = new Vector2(1, 0);
        panel.pivot = new Vector2(0.5f, 0);
        panel.offsetMin = new Vector2(20, 12);
        panel.offsetMax = new Vector2(-20, 57);

        HorizontalLayoutGroup panelLayout =
            panel.gameObject.AddComponent<HorizontalLayoutGroup>();

        panelLayout.spacing = 8;
        panelLayout.childControlWidth = true;
        panelLayout.childControlHeight = true;
        panelLayout.childForceExpandWidth = false;
        panelLayout.childForceExpandHeight = true;

        manager.addPanel = panel.gameObject;

        manager.addInput = MetaStudyTodoManager.MakeInput(
            panel, font, ""
        );

        RectTransform add = CreateRect("AddButton", panel);
        add.gameObject.AddComponent<Image>().color = green;

        LayoutElement addLayout =
            add.gameObject.AddComponent<LayoutElement>();

        addLayout.preferredWidth = 75;
        manager.addButton = add.gameObject.AddComponent<Button>();

        TMP_Text addLabel = MetaStudyTodoManager.MakeText(
            add, "추가", font, 22, Color.white
        );

        addLabel.alignment = TextAlignmentOptions.Center;
        MetaStudyTodoManager.Stretch(addLabel.rectTransform);

        panel.gameObject.SetActive(false);

        EditorUtility.SetDirty(manager);
        EditorSceneManager.MarkSceneDirty(card.scene);
        EditorSceneManager.SaveScene(card.scene);

        Debug.Log("MetaStudy ToDo 설치 완료!");
    }

    static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }
}
