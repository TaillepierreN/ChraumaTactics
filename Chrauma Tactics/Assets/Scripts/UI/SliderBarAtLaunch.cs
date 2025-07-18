using UnityEngine;
using UnityEngine.UI;

public class SliderRunTo1 : MonoBehaviour
{
    public Slider slider;
    public float speed = 0.5f;
    public GameObject loadingPanel;
    public GameObject nextPanel;

    private float time = 0f;
    private bool loading = true;

    void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        slider.value = 0f;
    }

    void Update()
    {
        if (!loading) return;

        time += Time.deltaTime * speed;
        slider.value = Mathf.Clamp01(time);

        if (slider.value >= 1f)
        {
            loading = false;
            EndLoading();
        }
    }

    void EndLoading()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        if (nextPanel != null)
            nextPanel.SetActive(true);
    }
}
