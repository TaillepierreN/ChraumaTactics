using UnityEngine;
using Unity.Netcode;

public class GameFlowNetwork : NetworkBehaviour
{
    public static GameFlowNetwork Instance { get; private set; }
    [SerializeField] private Rd_Gameplay _radioGameplay;
    private bool _gameStarted = false;

    public override void OnNetworkSpawn()
    {
        Instance = this;
        Debug.Log($"[NET] GameFlowNetwork spawned (IsServer={IsServer})");
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this)
            Instance = null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc()
    {
        if (!IsServer || _gameStarted) return;
        _radioGameplay.RoundManager.StartGame();
        _gameStarted = true;
    }
}
