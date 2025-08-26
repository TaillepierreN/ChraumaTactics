using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Tracks how many times a prefab was "registered" so we only add/remove once.
/// </summary>
public static class NetPrefabHandlerRegistry
{
    private static readonly Dictionary<GameObject, (INetworkPrefabInstanceHandler handler, int refCount)>
        s_map = new();

    public static bool TryAdd(GameObject prefab, INetworkPrefabInstanceHandler handler, out bool firstAdd)
    {
        firstAdd = false;
        if (!prefab || handler == null) return false;

        if (s_map.TryGetValue(prefab, out var rec))
        {
            s_map[prefab] = (rec.handler, rec.refCount + 1);
        }
        else
        {
            s_map[prefab] = (handler, 1);
            firstAdd = true;
        }
        return true;
    }

    public static bool TryRelease(GameObject prefab, out INetworkPrefabInstanceHandler handler, out bool lastRelease)
    {
        handler = null;
        lastRelease = false;
        if (!prefab) return false;

        if (!s_map.TryGetValue(prefab, out var rec)) return false;

        rec.refCount--;
        if (rec.refCount <= 0)
        {
            handler = rec.handler;
            lastRelease = true;
            s_map.Remove(prefab);
        }
        else
        {
            s_map[prefab] = rec;
        }
        return true;
    }
}
