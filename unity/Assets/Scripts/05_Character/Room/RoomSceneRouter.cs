using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomSceneRouter : MonoBehaviour
{
    [Serializable]
    public struct RoomSceneMapping
    {
        public string roomId;    // 예: "ROOM_01", "STUDY_A"
        public string sceneName; // Build Profiles / Build Settings에 등록된 씬 이름
    }

    [Header("Optional Input UI")]
    public TMP_InputField roomIdInputField;

    [Header("Current Room ID")]
    [SerializeField] private string currentRoomId = "";
    public string CurrentRoomId => currentRoomId;

    [Header("Room -> Scene Mappings")]
    [Tooltip("Room ID와 이동할 씬 이름을 매핑합니다.")]
    public List<RoomSceneMapping> roomMappings = new List<RoomSceneMapping>();

    [Header("Fallback Options")]
    [Tooltip("매핑에 없을 때 Room ID 문자열 자체를 씬 이름으로 시도할지 여부")]
    public bool fallbackToRoomIdAsSceneName = false;
    public string defaultSceneName = "";

    // UI 버튼의 OnClick() 등에 연결해서 입력창의 RoomId로 씬 이동
    public void LoadSceneFromInputField()
    {
        if (roomIdInputField == null)
        {
            Debug.LogWarning("[RoomSceneRouter] roomIdInputField가 연결되지 않았습니다.");
            return;
        }

        LoadSceneByRoomId(roomIdInputField.text);
    }

    // 다른 플레이어 스크립트나 UI/네트워크 콜백에서 직접 호출 가능
    public void SetRoomId(string roomId)
    {
        currentRoomId = (roomId ?? string.Empty).Trim();
    }

    public void LoadSceneByRoomId(string roomId)
    {
        SetRoomId(roomId);

        if (string.IsNullOrEmpty(currentRoomId))
        {
            Debug.LogWarning("[RoomSceneRouter] Room ID가 비어 있습니다.");
            return;
        }

        if (TryGetSceneName(currentRoomId, out string targetScene))
        {
            if (string.IsNullOrEmpty(targetScene))
            {
                Debug.LogWarning($"[RoomSceneRouter] '{currentRoomId}'에 연결된 씬 이름이 비어 있습니다.");
                return;
            }

            Debug.Log($"[RoomSceneRouter] Room '{currentRoomId}' -> 씬 '{targetScene}' 이동");
            SceneManager.LoadScene(targetScene);
            return;
        }

        if (!string.IsNullOrEmpty(defaultSceneName))
        {
            Debug.LogWarning($"[RoomSceneRouter] '{currentRoomId}' 매핑이 없어 기본 씬 '{defaultSceneName}'으로 이동합니다.");
            SceneManager.LoadScene(defaultSceneName);
            return;
        }

        Debug.LogWarning($"[RoomSceneRouter] '{currentRoomId}'에 해당하는 씬을 찾지 못했습니다.");
    }

    private bool TryGetSceneName(string roomId, out string sceneName)
    {
        for (int i = 0; i < roomMappings.Count; i++)
        {
            if (string.Equals(roomMappings[i].roomId, roomId, StringComparison.OrdinalIgnoreCase))
            {
                sceneName = roomMappings[i].sceneName;
                return true;
            }
        }

        if (fallbackToRoomIdAsSceneName)
        {
            sceneName = roomId;
            return true;
        }

        sceneName = string.Empty;
        return false;
    }
}