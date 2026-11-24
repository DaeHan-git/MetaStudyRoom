using TMPro;
using UnityEngine;

public class RoomInputToggleUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("처음에 숨겨둘 입력창 패널 또는 RoomInput 오브젝트")]
    public GameObject roomInputPanelObj;
    public TMP_InputField roomIdInputField;

    [Header("Options")]
    [Tooltip("버튼을 다시 누르면 닫히게 할지 여부")]
    public bool toggleOnButtonClick = true;
    [Tooltip("씬 이동 후 입력창을 자동으로 다시 숨길지 여부")]
    public bool hideAfterSubmit = true;

    void Start()
    {
        if (roomInputPanelObj != null)
        {
            roomInputPanelObj.SetActive(false);
        }
    }

    // '방 이동' 열기 버튼의 OnClick()에 이 메서드를 연결하세요.
    public void OnClickOpenOrToggleRoomInput()
    {
        if (roomInputPanelObj == null) return;

        bool nextState = toggleOnButtonClick ? !roomInputPanelObj.activeSelf : true;
        roomInputPanelObj.SetActive(nextState);

        if (nextState && roomIdInputField != null)
        {
            roomIdInputField.Select();
            roomIdInputField.ActivateInputField();
        }
    }

    // 닫기(X) 버튼이나 이동 완료 후 숨길 때 호출할 수 있습니다.
    public void HideRoomInput()
    {
        if (roomInputPanelObj != null)
        {
            roomInputPanelObj.SetActive(false);
        }
    }

    public void OnRoomEntered()
    {
        if (hideAfterSubmit)
        {
            HideRoomInput();
        }
    }
}