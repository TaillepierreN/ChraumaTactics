using CT.UI.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIAudioButtonEvents : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] Button buttonComponent;

    public void OnPointerEnter(PointerEventData e) => UIAudioEventHub.ButtonHovered();
    private void OnClick() => UIAudioEventHub.ButtonClicked();

	void Awake()
	{
        buttonComponent.onClick.AddListener(OnClick);
	}
}
