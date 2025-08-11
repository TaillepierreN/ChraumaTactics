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



        void Start()
        {
            if (unitPrefab != null)
            {
                Unit unit = unitPrefab.GetComponent<Unit>();
                if (unit != null)
                {
                    unitCost.text = unit.UnitCost;
                }
            }
        }

        public void OnButtonClicked()
        {
            UnitPlacer.Instance.StartPlacingUnit(unitPrefab, unitNum);
        }
    }
}
