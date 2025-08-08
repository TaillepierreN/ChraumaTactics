using CT.Gameplay;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace CT.UI.UnitSelectionUI
{
    public class UnitUISelector : MonoBehaviour
    {
        [SerializeField] private GameObject unitPrefab;
        [SerializeField] private TMP_Text unitCost;
        [SerializeField] private int unitNum;
        [SerializeField] private string UnitCost;

        void Start()
        {
            unitCost.text = UnitCost.ToString();
        }

        public void OnButtonClicked()
        {
            UnitPlacer.Instance.StartPlacingUnit(unitPrefab, unitNum);
        }
    }
}
