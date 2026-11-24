using TMPro;
using UnityEngine;

public class PlayerRoomHandler : MonoBehaviour
{
    [Header("Control Ownership")]
    public bool isLocalPlayer = true;

    [Header("Room Routing")]
    public RoomSceneRouter roomSceneRouter;
    public TMP_InputField roomIdInputField; // 입력창이 있을 때 연결(선택)
    [SerializeField] private string assignedRoomId = "";

    public string AssignedRoomId => assignedRoomId;

    void Start()
    {
        if (roomSceneRouter == null)
        {
            roomSceneRouter = FindFirstObjectByType<RoomSceneRouter>();
        }
    }

    public void AssignRoomId(string newRoomId)
    {
        if (!isLocalPlayer) return;

        assignedRoomId = (newRoomId ?? string.Empty).Trim();
        if (roomSceneRouter != null)
        {
            roomSceneRouter.SetRoomId(assignedRoomId);
        }
    }

    // 이동 버튼의 OnClick()에 이 메서드를 연결하세요.
    public void EnterAssignedRoomScene()
    {
        Debug.Log($"[Debug] InputField: {(roomIdInputField != null ? roomIdInputField.text : "NULL")}, assignedRoomId: '{assignedRoomId}', isLocalPlayer: {isLocalPlayer}");
        if (!isLocalPlayer) return;

        if (roomIdInputField != null)
        {
            assignedRoomId = (roomIdInputField.text ?? string.Empty).Trim();
        }

        if (roomSceneRouter == null)
        {
            Debug.LogWarning("[PlayerRoomHandler] RoomSceneRouter가 연결되지 않았습니다.");
            return;
        }

        roomSceneRouter.LoadSceneByRoomId(assignedRoomId);
    }
}