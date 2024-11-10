using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }
    public GameObject attackObjectGroup; // 攻撃エフェクトをまとめる親オブジェクト

    public StatusLog statusLog; // ステータスログのインスタンスを取得するためのフィールド
    public int expGained; // 獲得経験値
    public int goldGained; // 獲得ゴールド
    public int[] unitDamage; // ユニットごとのダメージ量
    public int[] unitKill; // ユニットごとのキル数
    public int[] unitMaxDPS; // ユニットごとの最大DPS
    private int[] unitCurrentDPS; // ユニットごとの現在のDPS
    private Coroutine dpsCoroutine; // DPSを計算するコルーチン

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // シーンロード時のコールバックを登録
        }
        else
        {
            Destroy(gameObject);
        }
        unitCurrentDPS = new int[unitDamage.Length];
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // コールバックの登録解除
    }

    /// <summary>
    /// シーンがロードされた時の処理
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // "BattleScene"の場合のみDPSコルーチンを開始
        if (scene.name == "BattleScene" && dpsCoroutine == null)
        {
            dpsCoroutine = StartCoroutine(CalculateDPSCoroutine());
        }
        // バトル終了時にDPSコルーチンを停止するが、念のため"BattleScene"でない場合もコルーチンを停止
        else if (scene.name != "BattleScene" && dpsCoroutine != null)
        {
            StopCoroutine(dpsCoroutine); // "BattleScene"でない場合はコルーチンを停止
            dpsCoroutine = null;
        }
    }

    /// <summary>
    /// 攻撃エフェクトをすべて削除する(gameManagerから呼び出し)
    /// </summary>
    public void DestroyAttackEffects()
    {
        foreach (Transform child in attackObjectGroup.transform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// ユニットのダメージを記録
    /// </summary>
    public void RecordUnitDamage(int unitIndex, int damage)
    {
        unitDamage[unitIndex] += damage;
        unitCurrentDPS[unitIndex] += damage;
    }

    /// <summary>
    /// ユニットのキル数を記録
    /// </summary>
    public void RecordUnitKill(int unitIndex)
    {
        unitKill[unitIndex]++;
    }

    /// <summary>
    /// DPSを計算するコルーチン
    /// </summary>
    private IEnumerator CalculateDPSCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);

            for (int i = 0; i < unitCurrentDPS.Length; i++)
            {
                if (unitCurrentDPS[i] > unitMaxDPS[i])
                {
                    unitMaxDPS[i] = unitCurrentDPS[i];
                }
                unitCurrentDPS[i] = 0;
            }
        }
    }

    /// <summary>
    /// バトル開始時の初期化処理
    /// </summary>
    public void OnBattleStart()
    {
        // バトル開始時に各ステータスを初期化
        expGained = 0;
        goldGained = 0;
        unitDamage = new int[5];
        unitKill = new int[5];
        unitMaxDPS = new int[5];
        unitCurrentDPS = new int[5];
    }

    /// <summary>
    /// バトル終了時の処理
    /// </summary>
    public void OnBattleEnd()
    {
        // バトル終了時にDPSコルーチンを停止
        StopCoroutine(dpsCoroutine);
        dpsCoroutine = null;
        // statusLogの記録を更新
        statusLog.totalDamage += unitDamage.Sum();
        statusLog.totalKill += unitKill.Sum();
        statusLog.currentExp += expGained;
        statusLog.currentGold += goldGained;
        //UnitTotalDamageにunitDamageを加算
        for (int i = 0; i < unitDamage.Length; i++)
        {
            statusLog.UnitTotalDamage[i] += unitDamage[i];
        }
        //UnitTotalKillにunitKillを加算
        for (int i = 0; i < unitKill.Length; i++)
        {
            statusLog.UnitTotalKill[i] += unitKill[i];
        }

        statusLog.expGained = expGained;
        statusLog.goldGained = goldGained;

        statusLog.unitDPS = unitMaxDPS;
        statusLog.unitDamage = unitDamage;
        statusLog.unitKill = unitKill;
    }

}
