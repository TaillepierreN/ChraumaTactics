using UnityEngine;
using Unity.Netcode;
using CT.Tools;
public class NetBootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject _netServicesPrefab;

    private void Start()
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null || !nm.IsServer || !nm.IsListening)
            return;

        if (NetX.IsServer)
        {
            GameObject netServices = Instantiate(_netServicesPrefab);
            NetworkObject networkObject = netServices.GetComponent<NetworkObject>();
            if (networkObject != null && !networkObject.IsSpawned)
                networkObject.Spawn(false);
        }
    }
}
