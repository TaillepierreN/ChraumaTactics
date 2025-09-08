using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public static class NetRemover
{
    public static void StripNetcodeComponents(GameObject go)
    {
        NetworkObject no = go.GetComponent<NetworkObject>();
        if (no)
        {
            no.enabled = false;
            Object.Destroy(no);
        }

        NetworkTransform nt = go.GetComponent<NetworkTransform>();
        if (nt)
        {
            nt.enabled = false;
            Object.Destroy(nt);
        }

        foreach (NetworkBehaviour nb in go.GetComponents<NetworkBehaviour>())
        {
            //Debug.Log($"got {nb} in {go}");
            nb.enabled = false;
            Object.Destroy(nb);
        }
        foreach (NetworkBehaviour nbc in go.GetComponentsInChildren<NetworkBehaviour>(true))
            if (nbc)
            {
                nbc.enabled = false;
                Object.Destroy(nbc);
            }

        NetworkObject[] nobs = go.GetComponentsInChildren<NetworkObject>(true);
        foreach (NetworkObject noc in nobs)
            if (noc)
            {
                noc.enabled = false;
                Object.Destroy(noc);
            }
    }
}
