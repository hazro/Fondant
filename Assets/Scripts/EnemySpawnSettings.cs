using UnityEngine;

/// <summary>
/// 敵キャラクターの出現設定を管理するスクリプタブルオブジェクトクラス。
/// </summary>
[CreateAssetMenu(fileName = "EnemySpawnSettings", menuName = "Game/Enemy Spawn Settings")]
public class EnemySpawnSettings : ScriptableObject
{
    [System.Serializable]
    public struct EnemyTypeSpawnChance
    {
        public GameObject enemyPrefab; // 敵キャラクターのプレハブ
        public int minCount; // 最小出現数
        public int maxCount; // 最大出現数
        public int preferredColumn; // 優先列 (0から始まるインデックス、0が一番左)
        public int priority; // 優先度 (数字が小さいほど優先度が高い)
    }

    [System.Serializable]
    public struct RoomEventSettings
    {
        /// <summary>
        /// ルームイベントの番号。
        /// </summary>
        public int roomEventNumber;

        /// <summary>
        /// このルームイベントで出現する敵キャラクターの設定の配列。
        /// </summary>
        public EnemyTypeSpawnChance[] enemyTypeSpawnChances;
    }

    /// <summary>
    /// ルームイベント設定の配列。
    /// </summary>
    public RoomEventSettings[] roomEventSettings;
}
