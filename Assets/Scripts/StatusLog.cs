using UnityEngine;

[CreateAssetMenu(fileName = "StatusLog", menuName = "ScriptableObjects/StatusLog", order = 1)]
/// <summary>
/// Unit別の初期ステータスとレベルアップ必要経験値とレベルごとのステータス上昇値
/// </summary>
public class StatusLog : ScriptableObject
{
    public int currentGold;
    public int currentExp;
}
