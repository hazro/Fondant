using UnityEngine;

[CreateAssetMenu(fileName = "JobStatus", menuName = "ScriptableObjects/JobStatus", order = 1)]

/// <summary>
/// 職業が持つステータス
/// </summary>
public class JobStatus : ScriptableObject
{
    // 初期のステータス
    public float Magic;
    public float Str;
    public float Dex;
    public float ResidtCondition;
    public float AttackUnitThrough;
    public float AttackObjectThrough;
    public float KnockBack;
    // Levelアップの上昇値
    public int levelExp;
    public float levelMagic;
    public float levelStr;
    public float levelDex;
    public float levelResidtCondition;
    public float levelAttackUnitThrough;
    public float levelAttackObjectThrough;
    public float levelKnockBack;
    public int targetJob;
    public float teleportation;
    public float escape;

    // ↓装備可能なもの
    public int EqueLargeSwordAxe;
    public int EqueSwordDagger;
    public int EqueStickStaffBook;
    public int Arrow;
    public int SPWeapon;
    public int EqueShield;
    public int EqueHeavyArmor;
    public int EqueLightArmor;
    public int EqueRobe;
    public int EqueAccessories;

}
