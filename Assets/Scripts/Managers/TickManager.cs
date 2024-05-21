public class TickManager
{
    private static TickManager _instance;

    public static TickManager Instance => _instance ??= new TickManager();

    public TickSystem TickSystem;
}
