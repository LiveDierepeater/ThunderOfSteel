public class TickManager
{
    private static TickManager _instance;

    public static TickManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new TickManager();
            }

            return _instance;
        }
    }

    public TickSystem TickSystem;
}
