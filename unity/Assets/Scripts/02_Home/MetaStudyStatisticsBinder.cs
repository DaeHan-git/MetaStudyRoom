using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MetaStudyStatisticsBinder : MonoBehaviour
{
    [Tooltip("For UI testing only. Turn off before connecting real data.")]
    public bool useDemoData = true;
    [Tooltip("Daily goal in minutes for Home progress.")]
    public int dailyGoalMinutes = 180;
    public bool isStatisticsScene;
    public bool isHomeScene;

    Transform page;
    DateTime displayedMonth;
    DateTime selectedDate;
    static readonly Color Ink = new Color32(23, 34, 56, 255);
    static readonly Color Muted = new Color32(125, 138, 146, 255);
    static readonly Color Green = new Color32(72, 124, 96, 255);

    void Start()
    {
        page = GameObject.Find("Canvas/Page")?.transform;
        if (page == null) { Debug.LogWarning("MetaStudy: Canvas/Page not found"); return; }
        if (useDemoData) StudyRecordStore.SeedDemoIfEmpty();
        displayedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        selectedDate = DateTime.Today;
        Refresh();
    }
    public void Refresh()
    {
        if (page == null) return;
        if (isHomeScene) RefreshHome();
        if (isStatisticsScene) RefreshStatistics();
    }
    static string Duration(int seconds)
    {
        int minutes = seconds / 60;
        return $"{minutes / 60}시간 {minutes % 60}분";
    }
    TMP_Text Label(Transform parent, string name) => parent.Find(name)?.GetComponent<TMP_Text>();
    void SetLabel(Transform parent, string name, string text)
    {
        var label = Label(parent, name);
        if (label != null) label.text = text;
    }
    void RefreshHome()
    {
        Transform card = page.Find("TodayStudyCard");
        if (card == null) return;
        int seconds = StudyRecordStore.SecondsOn(DateTime.Today);
        int goal = Mathf.Max(1, dailyGoalMinutes) * 60;
        int percent = Mathf.RoundToInt(100f * seconds / goal);
        TimeSpan t = TimeSpan.FromSeconds(seconds);
        SetLabel(card, "Time", $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}");
        SetLabel(card, "Goal", $"목표 {Duration(goal)} · {percent}% 달성");
        RectTransform track = card.Find("ProgressTrack") as RectTransform;
        RectTransform fill = card.Find("ProgressFill") as RectTransform;
        if (track != null && fill != null)
        {
            float width = track.rect.width * Mathf.Clamp01((float)seconds / goal);
            fill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }
    }
    // Reposition existing Summary text without regenerating or replacing the scene.
    void UpdateSummaryLayout(Transform summary, int studySeconds)
    {
        TMP_Text dateText = Label(summary, "Today");
        TMP_Text weekText = Label(summary, "Week");

        if (dateText != null)
        {
            RectTransform r = dateText.rectTransform;
            r.anchorMin = r.anchorMax = new Vector2(0f, 0.5f);
            r.pivot = new Vector2(0f, 0.5f);
            r.anchoredPosition = new Vector2(19f, 16f);
            r.sizeDelta = new Vector2(155f, 40f);
            dateText.alignment = TextAlignmentOptions.MidlineLeft;
            dateText.fontSize = 22f;
            dateText.fontStyle = FontStyles.Bold;
            dateText.color = Green;
            dateText.textWrappingMode = TextWrappingModes.NoWrap;
        }

        if (weekText != null)
        {
            RectTransform r = weekText.rectTransform;
            r.anchorMin = r.anchorMax = new Vector2(0f, 0.5f);
            r.pivot = new Vector2(0f, 0.5f);
            r.anchoredPosition = new Vector2(19f, -25f);
            r.sizeDelta = new Vector2(170f, 26f);
            weekText.alignment = TextAlignmentOptions.MidlineLeft;
            weekText.fontSize = 11f;
            weekText.fontStyle = FontStyles.Normal;
            weekText.color = Muted;
            weekText.textWrappingMode = TextWrappingModes.NoWrap;
        }

        // The right-hand text is a separate TMP object, centered in the right half.
        Transform existing = summary.Find("SelectedStudyDuration");
        TMP_Text durationText;
        if (existing == null)
        {
            GameObject go = new GameObject("SelectedStudyDuration", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(summary, false);
            durationText = go.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            durationText = existing.GetComponent<TMP_Text>();
            if (durationText == null) durationText = existing.gameObject.AddComponent<TextMeshProUGUI>();
        }

        if (dateText != null && dateText.font != null) durationText.font = dateText.font;
        RectTransform right = durationText.rectTransform;
        right.anchorMin = right.anchorMax = new Vector2(0.75f, 0.5f);
        right.pivot = new Vector2(0.5f, 0.5f);
        right.anchoredPosition = Vector2.zero;
        right.sizeDelta = new Vector2(175f, 55f);
        durationText.alignment = TextAlignmentOptions.Center;
        durationText.fontSize = 23f;
        durationText.fontStyle = FontStyles.Bold;
        durationText.color = new Color32(39, 92, 65, 255);
        durationText.raycastTarget = false;
        durationText.textWrappingMode = TextWrappingModes.NoWrap;
        int totalMinutes = Mathf.Max(0, studySeconds / 60);
        durationText.text = totalMinutes >= 60
            ? $"{totalMinutes / 60}시간 {totalMinutes % 60}분"
            : $"{totalMinutes}분";
    }

    void RefreshStatistics()
    {
        Transform summary = page.Find("Summary");
        if (summary != null)
        {
            // Left: selected date and weekly total. Right: bold, centered study duration.
            SetLabel(summary, "Today", selectedDate.ToString("M월 d일"));
            DateTime monday = selectedDate.AddDays(-((int)selectedDate.DayOfWeek + 6) % 7);
            SetLabel(summary, "Week", "선택한 주 " + Duration(StudyRecordStore.SecondsBetween(monday, monday.AddDays(6))));
            UpdateSummaryLayout(summary, StudyRecordStore.SecondsOn(selectedDate));
        }
        SetLabel(page, "Month", displayedMonth.ToString("yyyy년 M월"));
        DateTime gridStart = displayedMonth.AddDays(-(int)displayedMonth.DayOfWeek);
        for (int i = 0; i < 42; i++)
        {
            DateTime date = gridStart.AddDays(i);
            Transform cell = page.Find("Day" + i);
            if (cell == null) continue;
            var text = cell.GetComponent<TMP_Text>();
            if (text != null)
            {
                text.text = date.Day.ToString();
                text.color = date.Month == displayedMonth.Month ? (date.Date == selectedDate.Date ? Green : Ink) : Muted;
                text.fontStyle = date.Date == selectedDate.Date ? FontStyles.Bold : FontStyles.Normal;
                text.raycastTarget = true;
            }
            var button = cell.GetComponent<Button>();
            if (button == null) button = cell.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.RemoveAllListeners();
            DateTime captured = date;
            button.onClick.AddListener(() => { selectedDate = captured; displayedMonth = new DateTime(captured.Year, captured.Month, 1); Refresh(); });
            Transform dot = page.Find("StudyDot" + i);
            if (dot == null) dot = CreateDot(i, cell);
            dot.gameObject.SetActive(date.Month == displayedMonth.Month && StudyRecordStore.SecondsOn(date) > 0);
        }
        // The selected date/time is now shown in Summary; remove the duplicate block.
        Transform selected = page.Find("SelectedDate");
        if (selected != null) selected.gameObject.SetActive(false);
        RefreshBars();
    }
    Transform CreateDot(int index, Transform dayCell)
    {
        var go = new GameObject("StudyDot" + index, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(page, false);
        var r = go.GetComponent<RectTransform>();
        var day = dayCell as RectTransform;
        r.anchorMin = r.anchorMax = new Vector2(0, 1);
        r.pivot = new Vector2(0, 1);
        r.anchoredPosition = day.anchoredPosition + new Vector2(15, -23);
        r.sizeDelta = new Vector2(5, 5);
        var image = go.GetComponent<Image>(); image.color = Green; image.raycastTarget = false;
        return go.transform;
    }
    // Graph labels are generated at runtime, so the existing scene layout stays unchanged.
    TMP_Text GetOrCreateGraphLabel(string objectName, TMP_Text fontReference, float fontSize)
    {
        Transform existing = page.Find(objectName);
        TMP_Text label;
        if (existing != null)
        {
            label = existing.GetComponent<TMP_Text>();
            if (label == null) label = existing.gameObject.AddComponent<TextMeshProUGUI>();
        }
        else
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(page, false);
            label = go.GetComponent<TextMeshProUGUI>();
        }
        if (fontReference != null && fontReference.font != null) label.font = fontReference.font;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Ink;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        var rect = label.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(54f, 30f);
        return label;
    }

    void RefreshBars()
    {
        // Graph begins directly below the last calendar row (no SelectedDate panel).
        const float graphTop = 626f;
        const float graphBottom = 777f;
        const float graphHeight = graphBottom - graphTop;

        var title = page.Find("WeeklyTitle") as RectTransform;
        if (title != null)
        {
            title.anchoredPosition = new Vector2(27f, -584f);
            title.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 304f);
        }
        var background = page.Find("ChartBackground") as RectTransform;
        if (background != null)
        {
            background.anchoredPosition = new Vector2(48f, -616f);
            background.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 283f);
            background.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 169f);
        }

        DateTime monday = selectedDate.AddDays(-((int)selectedDate.DayOfWeek + 6) % 7);
        int[] seconds = new int[7];
        int maxMinutes = 0;
        for (int i = 0; i < 7; i++)
        {
            seconds[i] = StudyRecordStore.SecondsOn(monday.AddDays(i));
            maxMinutes = Math.Max(maxMinutes, Mathf.CeilToInt(seconds[i] / 60f));
        }
        int tickStep = Mathf.Max(30, Mathf.CeilToInt(maxMinutes / 3f / 30f) * 30);
        int axisMaxMinutes = tickStep * 3;
        TMP_Text reference = Label(page, "WeeklyTitle") ?? Label(page, "Month");

        // Left Y-axis, minutes only. No labels above bars.
        for (int tick = 0; tick <= 3; tick++)
        {
            TMP_Text axisLabel = GetOrCreateGraphLabel("GraphAxisMinute" + tick, reference, 9f);
            axisLabel.gameObject.SetActive(true);
            axisLabel.text = (tick * tickStep) + "분";
            axisLabel.color = Muted;
            axisLabel.alignment = TextAlignmentOptions.Right;
            axisLabel.rectTransform.sizeDelta = new Vector2(42f, 18f);
            axisLabel.rectTransform.anchoredPosition =
                new Vector2(25f, -(graphBottom - graphHeight * tick / 3f));
        }

        for (int i = 0; i < 7; i++)
        {
            var bar = page.Find("Bar" + i) as RectTransform;
            if (bar == null) continue;
            float height = graphHeight * seconds[i] / (axisMaxMinutes * 60f);
            float barX = 74f + i * 36f;
            bar.anchoredPosition = new Vector2(barX, -(graphBottom - height));
            bar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 22f);
            bar.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            TMP_Text dateLabel = GetOrCreateGraphLabel("GraphDate" + i, reference, 10f);
            dateLabel.gameObject.SetActive(true);
            dateLabel.text = monday.AddDays(i).ToString("M/d");
            dateLabel.rectTransform.sizeDelta = new Vector2(36f, 18f);
            dateLabel.rectTransform.anchoredPosition = new Vector2(barX + 11f, -800f);
            dateLabel.color = Muted;

            Transform oldDuration = page.Find("GraphDuration" + i);
            if (oldDuration != null) oldDuration.gameObject.SetActive(false);
        }
        SetLabel(page, "WeeklyTitle", $"{monday:M월 d일}~{monday.AddDays(6):M월 d일} 공부 시간");
    }
    public void PreviousMonth() { displayedMonth = displayedMonth.AddMonths(-1); selectedDate = displayedMonth; Refresh(); }
    public void NextMonth() { displayedMonth = displayedMonth.AddMonths(1); selectedDate = displayedMonth; Refresh(); }
    public void AddTestThirtyMinutes() { StudyRecordStore.AddStudySeconds(DateTime.Today, 1800); Refresh(); }
}
