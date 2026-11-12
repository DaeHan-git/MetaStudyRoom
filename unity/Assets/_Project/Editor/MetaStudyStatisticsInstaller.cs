#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class MetaStudyStatisticsInstaller
{
    const string Folder = "Assets/_Project/Scenes/02_Home/";
    [MenuItem("MetaStudy/Install Dynamic Study Statistics (Keep Existing UI)")]
    public static void Install()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("중단", "Play 모드를 종료한 뒤 실행하세요.", "확인");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        string original = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
        foreach (string name in new[] { "Home", "Statistics" })
        {
            string path = Folder + name + ".unity";
            if (AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(path) == null)
            {
                Debug.LogError("Scene not found: " + path);
                continue;
            }
            var scene = EditorSceneManager.OpenScene(path);
            var page = GameObject.Find("Canvas/Page");
            if (page == null) { Debug.LogError("Canvas/Page missing in " + path); continue; }
            var host = GameObject.Find("UIActions");
            if (host == null) host = new GameObject("UIActions");
            var binder = host.GetComponent<MetaStudyStatisticsBinder>();
            if (binder == null) binder = host.AddComponent<MetaStudyStatisticsBinder>();
            binder.isHomeScene = name == "Home";
            binder.isStatisticsScene = name == "Statistics";
            if (name == "Statistics")
            {
                var monthLabel = page.transform.Find("Month")?.GetComponent<TMP_Text>();
                if (monthLabel != null)
                {
                    AddMonthButton(page.transform, "PrevMonth", "‹", 33, monthLabel.font, binder, true);
                    AddMonthButton(page.transform, "NextMonth", "›", 290, monthLabel.font, binder, false);
                }
                for (int i = 0; i < 42; i++)
                {
                    var dot = page.transform.Find("StudyDot" + i)?.GetComponent<Image>();
                    if (dot != null) dot.raycastTarget = false;
                }
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Dynamic statistics installed: " + path);
        }
        if (!string.IsNullOrEmpty(original)) EditorSceneManager.OpenScene(original);
        else EditorSceneManager.OpenScene(Folder + "Home.unity");
        EditorUtility.DisplayDialog("완료", "Home / Statistics에 동적 학습 기록 기능을 연결했습니다. 기존 UI는 유지됩니다.", "확인");
    }
    static void AddMonthButton(Transform page, string name, string text, float x, TMP_FontAsset font, MetaStudyStatisticsBinder binder, bool previous)
    {
        var existing = page.Find(name);
        var go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        if (existing == null) go.transform.SetParent(page, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -237);
        rect.sizeDelta = new Vector2(36, 38);
        var image = go.GetComponent<Image>();
        if (image == null) image = go.AddComponent<Image>();
        image.color = new Color(1, 1, 1, 0.9f);
        var button = go.GetComponent<Button>();
        if (button == null) button = go.AddComponent<Button>();
        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            UnityEventTools.RemovePersistentListener(button.onClick, i);
        }
        button.onClick.RemoveAllListeners();
        if (previous) UnityEventTools.AddPersistentListener(button.onClick, binder.PreviousMonth);
        else UnityEventTools.AddPersistentListener(button.onClick, binder.NextMonth);
        EditorUtility.SetDirty(button);
        var child = go.transform.Find("Label");
        GameObject label = child != null ? child.gameObject : new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        if (child == null) label.transform.SetParent(go.transform, false);
        var r = label.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        var tmp = label.GetComponent<TextMeshProUGUI>();
        tmp.font = font; tmp.text = text; tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color32(23, 34, 56, 255);
        tmp.raycastTarget = false;
    }
}
#endif
