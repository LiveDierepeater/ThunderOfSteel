public class InputManager : InputReader
{
    private static InputManager _instance;

    public static InputManager Instance => _instance;

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
    }

    private void OnEnable()
    {
        PlayerEvents.OnLeftMouseButtonPressed += Test;
    }

    private void Test(bool _)
    {
        if (_)
        {
            print("LMB + Shift");
        }
        else
        {
            print("LMB");
        }
    }

    private void OnDestroy()
    {
        if(_instance == this) _instance = null;
    }
}
