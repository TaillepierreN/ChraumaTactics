using UnityEngine;

public class EnvironmentSelector : MonoBehaviour
{
    [Header("Environment variants (prefabs)")]
    [SerializeField] private GameObject[] _variants;

    [Header("Skyboxes (materials)")]
    [SerializeField] private Material[] _skyboxes;

    private GameObject _spawned;

    void Start()
    {
        if (_variants == null || _variants.Length == 0) return;

        int envIdx = Random.Range(0, _variants.Length);
        _spawned = Instantiate(_variants[envIdx], Vector3.zero, Quaternion.identity, transform);

        if (_skyboxes != null && _skyboxes.Length > 0)
        {
            int skyIdx = Random.Range(0, _skyboxes.Length);
            ApplySkybox(_skyboxes[skyIdx]);
        }

    }

    public void ApplySkybox(Material skyMat)
    {
        if (!skyMat) return;
        RenderSettings.skybox = skyMat;
        DynamicGI.UpdateEnvironment();
    }
}
