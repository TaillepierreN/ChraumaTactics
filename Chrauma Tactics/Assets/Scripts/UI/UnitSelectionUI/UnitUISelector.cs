using CT.Gameplay;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.XR;
using System.Collections;

namespace CT.UI.UnitSelectionUI
{
    public class UnitUISelector : MonoBehaviour
    {
        [SerializeField] private GameObject unitPrefab;
        [SerializeField] private TMP_Text unitCost;
        [SerializeField] private int unitNum;
        [SerializeField] private Rd_Gameplay _radioGameplay;
        [SerializeField] private Button _button;
        private Coroutine _listenCo;
        private bool _isSubscribed = false;
        public Team Team;
        private GameManager _gameManager;

        #region Unity Methods
        private void Awake()
        {
            _button = GetComponent<Button>();
        }
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
        void OnEnable()
        {
            _listenCo = StartCoroutine(ListenToChange());
            if (_gameManager != null)
                CheckIfCanAfford();
        }


        void OnDisable()
        {
            if (_listenCo != null)
            {
                StopCoroutine(_listenCo);
                _listenCo = null;
            }
            Unsubscribe();
        }

        #endregion
        #region Listeners

        private void Subscribe()
        {
            if (_isSubscribed) return;

            _radioGameplay.RoundManager.OnRoundChanged += HandleRoundChanged;
            Debug.Log("subbed");
            if (Team == Team.Player1)
                _radioGameplay.GameManager.P1CreditsChanged += CreditsChanged;
            else if (Team == Team.Player2)
                _radioGameplay.GameManager.P2CreditsChanged += CreditsChanged;

            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isSubscribed || _radioGameplay == null) return;

            _radioGameplay.RoundManager.OnRoundChanged -= HandleRoundChanged;
            if (Team == Team.Player1)
                _radioGameplay.GameManager.P1CreditsChanged -= CreditsChanged;
            else if (Team == Team.Player2)
                _radioGameplay.GameManager.P2CreditsChanged -= CreditsChanged;
            _isSubscribed = false;
        }
        #endregion
        #region Helpers
        private IEnumerator ListenToChange()
        {
            yield return new WaitUntil(() =>
                isActiveAndEnabled &&
                _radioGameplay != null &&
                _radioGameplay.RoundManager != null &&
                _radioGameplay.GameManager != null);

            if (!isActiveAndEnabled) yield break;

            Subscribe();
        }

        private void HandleRoundChanged(int newRound, RoundPhase newPhase)
        {
            CheckIfCanAfford();
        }

        private void CreditsChanged(int newCredits)
        {
            CheckIfCanAfford();
        }

        private void CheckIfCanAfford()
        {
            if (_gameManager.CanAfford(unitPrefab.GetComponent<Unit>().UnitCost, Team))
                _button.interactable = true;
            else
                _button.interactable = false;
        }
        public void OnButtonClicked()
        {
            if (_gameManager.SpendCredits(unitPrefab.GetComponent<Unit>().UnitCost, Team))
            {
                Debug.Log($"Placing unit: {unitPrefab.name}");
                UnitPlacer.Instance.StartPlacingUnit(unitPrefab, unitNum);
            }
            else
            {
                return;

            }
        }
        #endregion
    }
}
