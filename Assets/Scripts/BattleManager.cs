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
    /// 攻撃エフェクトをすべて削除するメソッド
    /// </summary>
    public void DestroyAttackEffects()
    {
        //攻撃エフェクトを削除
        foreach (Transform child in attackObjectGroup.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
