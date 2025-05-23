using UnityEngine;

public class TickSystem : MonoBehaviour
{
    public delegate void TickBeginDelegate();
    public TickBeginDelegate OnTickBegin;
    
    public delegate void TickDelegate();
    public TickDelegate OnTick;
    
    public delegate void TickEndDelegate();
    public TickEndDelegate OnTickEnd;

    public const float TickRate = 0.2f;
    
    // ReSharper disable once MemberCanBePrivate.Global
    public int ElapsedTicks { get; private set; }
    private float nextTick;

    private void Awake() => TickManager.Instance.TickSystem = this;

    private void Update() => TickTimer();

    private void TickTimer()
    {
        if (Time.time >= nextTick)
        {
            OnTickEnd?.Invoke();
            OnTickBegin?.Invoke();
            OnTick?.Invoke();
            
            nextTick = Time.time + TickRate;
            ElapsedTicks++;
        }
    }
}
