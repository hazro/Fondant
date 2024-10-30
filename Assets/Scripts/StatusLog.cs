using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "StatusLog", menuName = "ScriptableObjects/StatusLog", order = 1)]
/// <summary>
/// Unit別の初期ステータスとレベルアップ必要経験値とレベルごとのステータス上昇値
/// </summary>
public class StatusLog : ScriptableObject
{
    [Header("team Status")]
    public int currentGold;
    public int currentExp;
    public int shopResetCount = 3;

    [Header("battle record")]
    public int totalDamage;
    public int totalKill;
    public int expGained;
    public int goldGained;
    
    public int[] UnitTotalDamage = new int[5];
    public int[] UnitTotalKill = new int[5];
    public int[] unitDPS = new int[5];
    public int[] unitDamage = new int[5];
}
