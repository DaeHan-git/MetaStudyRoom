using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StudyDayRecord
{
    public string date; // yyyy-MM-dd (local calendar date)
    public int seconds;
}

[Serializable]
public class StudyRecordCollection
{
    public List<StudyDayRecord> days = new List<StudyDayRecord>();
}

public static class StudyRecordStore
{
    const string Key = "MetaStudy.StudyDays.v1";
    static StudyRecordCollection Read()
    {
        string json = PlayerPrefs.GetString(Key, "");
        if (string.IsNullOrEmpty(json)) return new StudyRecordCollection();
        var data = JsonUtility.FromJson<StudyRecordCollection>(json);
        return data != null && data.days != null ? data : new StudyRecordCollection();
    }
    static void Save(StudyRecordCollection data)
    {
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }
    public static int SecondsOn(DateTime date)
    {
        string key = date.ToString("yyyy-MM-dd");
        foreach (var day in Read().days) if (day.date == key) return day.seconds;
        return 0;
    }
    public static int SecondsBetween(DateTime firstInclusive, DateTime lastInclusive)
    {
        int sum = 0;
        for (DateTime day = firstInclusive.Date; day <= lastInclusive.Date; day = day.AddDays(1))
            sum += SecondsOn(day);
        return sum;
    }
    // Integration point for teammate 4: submit actual focused seconds, excluding pauses.
    // Call only after the session is saved/confirmed, to avoid duplicate counting.
    public static void AddStudySeconds(DateTime localDate, int seconds)
    {
        if (seconds <= 0) return;
        var data = Read();
        string key = localDate.ToString("yyyy-MM-dd");
        var record = data.days.Find(x => x.date == key);
        if (record == null) { record = new StudyDayRecord { date = key }; data.days.Add(record); }
        record.seconds += seconds;
        Save(data);
    }
    public static void SeedDemoIfEmpty()
    {
        if (PlayerPrefs.HasKey(Key)) return;
        DateTime today = DateTime.Today;
        for (int i = 0; i < 14; i++)
        {
            if (i % 4 == 3) continue;
            AddStudySeconds(today.AddDays(-i), (30 + (i * 37) % 150) * 60);
        }
    }
    public static void ClearAll()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }
}
