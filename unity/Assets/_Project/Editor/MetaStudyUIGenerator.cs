#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class MetaStudyUIGenerator
{
    const string SceneFolder = "Assets/_Project/Scenes/02_Home";
    static readonly Color Ink = Hex("#172238"), Muted = Hex("#7D8A92"), Green = Hex("#487C60"), Pale = Hex("#E9F3EB"), Border = Hex("#E4E9E8"), White = Color.white;
    static TMP_FontAsset font;
    static MetaStudyUIActions actions;
    static GameObject canvas;

    [MenuItem("MetaStudy/Generate 2번 UI (Home · Statistics · Ranking)")]
    public static void Generate()
    {
        Directory.CreateDirectory(SceneFolder);
        font = FindKoreanFont();
        if (font == null) { EditorUtility.DisplayDialog("폰트 없음", "NotoSansKR-Regular SDF.asset 파일을 확인하세요.", "확인"); return; }
        if (!PrepareFont(font)) return;
        BuildHome(); BuildStatistics(); BuildRanking();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(SceneFolder + "/Home.unity");
        EditorUtility.DisplayDialog("생성 완료", "Home / Statistics / Ranking 씬이 생성되었습니다. File > Build Profiles의 Scene List에 세 씬을 등록하세요.", "확인");
    }
    // Existing scenes can be fixed without regenerating their UI.
    [MenuItem("MetaStudy/Fix Korean Font in Existing UI")]
    public static void FixExistingKoreanText()
    {
        font = FindKoreanFont();
        if (font == null)
        {
            EditorUtility.DisplayDialog("폰트 없음", "Assets/_Project/Fonts/NotoSansKR-Regular SDF.asset 파일을 확인하세요.", "확인");
            return;
        }
        if (!PrepareFont(font)) return;
        int changed = 0;
        string[] sceneNames = { "Home", "Statistics", "Ranking" };
        foreach (string sceneName in sceneNames)
        {
            string path = SceneFolder + "/" + sceneName + ".unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) continue;
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            foreach (var root in scene.GetRootGameObjects())
            foreach (var textComponent in root.GetComponentsInChildren<TMP_Text>(true))
            {
                textComponent.font = font;
                textComponent.fontSharedMaterial = font.material;
                textComponent.SetAllDirty();
                EditorUtility.SetDirty(textComponent);
                changed++;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        EditorSceneManager.OpenScene(SceneFolder + "/Home.unity");
        EditorUtility.DisplayDialog("한글 폰트 적용", changed + "개 TMP 텍스트에 폰트를 지정했습니다. Game 탭에서 확인하세요.", "확인");
    }

    static bool PrepareFont(TMP_FontAsset asset)
    {
        if (asset.atlasPopulationMode != AtlasPopulationMode.Dynamic)
        {
            if (asset.sourceFontFile == null)
            {
                EditorUtility.DisplayDialog("폰트 설정 확인 필요", "TMP 폰트가 Static이고 Source Font File이 비어 있습니다. NotoSansKR-Regular SDF 에셋 Inspector에서 Atlas Population Mode를 Dynamic으로 설정하고 Source Font File에 NotoSansKR-Regular.ttf를 지정하세요.", "확인");
                return false;
            }
            asset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }
        if (asset.sourceFontFile == null)
        {
            EditorUtility.DisplayDialog("폰트 원본 확인 필요", "TMP 폰트의 Source Font File이 비어 있습니다. Inspector에서 NotoSansKR-Regular.ttf를 연결해야 한글 글리프가 동적으로 생성됩니다.", "확인");
            return false;
        }
        return true;
    }

    static TMP_FontAsset FindKoreanFont()
    {
        // Prefer the exact font asset supplied for this project.
        const string fontPath = "Assets/_Project/Fonts/NotoSansKR-Regular SDF.asset";
        var koreanFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
        if (koreanFont != null) return koreanFont;

        // Fallback to another Korean TMP font asset, if available.
        foreach (string guid in AssetDatabase.FindAssets("t:TMP_FontAsset"))
        {
            var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
            if (asset != null && (asset.name.Contains("NotoSansKR") || asset.name.Contains("Pretendard") || asset.name.Contains("Nanum")))
                return asset;
        }
        return null;
    }
    [MenuItem("MetaStudy/Fix Input Modules in Existing UI")]
    public static void FixExistingInputModules()
    {
        string[] names = { "Home", "Statistics", "Ranking" };
        int fixedCount = 0;
        foreach (string name in names)
        {
            string path = SceneFolder + "/" + name + ".unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) continue;
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            foreach (var root in scene.GetRootGameObjects())
            {
                var es = root.GetComponent<UnityEngine.EventSystems.EventSystem>();
                if (es == null) continue;
                var oldModule = root.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                if (oldModule != null) UnityEngine.Object.DestroyImmediate(oldModule);
                if (root.GetComponent<InputSystemUIInputModule>() == null)
                    root.AddComponent<InputSystemUIInputModule>();
                fixedCount++;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        EditorSceneManager.OpenScene(SceneFolder + "/Home.unity");
        EditorUtility.DisplayDialog("입력 모듈 수정", fixedCount + "개 Scene의 EventSystem을 확인했습니다.", "확인");
    }

    static void StartScene(string name)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camera.tag = "MainCamera";
        camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        camera.GetComponent<Camera>().backgroundColor = Hex("#E6E6E6");
        canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(390, 844);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(InputSystemUIInputModule));
        actions = new GameObject("UIActions", typeof(MetaStudyUIActions)).GetComponent<MetaStudyUIActions>();
        Panel("ScreenBackground", canvas.transform, 0, 0, 390, 844, Hex("#E6E6E6"));
        Panel("Page", canvas.transform, 16, 12, 358, 820, White);
    }
    static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0, 1); r.anchorMax = new Vector2(0, 1);
        r.pivot = new Vector2(0, 1);
        r.anchoredPosition = new Vector2(x, -y);
        r.sizeDelta = new Vector2(w, h);
        return r;
    }
    static GameObject Panel(string name, Transform parent, float x, float y, float w, float h, Color color)
    {
        var r = Rect(name, parent, x, y, w, h);
        var img = r.gameObject.AddComponent<Image>(); img.color = color;
        return r.gameObject;
    }
    static TMP_Text Text(string name, Transform parent, string value, float x, float y, float w, float h, int size = 16, Color? color = null, bool bold = false, TextAlignmentOptions align = TextAlignmentOptions.Left)
    {
        var r = Rect(name, parent, x, y, w, h);
        var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        if (font) t.font = font;
        t.text = value; t.fontSize = size; t.color = color ?? Ink;
        t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        t.alignment = align; t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Ellipsis;
        return t;
    }
    static Button Button(string name, Transform parent, string value, float x, float y, float w, float h, Color bg, Color fg, Action<Button> setup = null, int size = 15)
    {
        var p = Panel(name, parent, x, y, w, h, bg);
        var b = p.AddComponent<Button>();
        Text("Label", p.transform, value, 10, 0, w - 20, h, size, fg, true, TextAlignmentOptions.Center);
        setup?.Invoke(b);
        return b;
    }
    static void Line(Transform parent, float x, float y, float w) => Panel("Divider", parent, x, y, w, 1, Border);
    static void Header(Transform page, string title, Action<Button> back)
    {
        Button("Back", page, "‹", 15, 17, 34, 42, White, Ink, back, 28);
        Text("PageTitle", page, title, 59, 24, 275, 30, 17, Ink, true);
        Line(page, 18, 70, 322);
    }
    static Transform Page => canvas.transform.Find("Page");
    static void Save(string name)
    {
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), SceneFolder + "/" + name + ".unity");
    }
    static void BuildHome()
    {
        StartScene("Home"); var page = Page;
        Header(page, "홈", b => UnityEventTools.AddPersistentListener(b.onClick, new UnityAction(actions.OpenHome)));
        Button("StatisticsShortcut", page, "학습 통계·캘린더   ›", 20, 94, 318, 73, Pale, Ink, b => UnityEventTools.AddPersistentListener(b.onClick, new UnityAction(actions.OpenStatistics)));
        var study = Panel("TodayStudyCard", page, 20, 187, 318, 178, White).transform;
        Text("Caption", study, "오늘의 공부", 15, 15, 240, 24, 13, Ink, true);
        Text("Time", study, "02:15:00", 15, 47, 280, 51, 34, Ink, true);
        Text("Goal", study, "목표 3시간 · 73% 달성", 15, 106, 270, 23, 13, Muted);
        Panel("ProgressTrack", study, 15, 141, 286, 9, Border);
        Panel("ProgressFill", study, 15, 141, 209, 9, Green);
        var todo = Panel("TodoCard", page, 20, 382, 318, 228, White).transform;
        Text("TodoTitle", todo, "ToDo", 15, 14, 200, 26, 15, Ink, true);
        string[] tasks = {"자바 ch03 연습문제", "자바 ch04 연습문제", "고급모바일프로그래밍 9주차 강의 듣기", "네트워크프로그래밍 9주차 강의 듣기"};
        for (int i = 0; i < tasks.Length; i++)
        {
            int index = i;
            var row = Rect("Todo_" + (i + 1), todo, 15, 48 + i * 33, 290, 30);
            var toggleObj = new GameObject("Check", typeof(RectTransform), typeof(Toggle));
            toggleObj.transform.SetParent(row, false);
            var tr = toggleObj.GetComponent<RectTransform>(); tr.anchorMin = tr.anchorMax = new Vector2(0, .5f); tr.pivot = new Vector2(0, .5f); tr.anchoredPosition = Vector2.zero; tr.sizeDelta = new Vector2(20, 20);
            var bg = Panel("Circle", toggleObj.transform, 0, 0, 17, 17, Border).GetComponent<Image>();
            var check = Panel("Checkmark", bg.transform, 4, 4, 9, 9, Green).GetComponent<Image>();
            var toggle = toggleObj.GetComponent<Toggle>(); toggle.targetGraphic = bg; toggle.graphic = check; toggle.isOn = i == 0 || i == 2;
            toggleObj.AddComponent<MetaStudyTodoBinding>();
            Text("Task", row, tasks[index], 25, 1, 263, 27, 12, Muted);
        }
        Button("EnterStudyRoom", page, "▶  스터디룸 입장", 30, 744, 298, 50, Green, White, b => UnityEventTools.AddPersistentListener(b.onClick, new UnityAction(actions.EnterStudyRoom)));
        Save("Home");
    }
    static void BuildStatistics()
    {
        StartScene("Statistics"); var page = Page;
        Header(page, "학습 통계·캘린더", b => UnityEventTools.AddPersistentListener(b.onClick, new UnityAction(actions.OpenHome)));
        var summary = Panel("Summary", page, 20, 103, 318, 115, Pale).transform;
        Text("Today", summary, "오늘 2시간 15분", 17, 24, 280, 33, 21, Green, true);
        Text("Week", summary, "이번 주 12시간 30분", 17, 66, 280, 22, 12, Muted);
        Text("Month", page, "2024년 5월", 20, 239, 318, 35, 20, Ink, true, TextAlignmentOptions.Center);
        string[] weekdays = {"일", "월", "화", "수", "목", "금", "토"};
        for (int i = 0; i < 7; i++) Text("Weekday" + i, page, weekdays[i], 31 + i * 43, 282, 35, 24, 12, Muted, false, TextAlignmentOptions.Center);
        int[] days = {28,29,30,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,1,2,3,4,5,6,7,8};
        int[] marked = {3,6,9,12,16,19,23,26,30,33,37,40};
        for (int i = 0; i < days.Length; i++)
        {
            int col = i % 7, row = i / 7;
            float x = 31 + col * 43, y = 310 + row * 48;
            Text("Day" + i, page, days[i].ToString(), x, y, 35, 22, 12, Ink, false, TextAlignmentOptions.Center);
            if (Array.IndexOf(marked, i) >= 0) Panel("StudyDot" + i, page, x + 15, y + 23, 5, 5, Green);
        }
        var selected = Panel("SelectedDate", page, 27, 616, 304, 62, White).transform;
        Text("DateRecord", selected, "5월 21일 (화)     2시간 15분", 13, 19, 285, 25, 13, Ink, true);
        Text("WeeklyTitle", page, "이번 주 통계", 27, 696, 250, 24, 13, Ink, true);
        Panel("ChartBackground", page, 27, 724, 304, 70, Hex("#F5F7F6"));
        int[] bars = {5,20,38,53,18,34,48};
        for (int i = 0; i < bars.Length; i++) Panel("Bar" + i, page, 47 + i * 40, 784 - bars[i], 23, bars[i], Green);
        Save("Statistics");
    }
    static void BuildRanking()
    {
        StartScene("Ranking"); var page = Page;
        Header(page, "스터디 그룹 랭킹", b => UnityEventTools.AddPersistentListener(b.onClick, new UnityAction(actions.OpenHome)));
        Button("GroupDropdown", page, "♧  코딩 집중방      ▾", 28, 99, 302, 63, White, Ink, b => UnityEventTools.AddPersistentListener(b.onClick, new UnityAction(actions.SelectGroup)));
        Button("WeekTab", page, "이번 주", 28, 188, 143, 28, Pale, Green, b => UnityEventTools.AddPersistentListener(b.onClick, new UnityAction(actions.ShowWeek)), 13);
        Button("MonthTab", page, "이번 달", 188, 188, 142, 28, Pale, Green, b => UnityEventTools.AddPersistentListener(b.onClick, new UnityAction(actions.ShowMonth)), 13);
        string[] names = {"소정", "예린", "지훈"}, times = {"15시간", "12시간 30분", "10시간"}, medals = {"🥇", "🥈", "🥉"};
        for (int i = 0; i < 3; i++)
        {
            var row = Panel("Rank" + (i + 1), page, 28, 229 + i * 82, 302, 65, White).transform;
            Text("Medal", row, medals[i], 12, 17, 33, 30, 17);
            Text("Name", row, names[i], 52, 19, 88, 26, 15, Ink, true);
            Text("Time", row, times[i], 146, 19, 145, 26, 14, Ink, true);
        }
        Button("Reward", page, "🏆  랭킹 보상", 28, 691, 302, 64, White, Ink, b => UnityEventTools.AddPersistentListener(b.onClick, new UnityAction(actions.ShowRewards)), 14);
        Save("Ranking");
    }
    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c); return c;
    }
}
#endif
