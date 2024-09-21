using UnityEngine;

[CreateAssetMenu(fileName = "UnitStatus", menuName = "ScriptableObjects/UnitStatus", order = 1)]
/// <summary>
/// Unit別の初期ステータスとドロップアイテムを管理するScriptableObjectクラス。
/// </summary>
public class UnitStatus : ScriptableObject
{
    public int job;
    public int addLevel;
    public int weapons;
    public int shields;
    public int armor;
    public int accessories;

    public int drop1;
    public int drop2;
    public int drop3;

    public float drop1Rate;
    public float drop2Rate;
    public float drop3Rate;

    public int dropExp;
    public int dropGold;
}
