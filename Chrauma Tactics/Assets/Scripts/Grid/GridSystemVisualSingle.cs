using UnityEngine;

public class GridSystemVisualSingle : MonoBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private GameObject overlayObject;
    [SerializeField] private MeshRenderer overlayRenderer;

    public void Show()
    {
        _meshRenderer.enabled = true;
    }
    public void Hide()
    {
        _meshRenderer.enabled = false;
    }
    public void ShowOverlay(Color color)
    {
        overlayRenderer.material.color = color;
        overlayObject.SetActive(true);
    }

    public void HideOverlay()
    {
        overlayObject.SetActive(false);
    }
}
