using Unity.Netcode;
using UnityEngine;

namespace CT.Units.Attacks
{
    /// Need to attach this on the same GO as Unit and Continuous
    public class ContinuousNetProxy : NetworkBehaviour
    {
        [SerializeField] private Continuous _continuous;

        void Awake()
        {
            if (!_continuous) _continuous = GetComponent<Continuous>();
        }

        /// <summary>
        /// Start continuous beams on server and all clients for a target unit
        /// </summary>
        /// <param name="target">target unit to aim at</param>
        public void StartAllBeamsNetworked(Unit target)
        {
            if (!IsSpawned || target == null)
                return;

            if (IsServer)
            {
                _continuous.StartAllBeamsLocal(target);
                StartAllBeamsClientRpc(target.NetworkObjectId);
            }
            else
            {
                StartAllBeamsServerRpc(target.NetworkObjectId);
            }
        }

        /// <summary>
        /// stop all continuous beams on server and all clients
        /// </summary>
        public void StopAllBeamsNetworked()
        {
            if (!IsSpawned)
                return;

            if (IsServer)
            {
                _continuous.StopAllBeamsLocal();
                StopAllBeamsClientRpc();
            }
            else
            {
                StopAllBeamsServerRpc();
            }
        }

        #region Remote Procedure Calls

        /// <summary>
        /// starts beams on server and broadcasts to clients.
        /// </summary>
        /// <param name="targetId"></param>
        [ServerRpc(RequireOwnership = false)]
        private void StartAllBeamsServerRpc(ulong targetId)
        {
            if (!IsServer)
                return;

            Unit unit = GetUnitFromNetwork(targetId);
            if (!unit)
                return;

            _continuous.StartAllBeamsLocal(unit);
            StartAllBeamsClientRpc(targetId);
        }

        /// <summary>
        /// start beam locally on non host clients(only visual)
        /// </summary>
        /// <param name="targetId"></param>
        [ClientRpc]
        private void StartAllBeamsClientRpc(ulong targetId)
        {
            if (IsServer)
                return;

            Unit unit = GetUnitFromNetwork(targetId);
            if (!unit)
                return;

            _continuous.StartAllBeamsLocal(unit);
        }

        /// <summary>
        /// stop beam locally on server and broadcast to client(only visual)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void StopAllBeamsServerRpc()
        {
            if (!IsServer)
                return;

            _continuous.StopAllBeamsLocal();
            StopAllBeamsClientRpc();
        }

        /// <summary>
        /// stop beam locally on non host client(only visual)
        /// </summary>
        [ClientRpc]
        private void StopAllBeamsClientRpc()
        {
            if (IsServer)
                return;

            _continuous.StopAllBeamsLocal();
        }

        #endregion

        /// <summary>
        /// get a unit by its networkobjectId from spawn manager
        /// </summary>
        /// <param name="netId">target networkObjectId</param>
        /// <returns>the unit component if foud,otherwise null</returns>
        private Unit GetUnitFromNetwork(ulong netId)
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netId, out NetworkObject networkObj))
                return null;

            return networkObj ? networkObj.GetComponent<Unit>() : null;
        }
    }
}
