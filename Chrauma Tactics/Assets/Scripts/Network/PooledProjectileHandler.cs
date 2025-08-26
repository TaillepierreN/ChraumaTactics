using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Minimal INetworkPrefabInstanceHandler that sources instances from a Unity ObjectPool.
/// Add once. Used by Ballistic.Awake() via NetworkManager.Singleton.PrefabHandler.
/// </summary>
public sealed class PooledProjectileHandler : INetworkPrefabInstanceHandler
{
    private readonly GameObject _prefab;
    private readonly ObjectPool<GameObject> _pool;

    public PooledProjectileHandler(GameObject prefab, ObjectPool<GameObject> pool)
    {
        _prefab = prefab;
        _pool = pool;
    }

    /// <summary>
    /// NGO calls this on clients/host when a spawn message arrives
    /// </summary>
    /// <param name="ownerClientId"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        GameObject go = _pool != null
            ? _pool.Get()
            : Object.Instantiate(_prefab);

        go.transform.SetPositionAndRotation(position, rotation);
        var nob = go.GetComponent<NetworkObject>();
        if (!nob)
            nob = go.AddComponent<NetworkObject>();

        return nob;
    }

    /// <summary>
    /// NGO calls this on despawn
    /// </summary>
    /// <param name="networkObject"></param>
    public void Destroy(NetworkObject networkObject)
    {
        var go = networkObject.gameObject;
        if (_pool != null)
        {
            _pool.Release(go);
        }
        else
        {
            Object.Destroy(go);
        }
    }
}
