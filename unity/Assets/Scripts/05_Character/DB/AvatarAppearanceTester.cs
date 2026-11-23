using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DummyUserPreset
{
    public string uid;
    public AvatarAppearanceData appearance = new();
    public List<string> ownedItems = new();
}

public class AvatarAppearanceTester : MonoBehaviour
{
    [Header("비워두면 씬의 Player(AvatarAppearanceApplier)를 자동 탐색")]
    public AvatarAppearanceApplier targetApplier;

    [Header("Play 중에 바꿔볼 유저 ID")]
    public string userId = "user_01";

    [Header("유저별 더미 데이터 목록")]
    public List<DummyUserPreset> userPresets = new()
    {
        new DummyUserPreset
        {
            uid = "user_01",
            appearance = new AvatarAppearanceData { expressionId = "exp_smile", bodyId = "0", accessoryId = "1" },
            ownedItems = new List<string> { "exp_smile", "1", "33" }
        },
        new DummyUserPreset
        {
            uid = "user_02",
            appearance = new AvatarAppearanceData { expressionId = "exp_default", bodyId = "33", accessoryId = "2" },
            ownedItems = new List<string> { "exp_default", "33", "2" }
        }
    };

    private readonly DummyAvatarService _dummyService = new();
    private string _lastAppliedUserId;

    private void Awake()
    {
        ResolveTargetApplier();
    }

    private void Start()
    {
        ResolveTargetApplier();
        SeedAllPresets();
        ApplyByUserId(userId);
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        ResolveTargetApplier();

        if (userId != _lastAppliedUserId)
        {
            SeedAllPresets();
            ApplyByUserId(userId);
        }
    }

    private void ResolveTargetApplier()
    {
        if (targetApplier != null)
            return;

        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            targetApplier = playerObj.GetComponentInChildren<AvatarAppearanceApplier>();
        }

        if (targetApplier == null)
        {
            targetApplier = FindFirstObjectByType<AvatarAppearanceApplier>();
        }
    }

    public void SeedAllPresets()
    {
        foreach (var preset in userPresets)
        {
            if (preset == null || string.IsNullOrEmpty(preset.uid))
                continue;

            string[] owned = preset.ownedItems != null ? preset.ownedItems.ToArray() : Array.Empty<string>();
            _dummyService.SeedTestData(preset.uid, preset.appearance, owned);
        }
    }

    [ContextMenu("Apply By User Id")]
    public void ApplyByUserId()
    {
        SeedAllPresets();
        ApplyByUserId(userId);
    }

    public void ApplyByUserId(string targetUid)
    {
        ResolveTargetApplier();

        if (targetApplier == null)
        {
            Debug.LogWarning("[AvatarAppearanceTester] 대상 AvatarAppearanceApplier를 찾지 못했습니다.");
            return;
        }

        userId = targetUid;
        _lastAppliedUserId = targetUid;
        targetApplier.SyncObservedUid(targetUid);

        _dummyService.LoadAppearance(targetUid, loadedData =>
        {
            _dummyService.GetOwnedItemIds(targetUid, ownedSet =>
            {
                targetApplier.ApplyAppearance(loadedData, ownedSet);
                Debug.Log(
                    $"[UID: {targetUid}] 표정: {targetApplier.CurrentAppearance.expressionId}, " +
                    $"몸통: {targetApplier.CurrentAppearance.bodyId}, " +
                    $"악세서리: {targetApplier.CurrentAppearance.accessoryId}"
                );
            });
        });
    }
}