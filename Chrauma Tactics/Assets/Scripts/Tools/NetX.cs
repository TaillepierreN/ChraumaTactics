using Unity.Netcode;

namespace CT.Tools
{
    /// <summary>Centralized helpers for NetworkManager state.</summary>
    public static class NetX
    {
        public static NetworkManager NM => NetworkManager.Singleton;

        /// <summary>NetworkManager exists and is running a session.</summary>
        public static bool IsListening => NM && NM.IsListening;

        /// <summary>We are in any online session (server or client).</summary>
        public static bool InSession => IsListening && (NM.IsServer || NM.IsClient);

        /// <summary>True on the server (host counts as server).</summary>
        public static bool IsServer => IsListening && NM.IsServer;

        /// <summary>True on a pure client (not host).</summary>
        public static bool IsClient => IsListening && NM.IsClient && !NM.IsServer;

        /// <summary>True on host.</summary>
        public static bool IsHost => IsListening && NM.IsHost;

        /// <summary>No session is running (single-player/offline).</summary>
        public static bool IsOffline => !IsListening;

        /// <summary>Authority = offline OR (online & server). Use this to gate game logic.</summary>
        public static bool IsAuthoritative => IsOffline || IsServer;

        /// <summary>Convenience: try to spawn only when allowed.</summary>
        public static bool TrySpawn(this NetworkObject no, bool destroyWithScene = true)
        {
            if (!no) return false;
            if (IsServer && !no.IsSpawned) { no.Spawn(destroyWithScene); return true; }
            return false;
        }
    }
}
