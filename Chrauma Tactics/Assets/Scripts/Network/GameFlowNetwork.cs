using UnityEngine;
using Unity.Netcode;

public class GameFlowNetwork : NetworkBehaviour
{
    public static GameFlowNetwork Instance { get; private set; }
    [SerializeField] private Rd_Gameplay _radioGameplay;
    private bool _gameStarted = false;
    /*bool to check who has locked in their commander*/
    private NetworkVariable<bool> _p1Selected = new(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> _p2Selected = new(writePerm: NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        Instance = this;
        //Debug.Log($"[NET] GameFlowNetwork spawned (IsServer={IsServer})");
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this)
            Instance = null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void NotifyCommanderSelectedServerRpc(Team team, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        bool beforeAny = !_p1Selected.Value && !_p2Selected.Value;

        switch (team)
        {
            case Team.Player1: _p1Selected.Value = true; break;
            case Team.Player2: _p2Selected.Value = true; break;
        }

        if (beforeAny && !(_p1Selected.Value && _p2Selected.Value))
        {
            ClientRpcParams target =
            new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { rpcParams.Receive.SenderClientId } }
            };
            ShowWaitingClientRpc(target);
        }

        TryStartAfterBothSelected();
    }

    private void TryStartAfterBothSelected()
    {
        if (_gameStarted) return;

        if (_p1Selected.Value && _p2Selected.Value)
        {
            _p1Selected.Value = false;
            _p2Selected.Value = false;

            _radioGameplay?.RoundManager?.StartGame();
            _gameStarted = true;
        }
    }

    [ClientRpc]
    private void ShowWaitingClientRpc(ClientRpcParams rpcParams = default)
    {
        _radioGameplay.CommanderSelectionmenu?.ShowWaiting();
    }
}
