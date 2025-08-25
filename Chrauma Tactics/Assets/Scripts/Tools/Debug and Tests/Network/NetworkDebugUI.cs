using UnityEngine;
using Unity.Netcode;

public class NetworkDebugUI : MonoBehaviour
{
	    public void StartHost()
    {
        bool ok = NetworkManager.Singleton.StartHost();
        Debug.LogWarning($"[NET] StartHost() => {ok} | IsServer={NetworkManager.Singleton.IsServer} IsClient={NetworkManager.Singleton.IsClient}");
    }
    public void StartClient()
    {
        bool ok = NetworkManager.Singleton.StartClient();
        Debug.LogWarning($"[NET] StartClient() => {ok}");
    }
    public void StartServer()
    {
        bool ok = NetworkManager.Singleton.StartServer();
        Debug.LogWarning($"[NET] StartServer() => {ok} | IsServer={NetworkManager.Singleton.IsServer}");
    }
    public void Shutdown()
    {
        Debug.LogWarning("[NET] Shutdown()");
        NetworkManager.Singleton.Shutdown();
    }
}

