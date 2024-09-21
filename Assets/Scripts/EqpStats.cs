using UnityEngine;

[CreateAssetMenu(fileName = "EqpStats", menuName = "ScriptableObjects/EqpStats", order = 1)]
/// <summary>
/// 装備によるステータス上昇値（単純な加算計算）
/// </summary>
public class EqpStats : ScriptableObject
{
    public string eqpName;
    public int HP;
    public float physicalAttackPower;
    public float magicalAttackPower;
    public float physicalDefensePower;
    public float magicalDefensePower;
    public float resistCondition;
    public float attackDelay;
    public float Speed;
    public int attackUnitThrough;
    public int attackObjectThrough;
    public float attackSize;
    public float knockBack;
    public int targetJob;
    public float teleportation;
    public float escape;
    public string ability; // とくしゅ能力
}
