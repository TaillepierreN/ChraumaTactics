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
        [SerializeField] private Rd_Gameplay _radioGameplay;
        private GameManager _gameManager;



        void Start()
        {
            _gameManager = _radioGameplay.GameManager;
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
            if (_gameManager.SpendCredits(unitPrefab.GetComponent<Unit>().UnitCost))
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
