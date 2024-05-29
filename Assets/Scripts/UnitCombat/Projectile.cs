using UnityEngine;

public abstract class Projectile: MonoBehaviour
{
    public float InitialSpeed;
    public Unit Target;
    public Vector3 TargetPosition;
    
    private int[] _armorDamage = new int[9];
    private Vector3 originPosition;
    
    protected virtual void Update()
    {
        UpdateTargetPosition();
        UpdateProjectilePosition();    

        if (DoesProjectileReachTargetPosition())
            HitTarget();
    }

    protected void Start() => originPosition = transform.position;

    protected virtual void HitTarget()
    {
        if (Target is not null)
            ApplyDamageToTarget();
        
        Destroy(gameObject);
    }

    private void UpdateTargetPosition()
    {
        if (Target is not null)
            TargetPosition = Target.transform.position;
        // try
        // {
        //     targetPosition = target.transform.position;
        // }
        // catch (MissingReferenceException _)
        // {
        //     
        // }
    }

    private void ApplyDamageToTarget() => Target.Events.OnAttack?.Invoke(originPosition, _armorDamage[(int)Target.UnitData.Armor]);

    private void HandleTargetDeath() => Target = null;

    public void InitializeWeaponryEvents(Weaponry ownerWeaponry) => ownerWeaponry.OnLoosingTarget += HandleTargetDeath;

    public void InitializeArmorDamage(int[] armorDamage) => _armorDamage = armorDamage;

    protected virtual void UpdateProjectilePosition() => transform.position = Vector3.MoveTowards(transform.position, TargetPosition, InitialSpeed * Time.deltaTime);

    private bool DoesProjectileReachTargetPosition() => Vector3.Distance(transform.position, TargetPosition) < 0.1f;
}
