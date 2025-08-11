using System;
using UnityEngine;

namespace CT.UI.Audio
{
    public static class UIAudioEventHub
    {
        public static event Action OnUIButtonHovered;
        public static event Action OnUIButtonClicked;

        public static void ButtonHovered() => OnUIButtonHovered?.Invoke();
        public static void ButtonClicked() => OnUIButtonClicked?.Invoke();
    }
}
