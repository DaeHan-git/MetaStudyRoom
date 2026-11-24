using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // UI Image 제어를 위해 추가

public class AwayModeController : MonoBehaviour
{
    [Header("State")]
    [Tooltip("true = 자리비움, false = 공부중")]
    [SerializeField] private bool isAway = false;

    // 다른 스크립트에서 현재 상태를 읽거나, 값이 바뀔 때 이벤트를 구독할 수 있습니다.
    public bool IsAway => isAway;
    public event Action<bool> OnAwayStateChanged;

    [Header("Optional UI Label")]
    public TextMeshProUGUI statusButtonText;
    public string awayLabel = "공부중으로 전환";
    public string studyingLabel = "자리비움";

    [Header("Optional UI Image")]
    [Tooltip("상태에 따라 스프라이트를 바꿀 UI Image (버튼 이미지 또는 별도 상태 아이콘)")]
    public Image statusImage;
    public Sprite awaySprite;     // 자리비움(isAway == true)일 때 보여줄 이미지
    public Sprite studyingSprite; // 공부중(isAway == false)일 때 보여줄 이미지

    void Start()
    {
        RefreshUI();
    }

    // UI 버튼의 OnClick() 이벤트에 이 메서드를 연결하세요.
    public void ToggleAwayState()
    {
        SetAwayState(!isAway);
    }

    // 외부 스크립트에서 직접 상태를 지정해야 할 때도 사용할 수 있습니다.
    public void SetAwayState(bool away)
    {
        if (isAway == away) return;

        isAway = away;
        RefreshUI();
        OnAwayStateChanged?.Invoke(isAway);
    }

    private void RefreshUI()
    {
        if (statusButtonText != null)
        {
            statusButtonText.text = isAway ? awayLabel : studyingLabel;
        }

        if (statusImage != null)
        {
            Sprite targetSprite = isAway ? awaySprite : studyingSprite;
            if (targetSprite != null)
            {
                statusImage.sprite = targetSprite;
            }
        }
    }
}