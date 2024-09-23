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
    public float attackRange; // 攻撃範囲
    public float attackStanceDuration; // 立ち止まって攻撃する時間
    public float attackStanceDelay; // 立ち止まって攻撃するまでの待ち時間
    public string ability; // とくしゅ能力
}
