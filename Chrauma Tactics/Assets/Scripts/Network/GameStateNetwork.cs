using Unity.Netcode;
using UnityEngine;

public class GameStateNetwork : NetworkBehaviour
{
    /*written by server, fo everyone to read*/
    public NetworkVariable<int> P1Credits = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> P2Credits = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> P1HP = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> P2HP = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private Rd_Gameplay _rdGameplay;

    void Awake()
    {
        _rdGameplay.SetGameStateNetwork(this);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        var gm = _rdGameplay.GameManager;
        if (gm != null)
        {
            P1Credits.Value = gm.player1.Credits;
            P2Credits.Value = gm.player2.Credits;
            P1HP.Value = gm.player1.HP;
            P2HP.Value = gm.player2.HP;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestFullStateSyncServerRpc()
    {
        var gm = _rdGameplay.GameManager;
        if (gm != null)
        {
            P1Credits.Value = gm.player1.Credits;
            P2Credits.Value = gm.player2.Credits;
            P1HP.Value = gm.player1.HP;
            P2HP.Value = gm.player2.HP;
        }
    }
}
