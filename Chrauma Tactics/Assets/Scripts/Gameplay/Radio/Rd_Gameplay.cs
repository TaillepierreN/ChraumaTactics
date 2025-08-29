using UnityEngine;
using CT.Gameplay;

[CreateAssetMenu(fileName = "Gameplay", menuName = "Radio/Gameplay")]
public class Rd_Gameplay : ScriptableObject
{
    private GameManager _gameManager;
    private RoundManager _roundManager;
    private RoundUIManager _roundUIManager;
    private BoostManager _boostManager;
    private GameStateNetwork _gameStateNetwork;
    private Transform _poolContainer;
    private CommanderSelectionMenu _commanderSelectionMenu;


    public GameManager GameManager => _gameManager;
    public RoundManager RoundManager => _roundManager;
    public RoundUIManager RoundUIManager => _roundUIManager;
    public BoostManager BoostManager => _boostManager;
    public GameStateNetwork GameStateNetwork => _gameStateNetwork;
    public Transform Pool => _poolContainer;
    public CommanderSelectionMenu CommanderSelectionmenu => _commanderSelectionMenu;


    public void SetGameManager(GameManager gm) => _gameManager = gm;

    public void SetRoundManager(RoundManager rm) => _roundManager = rm;

    public void SetRoundUIManager(RoundUIManager ruim) => _roundUIManager = ruim;

    public void SetBoostManager(BoostManager bm) => _boostManager = bm;

    public void SetGameStateNetwork(GameStateNetwork gsn) => _gameStateNetwork = gsn;

    public void SetPoolContainer(Transform p) => _poolContainer = p;
    public void SetCommanderSelectionMenu(CommanderSelectionMenu csm) => _commanderSelectionMenu = csm;
}
