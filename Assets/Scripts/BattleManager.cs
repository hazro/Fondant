using UnityEngine;

/// <summary>
/// 自動戦闘を管理するクラス
/// </summary>
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }
    //攻撃エフェクトを格納するオブジェクト
    public GameObject attackObjectGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 自動戦闘を開始するメソッド
    /// </summary>
    public void StartAutoBattle()
    {
        // 自動戦闘ロジックの実装
        // 敵とプレイヤーの戦闘を開始
    }

    /// <summary>
    /// バトルが終了した後、次のルーム選択を行う
    /// </summary>
    private void OnBattleEnd()
    {
        RoomSelectionManager.Instance.DisplayRoomOptions();
    }
}
