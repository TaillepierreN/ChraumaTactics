using UnityEngine;
using Unity.Netcode;
using CT.Tools;

public class EnvironmentSelector : NetworkBehaviour
{
    [Header("Environment variants (prefabs)")]
    [SerializeField] private GameObject[] _variants;

    [Header("Skyboxes (materials)")]
    [SerializeField] private Material[] _skyboxes;

    private GameObject _spawned;
    private readonly NetworkVariable<int> _envIdx = new(writePerm: NetworkVariableWritePermission.Server);
    private readonly NetworkVariable<int> _skyIdx = new(writePerm: NetworkVariableWritePermission.Server);


    void Start()
    {
        if (!NetX.IsListening)
            ApplyLocal();
    }

    public override void OnNetworkSpawn()
    {
        _envIdx.OnValueChanged += (_, __) => ApplyMultiplayer();
        _skyIdx.OnValueChanged += (_, __) => ApplyMultiplayer();
        if (IsServer)
        {
            int env = (_variants != null && _variants.Length > 0) ? Random.Range(0, _variants.Length) : -1;
            int sky = (_skyboxes != null && _skyboxes.Length > 0) ? Random.Range(0, _skyboxes.Length) : -1;

            if (env >= _variants.Length)
                env = _variants.Length - 1;
            if (sky >= _skyboxes.Length)
                sky = _skyboxes.Length - 1;

            _envIdx.Value = env;
            _skyIdx.Value = sky;
        }
        ApplyMultiplayer();
    }

    private bool ApplyLocal()
    {
        if (_variants == null || _variants.Length == 0) return false;

        int envIdx = Random.Range(0, _variants.Length);
        SpawnEnvironment(envIdx);

        if (_skyboxes != null && _skyboxes.Length > 0)
        {
            int skyIdx = Random.Range(0, _skyboxes.Length);
            ApplySkybox(_skyboxes[skyIdx]);
        }

        return true;
    }

    private void ApplyMultiplayer()
    {
        int envIdx = _envIdx.Value;
        int skyIdx = _skyIdx.Value;

        if (envIdx >= 0 && _variants != null && envIdx < _variants.Length)
            SpawnEnvironment(envIdx);

        if (skyIdx >= 0 && _skyboxes != null && skyIdx < _skyboxes.Length)
            ApplySkybox(_skyboxes[skyIdx]);
    }

    private void SpawnEnvironment(int envIdx)
    {
        _spawned = Instantiate(_variants[envIdx], Vector3.zero, Quaternion.identity, transform);
    }

    public void ApplySkybox(Material skyMat)
    {
        if (!skyMat) return;
        RenderSettings.skybox = skyMat;
        DynamicGI.UpdateEnvironment();
    }
}
