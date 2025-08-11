using UnityEngine;

namespace CT.UI.Audio
{
    public class UIAudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource AudioSource;
        [SerializeField] private AudioClip HoverSound;
        [SerializeField] private AudioClip ClickSound;

        private void OnEnable()
        {
            UIAudioEventHub.OnUIButtonHovered += PlayHover;
            UIAudioEventHub.OnUIButtonClicked += PlayClick;
        }
        private void OnDisable()
        {
            UIAudioEventHub.OnUIButtonHovered -= PlayHover;
            UIAudioEventHub.OnUIButtonClicked -= PlayClick;
        }

        private void PlayHover()
        {
            if (HoverSound && AudioSource)
                AudioSource.PlayOneShot(HoverSound);
            else
                Debug.Log("no hover sound or audio source");
        }

        private void PlayClick()
        {
            if (ClickSound && AudioSource)
                AudioSource.PlayOneShot(ClickSound);
            else
                Debug.Log("no click sound or audio source");
        }

    }
}