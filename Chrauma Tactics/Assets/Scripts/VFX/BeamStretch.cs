using UnityEngine;

public class BeamStretch : MonoBehaviour
{

    [Header("Prefab properties")]
    [SerializeField] float unitLength = 1f;
    [SerializeField] float thickness = 1f;

    Vector3 _axisVector => Vector3.right;

    public void UpdateBeam(Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        if (distance < 0.0001f)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        /*milieu*/
        transform.position = start + 0.5f * direction;
        transform.rotation = Quaternion.FromToRotation(_axisVector, direction.normalized);

        float axisScale = distance / Mathf.Max(0.0001f, unitLength);
        Vector3 scale = new Vector3(axisScale, thickness, thickness);
        transform.localScale = scale;
    }

    public void Hide() => gameObject.SetActive(false);
    public void SetScale(float t) => thickness = unitLength = t;
}
