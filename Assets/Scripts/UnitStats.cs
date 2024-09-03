using UnityEngine;

[CreateAssetMenu(fileName = "UnitStats", menuName = "ScriptableObjects/UnitStats", order = 1)]
/// <summary>
/// Unit別の初期ステータスとレベルアップ必要経験値とレベルごとのステータス上昇値
/// </summary>
public class UnitStats : ScriptableObject
{
    public int job;
    public int totalExp;
    public int addLevel;
    //public float magic;
    //public float str;
    //public float dex;
    //public float residtCondition;
    //public float attackDelay;
    //public float attackSpeed;
    //public float attackUnitThrough;
    //public float attackObjectThrough;
    //public float attackSize;
    //public float knockBack;
    //public int targetJob;
    public int weapons;
    public int shields;
    public int armor;
    public int accessories;
    //public float teleportation;
    //public float escape;

    public int drop1;
    public int drop2;
    public int drop3;

    public float drop1Rate;
    public float drop2Rate;
    public float drop3Rate;
}
