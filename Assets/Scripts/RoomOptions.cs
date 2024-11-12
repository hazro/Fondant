using UnityEngine;

[CreateAssetMenu(fileName = "RoomOptions", menuName = "ScriptableObjects/RoomOptions", order = 1)]
public class RoomOptions : ScriptableObject
{
    [Header("Room Sale Info")]
    public int salePercentage; // セール情報（例：0 または 25）

    [Header("Room Effects")]
    public bool monsterDoubleCount; // モンスターの出現数が2倍になる
    public bool doubleGold;         // ゴールドが2倍になる
    public bool doubleExp;          // 経験値が2倍になる
    public bool monsterLevelUp;     // モンスターのレベルが1上がる
    public bool monsterHpUp;        // モンスターのHPが1.5倍になる
    public bool monsterAtkUp;       // モンスターの攻撃力が1.5倍になる
    public bool monsterDefUp;       // モンスターの防御力が1.5倍になる
    public bool monsterAddTime;     // モンスターが制限時間内につぎつぎと出現する
    public bool itemDropUp;         // アイテムドロップ率が1.5倍になる
    public bool runeDropUp;         // ルーンドロップ率が1.5倍になる

    /// <summary>
    /// すべてのオプションをリセットするメソッド
    /// </summary>
    public void ResetOptions()
    {
        salePercentage = 0;
        monsterDoubleCount = false;
        doubleGold = false;
        doubleExp = false;
        monsterLevelUp = false;
        monsterHpUp = false;
        monsterAtkUp = false;
        monsterDefUp = false;
        monsterAddTime = false;
        itemDropUp = false;
        runeDropUp = false;
    }
}
