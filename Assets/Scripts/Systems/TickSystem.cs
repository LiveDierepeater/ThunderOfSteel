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
    
    public int elapsedTicks { get; private set; }
    private float nextTick;

    private void Awake() => TickManager.Instance.TickSystem = this;

    void Update() => TickTimer();

    private void TickTimer()
    {
        if (Time.time >= nextTick)
        {
            OnTickBegin?.Invoke();
            OnTick?.Invoke();
            OnTickEnd?.Invoke();
            
            nextTick = Time.time + TickRate;
            elapsedTicks++;
        }
    }
}
