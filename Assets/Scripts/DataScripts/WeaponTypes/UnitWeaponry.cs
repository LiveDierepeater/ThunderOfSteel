using UnityEngine;

[CreateAssetMenu(fileName = "New Weaponry", menuName = "Data/Weaponry")]
public class UnitWeaponry : ScriptableObject
{
    public string WeaponName;

    [Header("Armor Damage")]
    
    public int Damage_Infantry;
    public int Damage_Truck;
    public int Damage_Building;
    public int Damage_Armor_Level_01;
    public int Damage_Armor_Level_02;
    public int Damage_Armor_Level_03;
    public int Damage_Armor_Level_04;
    public int Damage_Armor_Level_05;
    public int Damage_Air;
    
    [Space(10)]
    
    public float AttackRange;
    public float CoolDown;
}
