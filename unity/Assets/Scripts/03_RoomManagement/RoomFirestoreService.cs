using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

// 서버 데이터 접근 전용. 씬 오브젝트에 붙이지 않습니다.
public static class RoomFirestoreService
{
    public const string CollectionName = "rooms";

    public static string RequireUser(string expectedUid = null)
    {
        string uid = FirebaseManager.CurrentUserId;
        if (string.IsNullOrEmpty(uid))
            throw new InvalidOperationException("로그인 후 이용해 주세요.");
        if (expectedUid != null && uid != expectedUid)
            throw new InvalidOperationException("로그인 계정이 변경됐어요. 다시 시도해 주세요.");
        return uid;
    }

    public static Query ActivePublicRooms()
    {
        RequireUser();
        // 아래 두 조건은 firestore.rules의 목록 읽기 조건과 일치해야 합니다.
        return FirebaseManager.Firestore.Collection(CollectionName)
            .WhereEqualTo("isPrivate", false).WhereEqualTo("status", "active");
    }

    public static async Task<string> CreateAsync(string uid, string title,
        string categoryId, string mapId, int maxMembers, bool textChat, bool voiceChat)
    {
        await FirebaseManager.InitializeAsync();
        RequireUser(uid);
        title = (title ?? "").Trim();
        if (title.Length == 0 || title.Length > 60)
            throw new ArgumentException("방 제목은 1~60자로 입력해 주세요.");
        if (categoryId != "coding" && categoryId != "language" && categoryId != "certificate")
            throw new ArgumentException("카테고리를 확인해 주세요.");
        if (mapId != "cafe" && mapId != "library" && mapId != "school")
            throw new ArgumentException("맵을 확인해 주세요.");
        if (maxMembers != 2 && maxMembers != 4 && maxMembers != 6 && maxMembers != 8)
            throw new ArgumentException("정원은 2, 4, 6, 8명 중 선택해 주세요.");

        DocumentReference doc = FirebaseManager.Firestore.Collection(CollectionName).Document();
        var data = new Dictionary<string, object>
        {
            { "title", title }, { "categoryId", categoryId }, { "mapId", mapId },
            { "ownerUid", uid }, { "maxMembers", maxMembers },
            { "isPrivate", false }, { "status", "active" },
            { "textChat", textChat }, { "voiceChat", voiceChat },
            { "createdAt", FieldValue.ServerTimestamp },
            { "updatedAt", FieldValue.ServerTimestamp }
        };
        // 완료는 서버 승인 후에만 발생합니다. 오프라인이면 대기할 수 있습니다.
        await doc.SetAsync(data);
        RequireUser(uid);
        return doc.Id;
    }

    public static async Task<RoomRecord> GetActiveAsync(string uid, string roomId)
    {
        await FirebaseManager.InitializeAsync();
        RequireUser(uid);
        DocumentSnapshot snapshot = await FirebaseManager.Firestore
            .Collection(CollectionName).Document(roomId).GetSnapshotAsync(Source.Server);
        RequireUser(uid);
        if (!snapshot.Exists || snapshot.Metadata.HasPendingWrites)
            throw new InvalidOperationException("방 정보를 서버에서 확인하지 못했어요.");
        RoomRecord room = RoomRecord.FromSnapshot(snapshot);
        if (room.IsPrivate || room.Status != "active")
            throw new InvalidOperationException("현재 입장할 수 없는 방이에요.");
        return room;
    }

    public static async Task CloseAsync(string uid, string roomId)
    {
        await FirebaseManager.InitializeAsync();
        RequireUser(uid);
        // 소유권과 active -> closed 전환은 서버 규칙이 검사합니다.
        await FirebaseManager.Firestore.Collection(CollectionName).Document(roomId)
            .UpdateAsync(new Dictionary<string, object>
            {
                { "status", "closed" }, { "updatedAt", FieldValue.ServerTimestamp }
            });
        RequireUser(uid);
    }
}

public sealed class RoomRecord
{
    public string Id, Title, CategoryId, MapId, OwnerUid, Status;
    public int MaxMembers;
    public bool IsPrivate, TextChat, VoiceChat;
    public DateTime CreatedAtUtc;

    public static RoomRecord FromSnapshot(DocumentSnapshot doc)
    {
        return new RoomRecord
        {
            Id = doc.Id,
            Title = doc.GetValue<string>("title"),
            CategoryId = doc.GetValue<string>("categoryId"),
            MapId = doc.GetValue<string>("mapId"),
            OwnerUid = doc.GetValue<string>("ownerUid"),
            Status = doc.GetValue<string>("status"),
            MaxMembers = checked((int)doc.GetValue<long>("maxMembers")),
            IsPrivate = doc.GetValue<bool>("isPrivate"),
            TextChat = doc.GetValue<bool>("textChat"),
            VoiceChat = doc.GetValue<bool>("voiceChat"),
            CreatedAtUtc = doc.GetValue<Timestamp>("createdAt").ToDateTime()
        };
    }
}

// 씬 전환에 필요한 선택값만 보관합니다. 입장/좌석/인원 등록을 뜻하지 않습니다.
public static class RoomSelectionContext
{
    private static RoomRecord selected;
    private static string selectedByUid;
    public static RoomRecord SelectedRoom =>
        FirebaseManager.IsSignedIn && FirebaseManager.CurrentUserId == selectedByUid
            ? selected : null;
    public static string RoomId => SelectedRoom?.Id;
    public static void Select(RoomRecord room, string uid)
    {
        RoomFirestoreService.RequireUser(uid);
        selected = room;
        selectedByUid = uid;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Clear()
    {
        selected = null;
        selectedByUid = null;
    }
}
