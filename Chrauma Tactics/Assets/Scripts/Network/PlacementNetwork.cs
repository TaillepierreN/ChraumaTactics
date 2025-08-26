using UnityEngine;
using Unity.Netcode;

public class PlacementNetwork : NetworkBehaviour
{
    public static PlacementNetwork Instance { get; private set; }

    [Header("Network registered prefabs")]
    [SerializeField] private GameObject _squdPrefab;
    [SerializeField] private GameObject[] _unitPrefabs;

    public override void OnNetworkSpawn()
    {
        if (Instance == null)
            Instance = this;
    }

    ///
    [ServerRpc(RequireOwnership = false)]
    public void PlaceSquadServerRpc(Vector3 worldPosition, int unitCount, int unitIndex)
    {
        if (!IsServer) return;

        if (!_squdPrefab)
        {
            Debug.LogWarning("PlacementNetwork: Squad prefab is not assigned");
            return;
        }

        if (unitIndex < 0 || unitIndex >= _unitPrefabs.Length)
        {
            Debug.LogWarning("PlacementNetwork: invalid unit index");
            return;
        }

        //TODO: Add placement validation

        GameObject squadGameobject = Instantiate(_squdPrefab, worldPosition, Quaternion.identity);
        NetworkObject squadNetworkObject = squadGameobject.GetComponent<NetworkObject>();
        if (squadNetworkObject == null)
        {
            Debug.LogWarning("PlacementNetwork: Squad prefab does not have a NetworkObject component");
            Destroy(squadGameobject);
            return;
        }

        squadNetworkObject.Spawn(true);

        Squad squad = squadGameobject.GetComponent<Squad>();
        squad.nbrOfUnits = Mathf.Clamp(unitCount, 1, 16);
        squad.unitPrefab = _unitPrefabs[unitIndex];
        //squad.team = GameManager.Instance.GetTeamFromClientId(OwnerClientId);
        squad.SpawnUnit();
    }
    public int IndexOfUnit(GameObject prefab)
    {
        for (int i = 0; i < _unitPrefabs.Length; i++)
            if (_unitPrefabs[i] == prefab) return i;
        return -1;
    }
}
