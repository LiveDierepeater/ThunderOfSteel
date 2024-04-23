using System;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public event Action<Weaponry, UnitHealth> OnAttackStarted;
    public event Action<Weaponry, UnitHealth> OnAttackStopped;
    public event Action<UnitHealth> OnUnitDied;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartAttack(Weaponry attacker, UnitHealth target)
    {
        OnAttackStarted?.Invoke(attacker, target);
    }

    public void StopAttack(Weaponry attacker, UnitHealth target)
    {
        OnAttackStopped?.Invoke(attacker, target);
    }

    public void NotifyDeath(UnitHealth target)
    {
        OnUnitDied?.Invoke(target);
    }
}
