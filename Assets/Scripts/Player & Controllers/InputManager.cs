using System;

public class InputManager : InputReader
{
    private static InputManager _instance;
    private static GameManager _gameManager;

    private InGamePlayerController _inGamePlayerController;

    private GameManager.GameState _gameState;

    public static InputManager Instance => _instance;

    public Player Player;

    #region Initializing

    protected override void Awake()
    {
        base.Awake();
        if (_instance is not null)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject.transform.root);
        
        Player = Instantiate(Player);
        InitializeAllPlayerControllers();
    }

    private void Start()
    {
        _gameManager.OnChangedGameState += ChangePlayerController;
        ActivatePlayerController(_inGamePlayerController);
    }

    #endregion

    #region External Called Logic

    private void ChangePlayerController(GameManager.GameState newGameState)
    {
        _gameState = newGameState;

        switch (_gameState)
        {
            case GameManager.GameState.MainMenu:
                break;
            
            case GameManager.GameState.PauseMenu:
                break;
            
            case GameManager.GameState.InGame:
                ActivatePlayerController(_inGamePlayerController);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void SetGameManager(GameManager newGameManager)
    {
        _gameManager = newGameManager;
        _gameState = _gameManager.State;
    }

    #endregion

    #region Extracted Logic Methods

    private void InitializeAllPlayerControllers()
    {
        _inGamePlayerController = Player.gameObject.AddComponent<InGamePlayerController>();
    }

    private void ActivatePlayerController<T>(T playerController)
    {
        if (playerController is InGamePlayerController)
        {
            _inGamePlayerController.SetLayerMaskInfo(Player);
            _inGamePlayerController.enabled = true;

            // TODO: Deactivate all other PlayerController
        }
        else
            _inGamePlayerController.enabled = false;
    }

    #endregion

    private void OnDestroy()
    {
        if(_instance == this) _instance = null;
    }
}
