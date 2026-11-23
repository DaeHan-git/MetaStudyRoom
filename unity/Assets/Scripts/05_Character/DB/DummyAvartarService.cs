using System;
using System.Collections.Generic;
using UnityEngine;



public interface IAvatarAppearanceStorage
{
    void SaveAppearance(string uid, AvatarAppearanceData data, Action<bool> onComplete = null);
    void LoadAppearance(string uid, Action<AvatarAppearanceData> onLoaded);
}

public interface IOwnedItemProvider
{
    void GetOwnedItemIds(string uid, Action<HashSet<string>> onLoaded);
}

public class DummyAvatarService : IAvatarAppearanceStorage, IOwnedItemProvider
{
    private readonly Dictionary<string, AvatarAppearanceData> _avatars = new();
    private readonly Dictionary<string, HashSet<string>> _ownedItems = new();

    public void SeedTestData(string uid, AvatarAppearanceData appearance, params string[] ownedItemIds)
    {
        _avatars[uid] = appearance;
        _ownedItems[uid] = new HashSet<string>(ownedItemIds);
    }

    public void SaveAppearance(string uid, AvatarAppearanceData data, Action<bool> onComplete = null)
    {
        if (string.IsNullOrEmpty(uid) || data == null)
        {
            onComplete?.Invoke(false);
            return;
        }

        _avatars[uid] = data;
        onComplete?.Invoke(true);
    }

    public void LoadAppearance(string uid, Action<AvatarAppearanceData> onLoaded)
    {
        if (!string.IsNullOrEmpty(uid) && _avatars.TryGetValue(uid, out var data))
        {
            onLoaded?.Invoke(data);
            return;
        }

        onLoaded?.Invoke(new AvatarAppearanceData());
    }

    public void GetOwnedItemIds(string uid, Action<HashSet<string>> onLoaded)
    {
        if (!string.IsNullOrEmpty(uid) && _ownedItems.TryGetValue(uid, out var items))
        {
            onLoaded?.Invoke(new HashSet<string>(items));
            return;
        }

        onLoaded?.Invoke(new HashSet<string>());
    }
}