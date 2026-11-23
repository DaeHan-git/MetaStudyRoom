using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MetaStudyUIActions : MonoBehaviour
{
    public void OpenHome() => Load("Home");
    public void OpenStatistics() => Load("Statistics");
    public void OpenRanking() => Load("Ranking");

    private void Load(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"'{sceneName}' Scene을 Build Profiles > Scene List에 추가하고 활성화하세요.");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

    public void EnterStudyRoom() => Debug.Log("스터디룸 입장: 3번/4번 담당 기능과 연결 예정");
    public void SelectGroup() => Debug.Log("그룹 선택: 서버 연동 예정");
    public void ShowRewards() => Debug.Log("랭킹 보상: 서버 연동 예정");
    public void ToggleTodo(Toggle toggle) => Debug.Log("ToDo 상태 변경: " + toggle.isOn);
    public void ShowWeek() => ChangePeriod(true);
    public void ShowMonth() => ChangePeriod(false);

    public void ChangePeriod(bool weekly)
    {
        foreach (var t in FindObjectsByType<MetaStudyPeriodLabel>(FindObjectsSortMode.None))
            t.SetPeriod(weekly);
        Debug.Log(weekly ? "이번 주 선택" : "이번 달 선택");
    }
}

public class MetaStudyTodoBinding : MonoBehaviour
{
    private Toggle toggle;
    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        if (toggle != null) toggle.onValueChanged.AddListener(OnChanged);
    }
    private void OnDestroy()
    {
        if (toggle != null) toggle.onValueChanged.RemoveListener(OnChanged);
    }
    private void OnChanged(bool checkedValue) => Debug.Log("ToDo 상태 변경: " + checkedValue);
}

public class MetaStudyPeriodLabel : MonoBehaviour
{
    public TMP_Text label;
    public void SetPeriod(bool weekly)
    {
        if (label != null) label.text = weekly ? "이번 주" : "이번 달";
    }
}
