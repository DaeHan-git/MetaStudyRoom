using UnityEngine;
using UnityEngine.UI;

public class AvatarSelectManager : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private ToggleGroup avatarToggleGroup; // AvatarGridGroup 연결
    [SerializeField] private Toggle[] avatarToggles;         // AvatarItem 1~4 토글 배열

    private const string AVATAR_KEY = "SelectedAvatarIndex"; // 저장할 데이터 키 값

    private void Start()
    {
        // 이전 선택 데이터 불러오기 (기본값: 0번 아바타)
        int savedIndex = PlayerPrefs.GetInt(AVATAR_KEY, 0);

        // 이전 선택 데이터에 맞춰 토글 IsOn 설정
        for (int i = 0; i < avatarToggles.Length; i++)
        {
            if (avatarToggles[i] != null)
            {
                avatarToggles[i].isOn = (i == savedIndex);
            }
        }
    }

    // 저장 버튼 클릭 시 호출되는 함수
    public void OnClickSave()
    {
        int selectedIndex = -1;

        // 4개 토글 중 현재 체크(isOn)된 토글 찾기
        for (int i = 0; i < avatarToggles.Length; i++)
        {
            if (avatarToggles[i] != null && avatarToggles[i].isOn)
            {
                selectedIndex = i;
                break;
            }
        }

        if (selectedIndex != -1)
        {
            // 선택된 인덱스 저장 (0: 첫번째, 1: 두번째 ...)
            PlayerPrefs.SetInt(AVATAR_KEY, selectedIndex);
            PlayerPrefs.Save();

            Debug.Log($"[아바타 저장 완료] 선택된 아바타 인덱스: {selectedIndex}");

            // TODO: 다음 UI 화면이 준비되면 아래에 씬 이동이나 패널 전환 코드 추가
            // SceneManager.LoadScene("NextSceneName");
        }
        else
        {
            Debug.LogWarning("선택된 아바타가 없습니다.");
        }
    }

    // 뒤로가기 버튼 클릭 시 호출되는 함수
    public void OnClickBack()
    {
        Debug.Log("뒤로가기 버튼 클릭됨");
        // TODO: 이전 UI 화면 전환 코드 추가
    }
}