using CT.Gameplay;
using UnityEngine.UI;
using UnityEngine;

namespace CT.UI.UnitSelectionUI
{
    public class UnitUISelector : MonoBehaviour
    {
        [SerializeField] private GameObject unitPrefab;
        [SerializeField] private Image unitImage;
        [SerializeField] private Sprite unitIcon;

        void Start()
        {
            unitImage.sprite = unitIcon;
        }

        public void OnButtonClicked()
        {
            UnitPlacer.Instance.StartPlacingUnit(unitPrefab);
        }
    }
}
