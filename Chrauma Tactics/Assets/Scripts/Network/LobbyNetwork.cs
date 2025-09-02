using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using CT.UI;
using CT.Tools;
using System.Collections;

namespace CT.Network
{
    public class LobbyNetwork : MonoBehaviour
    {
        private const string MsgLobbyCount = "ct/lobby/count";
        private const string MsgRequestCount = "ct/lobby/request";

        [SerializeField] GameObject _statusPanel;
        [SerializeField] TMP_Text _nbrPlayerText;
        [SerializeField] GameObject _buttonStart;
        [SerializeField] GameObject _connectingPanel;
        [SerializeField] GameObject _failedConnectPanel;

        // --- UI buttons ---
        public void StartHost()
        {
            if (!NetX.NM) return;

            HookEvents();
            if (!NetX.NM.StartHost()) return;

            _statusPanel?.SetActive(true);
            BroadcastCount();
        }

        public void StartClient()
        {
            if (!NetX.NM) return;

            if (NetX.NM.NetworkConfig.NetworkTransport is Unity.Netcode.Transports.UTP.UnityTransport utp)
            {
                utp.MaxConnectAttempts = 2;
                utp.ConnectTimeoutMS = 1500;
            }

            HookEvents();

            _connectingPanel.SetActive(true);

            if (!NetX.NM.StartClient())
            {
                _statusPanel?.SetActive(false);
                _failedConnectPanel.SetActive(true);
                return;
            }

            StartCoroutine(ConnectWatchdog(5f));
        }

        private IEnumerator ConnectWatchdog(float seconds)
        {
            float end = Time.time + seconds;
            while (Time.time < end)
            {
                if (NetX.NM && NetX.NM.IsConnectedClient) yield break;
                yield return null;
            }
            NetX.NM?.Shutdown();
            _statusPanel?.SetActive(false);
            _failedConnectPanel?.SetActive(true);
        }

        public void Shutdown()
        {
            UnhookEvents();
            NetworkManager.Singleton?.Shutdown();
            SetCountLabel(0);
            _buttonStart?.SetActive(false);
            _statusPanel?.SetActive(false);
            _failedConnectPanel?.SetActive(false);
            SceneLoader.CompleteNetwork();
        }

        public void StartGame()
        {
            if (!NetX.NM || !NetX.NM.IsServer) return;

            NetX.NM.SceneManager.LoadScene("SampleScene",
                UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        public void GoBack()
        {
            SceneLoader.LoadOffline("GameMenu");
        }

        private void HookEvents()
        {
            if (!NetX.NM) return;

            NetX.NM.OnServerStarted -= OnServerStarted;
            NetX.NM.OnClientConnectedCallback -= OnClientConnected;
            NetX.NM.OnClientDisconnectCallback -= OnClientDisconnected;

            NetX.NM.OnServerStarted += OnServerStarted;
            NetX.NM.OnClientConnectedCallback += OnClientConnected;
            NetX.NM.OnClientDisconnectCallback += OnClientDisconnected;

            RegisterCountHandlers();
        }

        private void UnhookEvents()
        {
            if (!NetX.NM) return;

            NetX.NM.OnServerStarted -= OnServerStarted;
            NetX.NM.OnClientConnectedCallback -= OnClientConnected;
            NetX.NM.OnClientDisconnectCallback -= OnClientDisconnected;

            UnregisterCountHandlers();
        }

        private void RegisterCountHandlers()
        {
            if (NetX.NM?.CustomMessagingManager == null) return;


            NetX.NM.CustomMessagingManager.UnregisterNamedMessageHandler(MsgLobbyCount);
            NetX.NM.CustomMessagingManager.RegisterNamedMessageHandler(MsgLobbyCount, OnCountMessage);

            if (NetX.NM.IsServer)
            {
                NetX.NM.CustomMessagingManager.UnregisterNamedMessageHandler(MsgRequestCount);
                NetX.NM.CustomMessagingManager.RegisterNamedMessageHandler(MsgRequestCount,
            (sender, _) =>
            {
                if (NetX.NM.IsServer)
                    SendCountToClient(sender);
            });
            }
        }

        private void UnregisterCountHandlers()
        {
            if (NetX.NM?.CustomMessagingManager == null) return;

            NetX.NM.CustomMessagingManager.UnregisterNamedMessageHandler(MsgLobbyCount);
            if (NetX.NM.IsServer)
                NetX.NM.CustomMessagingManager.UnregisterNamedMessageHandler(MsgRequestCount);
        }

        private void OnServerStarted() => BroadcastCount();
        private void OnClientConnected(ulong clientId)
        {
            if (NetX.NM && NetX.NM.IsServer)
            {
                BroadcastCount();
                SendCountToClient(clientId);
            }
            else if (clientId == NetX.NM.LocalClientId)
            {
                _statusPanel.SetActive(true);
                _connectingPanel.SetActive(false);
                RequestCountFromHost();
            }
        }
        private void OnClientDisconnected(ulong clientId)
        {
            if (!NetX.NM) return;

            if (NetX.NM.IsServer)
            {
                BroadcastCount();
            }
            else if (clientId == NetX.NM.LocalClientId)
            {

                _statusPanel?.SetActive(false);
                _failedConnectPanel.SetActive(true);
                _connectingPanel.SetActive(false);
                SetCountLabel(0);
            }
        }

        private void SendCountToClient(ulong clientId)
        {
            if (NetX.NM?.CustomMessagingManager == null) return;

            int count = NetX.NM.ConnectedClientsIds.Count;
            using var w = new FastBufferWriter(sizeof(int), Allocator.Temp);
            w.WriteValueSafe(count);
            NetX.NM.CustomMessagingManager.SendNamedMessage("ct/lobby/count", clientId, w);
        }
        private void RequestCountFromHost()
        {
            if (NetX.NM?.CustomMessagingManager == null) return;

            using var w = new FastBufferWriter(0, Allocator.Temp);
            NetX.NM.CustomMessagingManager.SendNamedMessage(MsgRequestCount, NetworkManager.ServerClientId, w);
        }

        private void BroadcastCount()
        {
            if (!NetX.NM || !NetX.NM.IsServer) return;

            int count = ComputeCount();
            SetCountLabel(count);
            UpdateStartButton(count);

            if (NetX.NM.CustomMessagingManager == null) return;
            using var w = new FastBufferWriter(sizeof(int), Allocator.Temp);
            w.WriteValueSafe(count);
            NetX.NM.CustomMessagingManager.SendNamedMessageToAll(MsgLobbyCount, w);
        }


        private int ComputeCount()
        {
            return (NetX.NM != null) ? NetX.NM.ConnectedClientsIds.Count : 0;
        }

        // --- UI helpers ---
        private void OnCountMessage(ulong _, FastBufferReader reader)
        {
            if (!reader.TryBeginRead(sizeof(int))) return;
            reader.ReadValueSafe(out int count);
            SetCountLabel(count);
            UpdateStartButton(count);
        }

        private void SetCountLabel(int count)
        {
            if (_nbrPlayerText)
                _nbrPlayerText.text = $"Connected Players: {count}";
        }

        private void UpdateStartButton(int count)
        {
            bool canStart = NetX.NM && NetX.NM.IsServer && count >= 2;
            if (_buttonStart) _buttonStart.SetActive(canStart);
        }
    }
}
