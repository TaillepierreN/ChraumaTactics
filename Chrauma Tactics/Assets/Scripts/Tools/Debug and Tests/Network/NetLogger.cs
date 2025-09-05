using Unity.Netcode;
using UnityEngine;

public class NetLogger : MonoBehaviour
{
    void Start()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null) return;

        nm.OnServerStarted += () =>
        {
            Debug.Log("[NET] Server started. IsHost=" + nm.IsHost);
        };
        nm.OnClientConnectedCallback += (clientId) =>
        {
            Debug.Log("[NET] Client connected: " + clientId +
                      (clientId == nm.LocalClientId ? " (local)" : ""));
        };
        nm.OnClientDisconnectCallback += (clientId) =>
        {
            Debug.Log("[NET] Client disconnected: " + clientId);
        };
    }
}
