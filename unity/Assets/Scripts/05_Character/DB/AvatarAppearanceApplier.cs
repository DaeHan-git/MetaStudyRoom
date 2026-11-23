using System.Collections.Generic;
using UnityEngine;

public class AvatarAppearanceApplier : MonoBehaviour
{
    [Header("현재 플레이어 UID (Play 중 변경 시 Tester와 동기화)")]
    public string uid = "user_01";

    [Header("기본값 (미보유 또는 빈 값일 때 사용)")]
    public string defaultExpressionId = "exp_default";
    public string defaultBodyId = "cat";
    public string defaultAccessoryId = "";

    [Header("Resources 하위 폴더 경로")]
    public string expressionFolder = "Avatar/Expressions";
    public string bodyFolder = "Avatar/Bodies";
    public string accessoryFolder = "Avatar/Accessories";

    [Header("기존 고정 바디 오브젝트 (예: Player/Mesh/Cat)")]
    public GameObject legacyDefaultBodyObject;

    [Header("선택적 부모 위치 (비워두면 캐릭터 루트 아래에 생성)")]
    public Transform expressionAnchor;
    public Transform bodyAnchor;
    public Transform accessoryAnchor;

    private string _lastObservedUid;
    private GameObject _currentExpressionObj;
    private GameObject _currentBodyObj;
    private GameObject _currentAccessoryObj;

    public AvatarAppearanceData CurrentAppearance { get; private set; } = new();

    private void Start()
    {
        _lastObservedUid = uid;
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        if (uid != _lastObservedUid)
        {
            _lastObservedUid = uid;
            var tester = FindFirstObjectByType<AvatarAppearanceTester>();
            if (tester != null && tester.userId != uid)
            {
                tester.userId = uid;
                tester.ApplyByUserId(uid);
            }
        }
    }

    public void SyncObservedUid(string newUid)
    {
        uid = newUid;
        _lastObservedUid = newUid;
    }

    public void ApplyAppearance(AvatarAppearanceData data, HashSet<string> ownedItemIds = null)
    {
        if (data == null)
            data = new AvatarAppearanceData();

        string resolvedExpression = ResolveValidId(data.expressionId, defaultExpressionId, ownedItemIds, allowEmpty: false);
        string resolvedBody = ResolveValidId(data.bodyId, defaultBodyId, ownedItemIds, allowEmpty: false);
        string resolvedAccessory = ResolveValidId(data.accessoryId, defaultAccessoryId, ownedItemIds, allowEmpty: true);

        CurrentAppearance = new AvatarAppearanceData
        {
            expressionId = resolvedExpression,
            bodyId = resolvedBody,
            accessoryId = resolvedAccessory
        };

        bool wantLegacyBody = legacyDefaultBodyObject != null && resolvedBody == defaultBodyId;

        if (wantLegacyBody)
        {
            _currentBodyObj = DestroyIfExists(_currentBodyObj);
            legacyDefaultBodyObject.SetActive(true);
        }
        else
        {
            GameObject spawnedBody = SpawnPartById(bodyFolder, resolvedBody, bodyAnchor, _currentBodyObj);
            if (spawnedBody != null)
            {
                _currentBodyObj = spawnedBody;
                if (legacyDefaultBodyObject != null)
                    legacyDefaultBodyObject.SetActive(false);
            }
            else
            {
                _currentBodyObj = null;
                if (legacyDefaultBodyObject != null)
                    legacyDefaultBodyObject.SetActive(true);
            }
        }

        _currentExpressionObj = SpawnPartById(expressionFolder, resolvedExpression, expressionAnchor, _currentExpressionObj);
        _currentAccessoryObj = SpawnPartById(accessoryFolder, resolvedAccessory, accessoryAnchor, _currentAccessoryObj);
    }

    private GameObject DestroyIfExists(GameObject target)
    {
        if (target != null)
        {
            Destroy(target);
        }
        return null;
    }

    private string ResolveValidId(string candidateId, string fallbackId, HashSet<string> ownedItemIds, bool allowEmpty)
    {
        if (allowEmpty && string.IsNullOrEmpty(candidateId))
            return string.Empty;

        if (string.IsNullOrEmpty(candidateId))
            return fallbackId;

        if (ownedItemIds != null && candidateId != fallbackId && !ownedItemIds.Contains(candidateId))
            return fallbackId;

        return candidateId;
    }

    private GameObject SpawnPartById(string folderPath, string id, Transform anchor, GameObject previousObj)
    {
        if (previousObj != null)
        {
            Destroy(previousObj);
        }

        if (string.IsNullOrEmpty(id))
            return null;

        string cleanFolder = string.IsNullOrEmpty(folderPath) ? string.Empty : folderPath.Trim('/') + "/";
        string resourcePath = $"{cleanFolder}{id}";
        GameObject prefab = Resources.Load<GameObject>(resourcePath);

        if (prefab == null)
        {
            Debug.LogWarning($"[AvatarAppearanceApplier] 프리팹을 찾을 수 없습니다. 경로: Resources/{resourcePath}");
            return null;
        }

        Transform parent = anchor != null ? anchor : transform;
        return Instantiate(prefab, parent);
    }
}