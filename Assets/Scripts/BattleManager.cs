using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using TMPro;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }
    private GameManager gameManager;
    private WorldManager worldManager;
    public Transform enemyGroup; // 敵をまとめる親オブジェクト
    public Transform enemyGridPoint; // 敵のスポーンポイント
    public Coroutine spawnCoroutine; // 敵のスポーンコルーチン
    public GameObject attackObjectGroup; // 攻撃エフェクトをまとめる親オブジェクト
    private int clearTime = 60; // クリアタイム
    [SerializeField] private GameObject countDownUI; // カウントダウンUI
    [SerializeField] private TextMeshProUGUI timerText; // タイマーテキスト
    [SerializeField] private TextMeshProUGUI countDownText; // カウントダウンテキスト

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
        if(gameManager == null){
            gameManager = GameManager.Instance;
        }
        if(worldManager == null){
            worldManager = WorldManager.Instance;
        }

        countDownText.text = ""; // カウントダウンテキストを初期化
        timerText.text = ""; // タイマーテキストを初期化
        if(gameManager.roomOptions.monsterAddTime){
            countDownUI.SetActive(true); // カウントダウンUIを表示する
            countDownText.gameObject.SetActive(true); // カウントダウンテキストを表示する
            timerText.text = " 00:00 / " + string.Format("{0:00}:{1:00}", clearTime / 60, clearTime % 60);
        }
        else{
            countDownUI.SetActive(false); // カウントダウンUIを非表示にする
            countDownText.gameObject.SetActive(false); // カウントダウンテキストを非表示にする
        }

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

    void Update()
    {
        // モンスターハウスでバトルシーンの時カウントダウンテキストを更新
        if (gameManager.roomOptions.monsterAddTime && SceneManager.GetActiveScene().name == "BattleScene")
        {
            int battleTime = (int)Time.timeSinceLevelLoad;
            // crearTime(秒)をバトル開始からの経過時間でカウントダウン
            int timeLeft = clearTime - battleTime;
            if (timeLeft <= 0)
            {
                timeLeft = 0;
                // カウントダウンが0になったらバトル終了
                countDownText.text = "0";
                countDownText.fontSize = 200;
                countDownText.color = new Color(1, 1, 1, 1);
                // enemyGroupに子オブジェクトが存在する場合のみランダムなモンスターを選択
                if (enemyGroup.childCount > 0)
                {
                    GameObject targetObject = enemyGroup.GetChild(Random.Range(0, enemyGroup.childCount)).gameObject;
                    OnBattleEnd(targetObject);
                }
                else
                {
                    Debug.LogWarning("No enemies found in enemyGroup.");
                    OnBattleEnd(null); // もしくは適切な処理を行う
                }
            }
            // バトル経過時間を00:00形式で表示
            string crearTimeStr = string.Format("{0:00}:{1:00}", clearTime / 60, clearTime % 60);
            string battleTimeStr = string.Format("{0:00}:{1:00}", battleTime / 60, battleTime % 60);
            timerText.text = battleTimeStr + " / " + crearTimeStr;

            if (timeLeft <= 10)
            {
                countDownText.text = timeLeft.ToString();
                // font size 150 -> 50 , 透明度 1 -> 0　のアニメーションを1秒かけて実行を繰り返す
                countDownText.fontSize = Mathf.Lerp(50, 200, Mathf.PingPong(Time.time, 1));
                countDownText.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, Mathf.PingPong(Time.time, 1)));
            }
            else
            {
                countDownText.text = "";
            }
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

        if (gameManager != null)
        {
            if(gameManager.roomOptions.monsterAddTime)
            {
                // 敵のスポーンコルーチンを開始
                spawnCoroutine = StartCoroutine(SpawnEnemiesOverTime());
            }
        }
    }

    /// <summary>
    /// バトル終了時の処理
    /// </summary>
    public void OnBattleEnd(GameObject targetObject = null, bool isGameOver = false)
    {
        // player全員の攻撃を停止
        foreach (GameObject player in gameManager.livingUnits)
        {
            player.GetComponent<Unit>().StopAttack();
        }
        // 敵のスポーンコルーチンを停止
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        // enemyGridPointを削除
        if(enemyGridPoint != null){
            Destroy(enemyGridPoint.gameObject);
        }

        // 攻撃エフェクトを削除
        DestroyAttackEffects();
        
        // バトル終了時にDPSコルーチンを停止
        if (dpsCoroutine != null)
        {
            StopCoroutine(dpsCoroutine);
            dpsCoroutine = null;
        }
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

        // ゲームオーバーの場合はゲームオーバーシーンに遷移
        if (isGameOver)
        {
            gameManager.LoadScene("GameOverScene"); // ゲームオーバーシーンに遷移
        }
        // gameManagerにバトル勝利を通知
        else
        {
            gameManager.victory(targetObject);
        }
    }


    //////////////////// 5秒おきにランダムに敵を出現させるコールーチンメソッド群 ///////////////////////////////
    /// <summary>
    /// 3秒ごとに敵をランダムな位置に出現させるコルーチン
    /// </summary>
    private IEnumerator SpawnEnemiesOverTime()
    {
        while (true)
        {
            Debug.Log("敵のスポーンコルーチンを開始");
            yield return new WaitForSeconds(3f);
            SpawnRandomEnemy();
        }
    }

    /// <summary>
    /// ランダムな敵を出現させるメソッド
    /// </summary>
    private void SpawnRandomEnemy()
    {
        // worldEnemySpawnリストから出現確率に基づいてランダムな敵を選択
        var spawnSetting = GetRandomSpawnSetting();
        if (spawnSetting == null) return;

        // 選択された敵のPrefabをロード
        GameObject enemyPrefab = Resources.Load<GameObject>("Prefabs/Unit/Enemy/Enemy" + spawnSetting.ID.ToString("00"));
        if (enemyPrefab == null) return;
        Debug.Log("敵をspawn: " + enemyPrefab.name);
        Debug.Log("次の敵のスポーンまで3秒待機");

        // グリッド内のランダムな位置を取得
        Vector3 spawnPosition = GetRandomSpawnPosition();

        // 敵を生成し、enemyGroupに追加
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, enemyGroup);
        enemy.transform.SetParent(enemyGroup);
        enemy.GetComponent<UnitController>().enabled = true;
        enemy.GetComponent<AttackController>().enabled = true;
        gameManager.enemyCount++;
    }

    /// <summary>
    /// 出現確率に基づいてランダムに敵のスポーン設定を取得
    /// </summary>
    private ItemData.EnemySpawnSettingsData GetRandomSpawnSetting()
    {
        ////////////////////ここはEnemySpawnのメソッドと重複///////////////////
        // gameManagerから現在のワールド番号を取得
        int currentWorld = worldManager.GetCurrentWorld();
        // gameManagerから現在のルームイベント番号を取得
        int currentRoomEvent = worldManager.GetCurrentRoomEvent();
        // キャラクター情報を格納するリストを作成
        List<ItemData.EnemySpawnSettingsData> worldEnemySpawn = new List<ItemData.EnemySpawnSettingsData>();

        // gameManager.itemData.enemySpawnSettingsから現在のワールド番号とルームイベント番号(currentRoomEvent→stage)に対応するEnemySpawnSettingsDataをすべて取得
        foreach (var enemySpawn in gameManager.itemData.enemySpawnSettings)
        {
            // まだWorld2以降が無いので、world2以降でもWorld1以降の敵キャラクター設定を取得
            if (enemySpawn.world <= currentWorld && enemySpawn.world != 0 && enemySpawn.stage == currentRoomEvent)
            {
                worldEnemySpawn.Add(enemySpawn);
            }
        }
        ////////////////////////////////////////
        ///
        // 優先度の逆数を出現重みとする（優先度が高いほど重みが大きくなる）
        var weightedSpawnSettings = worldEnemySpawn
            .Select(spawnSetting => new { Setting = spawnSetting, Weight = 1.0f / (spawnSetting.priority + 1) })
            .ToList();

        // 重みの合計を計算
        float totalWeight = weightedSpawnSettings.Sum(ws => ws.Weight);

        // ランダムな点を選択
        float randomPoint = Random.Range(0, totalWeight);
        float cumulativeWeight = 0f;

        // 累積重みに基づいてランダムに選択
        foreach (var weightedSetting in weightedSpawnSettings)
        {
            cumulativeWeight += weightedSetting.Weight;
            if (randomPoint <= cumulativeWeight)
            {
                return weightedSetting.Setting;
            }
        }

        return null; // 取得失敗の場合はnullを返す
    }

    /// <summary>
    /// ランダムな位置を取得
    /// </summary>
    private Vector3 GetRandomSpawnPosition()
    {
        List<Transform> availablePositions = new List<Transform>();
        for (int i = 0; i < enemyGridPoint.childCount; i++)
        {
            availablePositions.Add(enemyGridPoint.GetChild(i));
        }

        // ランダムな位置を選択
        int randomIndex = Random.Range(0, availablePositions.Count);
        return availablePositions[randomIndex].position + new Vector3(0, 0, -0.1f);
    }
    //////////////////////////////////////////////////////////////////////////////////////////////

}
