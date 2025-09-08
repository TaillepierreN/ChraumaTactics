using CT.Gameplay;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Collections;
using Unity.Netcode;
using CT.Tools;

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
        private GameManager _gameManager;
        private GameStateNetwork _stateNet;
        private int _cost;
        public Team Team = Team.Player1;

        

        #region Unity Methods
        private void Awake()
        {
            _button = GetComponent<Button>();
        }
        void Start()
        {
            _gameManager = _radioGameplay.GameManager;
            _stateNet = _radioGameplay.GameStateNetwork;
            if (NetX.IsListening && NetX.NM && !NetX.IsServer)
                Team = Team.Player2;
            if (unitPrefab != null)
            {
                Unit unit = unitPrefab.GetComponent<Unit>();
                if (unit != null)
                {
                    unitCost.text = unit.UnitCost.ToString();
                    _cost = unit.UnitCost;
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

            /*offline*/
            if (!NetX.IsListening)
            {
                if (Team == Team.Player1)
                    _radioGameplay.GameManager.P1CreditsChanged += CreditsChanged;
                else if (Team == Team.Player2)
                    _radioGameplay.GameManager.P2CreditsChanged += CreditsChanged;
            }
            /*online*/
            else if (_stateNet != null && _stateNet.IsSpawned)
            {
                _stateNet.P1Credits.OnValueChanged += OnCreditsNVChangedP1;
                _stateNet.P2Credits.OnValueChanged += OnCreditsNVChangedP2;
            }
            _radioGameplay.GameManager.VoucherChanged += OnVoucherChanged;
            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isSubscribed || _radioGameplay == null) return;

            _radioGameplay.RoundManager.OnRoundChanged -= HandleRoundChanged;
            if (!NetX.IsListening)
            {
                if (Team == Team.Player1)
                    _radioGameplay.GameManager.P1CreditsChanged -= CreditsChanged;
                else if (Team == Team.Player2)
                    _radioGameplay.GameManager.P2CreditsChanged -= CreditsChanged;
            }
            else if (_stateNet != null)
            {
                _stateNet.P1Credits.OnValueChanged -= OnCreditsNVChangedP1;
                _stateNet.P2Credits.OnValueChanged -= OnCreditsNVChangedP2;
            }
            _radioGameplay.GameManager.VoucherChanged -= OnVoucherChanged;
            _isSubscribed = false;
        }

        private IEnumerator BindNvWhenReady()
        {

            while (NetX.IsListening && (_stateNet == null || !_stateNet.IsSpawned))
            {
                _stateNet = _radioGameplay.GameStateNetwork;
                yield return null;
            }
            if (!isActiveAndEnabled) yield break;

            _stateNet.P1Credits.OnValueChanged -= OnCreditsNVChangedP1;
            _stateNet.P2Credits.OnValueChanged -= OnCreditsNVChangedP2;
            _stateNet.P1Credits.OnValueChanged += OnCreditsNVChangedP1;
            _stateNet.P2Credits.OnValueChanged += OnCreditsNVChangedP2;
            CheckIfCanAfford();
        }
        #endregion
        #region Helpers

        private void OnCreditsNVChangedP1(int _, int __) { /*Debug.Log($"P1 NV -> {__}");*/ CheckIfCanAfford(); }
        private void OnCreditsNVChangedP2(int _, int __) { /*Debug.Log($"P2 NV -> {__}");*/ CheckIfCanAfford(); }
        private void OnVoucherChanged() => CheckIfCanAfford();
        private IEnumerator ListenToChange()
        {
            yield return new WaitUntil(() =>
                isActiveAndEnabled &&
                _radioGameplay != null &&
                _radioGameplay.RoundManager != null &&
                _radioGameplay.GameManager != null);

            if (!isActiveAndEnabled) yield break;

            Subscribe();

            if (NetX.IsListening)
                StartCoroutine(BindNvWhenReady());
        }

        private void HandleRoundChanged(int newRound, RoundPhase newPhase)
        {
            CheckIfCanAfford();
        }

        private void CreditsChanged(int newCredits, int player)
        {
            CheckIfCanAfford();
        }

        private void CheckIfCanAfford()
        {
            Player player = _gameManager.GetPlayerByTeam(Team);
            bool hasVoucher = player != null && player.FreeSquadVouchers.Contains(unitPrefab);

            if (hasVoucher)
            {
                unitCost.text = "Free";
            }
            else
            {
                unitCost.text = _cost.ToString();
            }

            int cost = 0;
            Unit unit = unitPrefab.GetComponent<Unit>();
            if (unit != null)
                cost = unit.UnitCost;

            bool canAfford;
            if (!NetX.IsListening)
                canAfford = _gameManager.CanAfford(cost, Team);
            else
            {
                int credits = (Team == Team.Player1)
                            ? (_stateNet != null ? _stateNet.P1Credits.Value : 0)
                            : (_stateNet != null ? _stateNet.P2Credits.Value : 0);
                canAfford = credits >= cost;
            }
            _button.interactable = hasVoucher || canAfford;
        }

        public void OnButtonClicked()
        {
            Player player = _gameManager.GetPlayerByTeam(Team);
            if (player == null) return;
            if (player.FreeSquadVouchers.Contains(unitPrefab))
            {
                UnitPlacer.Instance.StartPlacingFreeUnit(unitPrefab, unitNum, Team);
                CheckIfCanAfford();
                return;
            }
            else if (!NetX.IsListening)
            {
                //Debug.Log($"Placing unit: {unitPrefab.name}");
                if (_gameManager.SpendCredits(unitPrefab.GetComponent<Unit>().UnitCost, Team))
                    UnitPlacer.Instance.StartPlacingUnit(unitPrefab, unitNum, Team);
                else
                    return;
            }
            else
                UnitPlacer.Instance.StartPlacingUnit(unitPrefab, unitNum, Team);
        }
        #endregion
    }
}
