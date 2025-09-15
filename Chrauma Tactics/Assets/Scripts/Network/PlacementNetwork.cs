using UnityEngine;
using Unity.Netcode;
using CT.Gameplay;
using CT.Tools;
using CT.Grid;

public class PlacementNetwork : NetworkBehaviour
{
    public static PlacementNetwork Instance { get; private set; }

    [Header("Network registered prefabs")]
    [SerializeField] private GameObject _squdPrefab;
    [SerializeField] private GameObject[] _unitPrefabs;
    [SerializeField] private Rd_Gameplay _radioGameplay;
    private Vector3 p1Forward = Vector3.forward;
    private Vector3 p2Forward = Vector3.back;
    public override void OnNetworkSpawn()
    {
        if (Instance == null)
            Instance = this;
    }

    ///
    [ServerRpc(RequireOwnership = false)]
    public void PlaceSquadServerRpc(Vector3 worldPosition, int unitCount, int unitIndex, bool useVoucher, ServerRpcParams rpcParams = default)
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

        /*Authoritative spend / voucher consume on server*/
        GameManager gm = _radioGameplay != null ? _radioGameplay.GameManager : null;
        if (gm == null)
        {
            Debug.LogWarning("PlacementNetwork: GameManager not found via Rd_Gameplay");
            return;
        }

        ulong sender = rpcParams.Receive.SenderClientId;
        Team team = (sender == NetworkManager.ServerClientId) ? Team.Player1 : Team.Player2;

        GridPosition gridPos = LevelGrid.Instance.GetGridPosition(worldPosition);
        if (!LevelGrid.Instance.IsValidGridPosition(gridPos) ||
            LevelGrid.Instance.HasAnySquadOnGridPosition(gridPos) ||
            !LevelGrid.Instance.IsInTeamArea(gridPos, team))
        {
            Debug.LogWarning("PlacementNetwork: invalid or occupied grid cell, rejecting placement");
            return;
        }

        GameObject squadUnitPrefab = _unitPrefabs[unitIndex];
        Unit unitComp = squadUnitPrefab.GetComponent<Unit>();
        int cost = (unitComp != null) ? unitComp.UnitCost : 0;

        Player player = gm.GetPlayerByTeam(team);
        bool free = false;
        if (useVoucher && player != null && player.FreeSquadVouchers.Contains(squadUnitPrefab))
        {
            player.ConsumeFreeSquadVoucher(squadUnitPrefab);
            free = true;
        }

        if (!free)
        {
            if (!gm.CanAfford(cost, team)) return;
            if (!gm.SpendCredits(cost, team)) return;
        }
        Vector3 teamForward = (team == Team.Player1) ? p1Forward : p2Forward;
        Quaternion rot = Quaternion.LookRotation(teamForward, Vector3.up);

        GameObject squadGameobject = Instantiate(_squdPrefab, worldPosition, rot);
        NetworkObject squadNetworkObject = squadGameobject.GetComponent<NetworkObject>();
        if (squadNetworkObject == null)
        {
            Debug.LogWarning("PlacementNetwork: Squad prefab does not have a NetworkObject component");
            Destroy(squadGameobject);
            return;
        }

        Squad squad = squadGameobject.GetComponent<Squad>();
        squad.nbrOfUnits = Mathf.Clamp(unitCount, 1, 16);
        squad.unitPrefab = _unitPrefabs[unitIndex];
        squad.team = team;
        squadNetworkObject.Spawn(true);
        squad.SpawnUnit();
        LevelGrid.Instance.AddSquadAtGridPosition(gridPos, squad);

        if (free)
        {
            MirrorVoucherRemoveClientRpc(unitIndex, team == Team.Player1 ? 1 : 2, new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { sender } }
            });
        }
    }
    public int IndexOfUnit(GameObject prefab)
    {
        for (int i = 0; i < _unitPrefabs.Length; i++)
            if (_unitPrefabs[i] == prefab) return i;
        return -1;
    }

    public GameObject GetUnitByIndex(int i) =>
    (i >= 0 && i < _unitPrefabs.Length) ? _unitPrefabs[i] : null;

    [ClientRpc]
    private void MirrorVoucherRemoveClientRpc(int unitIndex, int teamInt, ClientRpcParams _ = default)
    {
        GameManager gm = _radioGameplay.GameManager;
        GameObject prefab = GetUnitByIndex(unitIndex);
        if (gm == null || prefab == null) return;

        Team team = (Team)teamInt;
        gm.GetPlayerByTeam(team)?.ConsumeFreeSquadVoucher(prefab);
        gm.NotifyVoucherChanged();
    }

}
