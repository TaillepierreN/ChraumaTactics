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
        [SerializeField] private RoundManager roundManager;



        void Start()
        {
            if (unitPrefab != null)
            {
                Unit unit = unitPrefab.GetComponent<Unit>();
                if (unit != null)
                {
                    unitCost.text = unit.UnitCost.ToString();
                }
            }
        }

        public void OnButtonClicked()
        {
            if (roundManager.SpendCredits(unitPrefab.GetComponent<Unit>().UnitCost))
            {
                Debug.Log($"Placing unit: {unitPrefab.name}");
                UnitPlacer.Instance.StartPlacingUnit(unitPrefab, unitNum);
            }
            else
            {
                return;
            }
        }
    }
}
