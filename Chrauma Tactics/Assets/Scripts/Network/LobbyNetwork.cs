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

        // --- UI buttons ---
        public void StartHost()
        {
            var nm = NetworkManager.Singleton;
            if (!nm) return;

            if (!nm.StartHost()) return;

            HookEvents();
            _statusPanel?.SetActive(true);
            BroadcastCount();
        }

        public void StartClient()
        {
            var nm = NetworkManager.Singleton;
            if (!nm) return;

            RegisterCountHandlers();
            if (!nm.StartClient()) return;
			StartCoroutine(CheckIfConnected());
        }
		private IEnumerator CheckIfConnected()
		{
			yield return new WaitForSeconds(2f);
			Debug.Log($"is client? {NetX.IsClient}, is connected/listening? {NetX.IsListening}");

            _statusPanel?.SetActive(true);
            RequestCountFromHost();
		}

        public void Shutdown()
		{
			UnhookEvents();
			NetworkManager.Singleton?.Shutdown();
			SetCountLabel(0);
			_buttonStart?.SetActive(false);
			_statusPanel?.SetActive(false);
		}

        public void StartGame()
        {
            var nm = NetworkManager.Singleton;
            if (!nm || !nm.IsServer) return;

            UI.SceneLoader.BeginNetwork("SampleScene");
            nm.SceneManager.LoadScene("SampleScene",
                UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        public void GoBack()
        {
            SceneLoader.LoadOffline("GameMenu");
        }

        void HookEvents()
        {
            var nm = NetworkManager.Singleton;
            if (!nm) return;

            nm.OnServerStarted += OnServerStarted;
            nm.OnClientConnectedCallback += OnClientConnected;
            nm.OnClientDisconnectCallback += OnClientDisconnected;

            RegisterCountHandlers();
        }

        void UnhookEvents()
        {
            var nm = NetworkManager.Singleton;
            if (!nm) return;

            nm.OnServerStarted -= OnServerStarted;
            nm.OnClientConnectedCallback -= OnClientConnected;
            nm.OnClientDisconnectCallback -= OnClientDisconnected;

            UnregisterCountHandlers();
        }

        void RegisterCountHandlers()
        {
            var nm = NetworkManager.Singleton;
            if (nm?.CustomMessagingManager == null) return;


            nm.CustomMessagingManager.UnregisterNamedMessageHandler(MsgLobbyCount);
            nm.CustomMessagingManager.RegisterNamedMessageHandler(MsgLobbyCount, OnCountMessage);

            if (nm.IsServer)
            {
                nm.CustomMessagingManager.UnregisterNamedMessageHandler(MsgRequestCount);
                nm.CustomMessagingManager.RegisterNamedMessageHandler(MsgRequestCount,
            (sender, _) => SendCountToClient(sender));
            }
        }

        void UnregisterCountHandlers()
        {
            var nm = NetworkManager.Singleton;
            if (nm?.CustomMessagingManager == null) return;

            nm.CustomMessagingManager.UnregisterNamedMessageHandler(MsgLobbyCount);
            if (nm.IsServer)
                nm.CustomMessagingManager.UnregisterNamedMessageHandler(MsgRequestCount);
        }

        void OnServerStarted() => BroadcastCount();
        void OnClientConnected(ulong clientId)
        {
            var nm = NetworkManager.Singleton;
            if (nm && nm.IsServer)
            {
                BroadcastCount();
                SendCountToClient(clientId);
            }
        }
        void OnClientDisconnected(ulong _) { if (NetworkManager.Singleton.IsServer) BroadcastCount(); }

        void SendCountToClient(ulong clientId)
        {
            var nm = NetworkManager.Singleton;
            if (nm?.CustomMessagingManager == null) return;

            int count = nm.ConnectedClientsIds.Count;
            using var w = new FastBufferWriter(sizeof(int), Allocator.Temp);
            w.WriteValueSafe(count);
            nm.CustomMessagingManager.SendNamedMessage("ct/lobby/count", clientId, w);
        }
        void RequestCountFromHost()
        {
            var nm = NetworkManager.Singleton;
            if (nm?.CustomMessagingManager == null) return;

            using var w = new FastBufferWriter(0, Allocator.Temp);
            nm.CustomMessagingManager.SendNamedMessage(MsgRequestCount, NetworkManager.ServerClientId, w);
        }

        void BroadcastCount()
        {
            var nm = NetworkManager.Singleton;
            if (!nm || !nm.IsServer) return;

            int count = ComputeCount();
            SetCountLabel(count);
            UpdateStartButton(count);

            if (nm.CustomMessagingManager == null) return;
            using var w = new FastBufferWriter(sizeof(int), Allocator.Temp);
            w.WriteValueSafe(count);
            nm.CustomMessagingManager.SendNamedMessageToAll(MsgLobbyCount, w);
        }


        int ComputeCount()
        {
            var nm = NetworkManager.Singleton;
            return (nm != null) ? nm.ConnectedClientsIds.Count : 0;
        }

        // --- UI helpers ---
        void OnCountMessage(ulong _, FastBufferReader reader)
        {
            if (!reader.TryBeginRead(sizeof(int))) return;
            reader.ReadValueSafe(out int count);
            SetCountLabel(count);
            UpdateStartButton(count);
        }

        void SetCountLabel(int count)
        {
            if (_nbrPlayerText)
                _nbrPlayerText.text = $"Connected Players: {count}";
        }

        void UpdateStartButton(int count)
        {
            var nm = NetworkManager.Singleton;
            bool canStart = nm && nm.IsServer && count >= 2;
            if (_buttonStart) _buttonStart.SetActive(canStart);
        }
    }
}
