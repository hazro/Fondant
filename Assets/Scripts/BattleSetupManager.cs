using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Animations;

/// <summary>
/// 自動戦闘の準備をするクラス
/// </summary>
public class BattleSetupManager : MonoBehaviour
{

    public Button startBattleButton; // 戦闘開始ボタン
    public Transform enemyGridPoint; // 敵キャラクター配置用のグリッドポイント
    public Transform PlayerGridPoint; // プレイヤーキャラクター配置用のグリッドポイント
    private WorldManager worldManager; // WorldManagerの参照
    private GameManager gameManager; // GameManagerの参照
    private Transform enemyGroup; // エネミーグループのTransform
    private IventryUI iventryUI; // IventryUIの参照

    private void Awake()
    {

    }

    private void Start()
    {
        // 必要な初期化
        gameManager = GameManager.Instance;
        worldManager = WorldManager.Instance;
        BattleManager battleManager = BattleManager.Instance;

        if (worldManager.currentRoomEvent == 0)
            worldManager.currentRoomEvent = 1;

        iventryUI = FindObjectOfType<IventryUI>();

        if (startBattleButton == null || worldManager == null || enemyGridPoint == null || PlayerGridPoint == null)
        {
            Debug.LogError("必要なフィールドが設定されていません。");
            return;
        }

        // プレイヤーキャラクターの生成と配置を先に完了させる
        if (gameManager.livingUnits.Count == 0)
            gameManager.createCharacter();

        SetupCharacterPlacement(); // プレイヤーキャラクターを一気に配置

        // エネミー生成を遅らせる（コルーチンで非同期処理）
        StartCoroutine(SetupEnemiesWithDelay());

        // プレイヤーの方向設定
        foreach (var unit in gameManager.livingUnits)
            unit.GetComponent<PlayerDraggable>().LookAtNearestTarget();

        // その他の初期設定
        enemyGridPoint.SetParent(null);
        DontDestroyOnLoad(enemyGridPoint.gameObject);
        battleManager.enemyGridPoint = enemyGridPoint;

        foreach (Transform child in enemyGridPoint)
            child.GetComponent<PositionConstraint>().enabled = false;

        startBattleButton.onClick.AddListener(OnStartBattleButtonClicked);
    }

    /// <summary>
    /// 敵キャラクターの配置を遅らせるコルーチン
    /// </summary>
    private IEnumerator SetupEnemiesWithDelay()
    {
        yield return null; // 一度フレームを待つことでプレイヤー配置を完全に終了させる

        // エネミー生成処理
        SetupEnemies();
    }


    /// <summary>
    /// 敵キャラクターをグリッド上に配置するメソッド。
    /// </summary>
    public void SetupEnemies()
    {
        // 敵の数を初期化
        gameManager.enemyCount = 0;

        if (enemyGroup == null)
        {
            GameObject enemyGroupObj = new GameObject("EnemyGroup");
            enemyGroup = enemyGroupObj.transform;
            gameManager.enemyGroup = enemyGroupObj;
            BattleManager.Instance.enemyGroup = enemyGroupObj.transform;
            DontDestroyOnLoad(enemyGroupObj);
        }

        // 必要なデータを取得してソート
        int currentWorld = worldManager.GetCurrentWorld();
        int currentRoomEvent = worldManager.GetCurrentRoomEvent();
        List<ItemData.EnemySpawnSettingsData> worldEnemySpawn = gameManager.itemData.enemySpawnSettings
            .Where(es => es.world <= currentWorld && es.stage == currentRoomEvent && es.world != 0)
            .OrderByDescending(es => es.priority)
            .ToList();

        if (worldEnemySpawn.Count == 0)
        {
            Debug.LogWarning("現在のルームイベントに対する敵キャラクター設定がありません。");
            return;
        }

        // 各敵の数を決定
        Dictionary<ItemData.EnemySpawnSettingsData, int> enemyCounts = new Dictionary<ItemData.EnemySpawnSettingsData, int>();
        foreach (var spawnSetting in worldEnemySpawn)
        {
            int enemyCount = Random.Range(spawnSetting.minCount, spawnSetting.maxCount + 1);
            if (gameManager.roomOptions.monsterDoubleCount)
                enemyCount *= 2;
            enemyCounts[spawnSetting] = enemyCount;
        }

        // 敵が多すぎる場合は調整
        int totalEnemies = enemyCounts.Values.Sum();
        while (totalEnemies > 20)
        {
            var maxEnemy = enemyCounts.OrderByDescending(ec => ec.Value).First();
            enemyCounts[maxEnemy.Key]--;
            totalEnemies--;
            if (enemyCounts[maxEnemy.Key] <= 0)
                enemyCounts.Remove(maxEnemy.Key);
        }

        // 配置処理を開始
        StartCoroutine(SpawnEnemiesWithDelay(worldEnemySpawn, enemyCounts));
    }

    /// <summary>
    /// 敵キャラクターを間隔を空けてスポーンするコルーチン。
    /// </summary>
    private IEnumerator SpawnEnemiesWithDelay(List<ItemData.EnemySpawnSettingsData> spawnSettings, Dictionary<ItemData.EnemySpawnSettingsData, int> enemyCounts)
    {
        List<Transform> availablePositions = enemyGridPoint.Cast<Transform>().ToList();

        foreach (var spawnSetting in spawnSettings)
        {
            GameObject enemyPrefab = Resources.Load<GameObject>("Prefabs/Unit/Enemy/Enemy" + spawnSetting.ID.ToString("00"));
            int numEnemies = enemyCounts.ContainsKey(spawnSetting) ? enemyCounts[spawnSetting] : 0;

            for (int i = 0; i < numEnemies; i++)
            {
                if (availablePositions.Count == 0) break;

                Transform spawnPosition = availablePositions[0];
                availablePositions.RemoveAt(0);

                // エネミーをスポーン
                GameObject enemy = Instantiate(enemyPrefab, spawnPosition.position, Quaternion.identity, enemyGroup);
                enemy.transform.position += new Vector3(0, 0, -0.1f);

                // 出現エフェクト処理
                StartCoroutine(FadeInEnemy(enemy));

                gameManager.enemyCount++;
                yield return new WaitForSeconds(0.5f); // 次のスポーンまで待機
            }
        }
    }

    /// <summary>
    /// 敵キャラクターのフェードインを処理するコルーチン。
    /// </summary>
    private IEnumerator FadeInEnemy(GameObject enemy)
    {
        // 条件に合う子オブジェクトを取得
        Transform offsetTransform = GetOffsetTransform(enemy);

        // SpriteRendererとMaterialを取得
        SpriteRenderer spriteRenderer = offsetTransform?.GetComponent<SpriteRenderer>();
        Material material = spriteRenderer?.sharedMaterial;

        // RectTransformを持つ子オブジェクトを探して非表示にする
        List<GameObject> rectTransformChildren = new List<GameObject>();
        foreach (Transform child in enemy.transform)
        {
            if (child.GetComponent<RectTransform>() != null)
            {
                rectTransformChildren.Add(child.gameObject);
                child.gameObject.SetActive(false);
            }
        }

        if (spriteRenderer != null && material != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0;
            spriteRenderer.color = color;

            for (float t = 0; t < 1f; t += Time.deltaTime)
            {
                color.a = t;
                spriteRenderer.color = color;

                yield return null;
            }

            // 最終的にアルファを1に設定
            color.a = 1;
            spriteRenderer.color = color;

        }

        // RectTransformを持つ子オブジェクトを再表示
        foreach (GameObject child in rectTransformChildren)
        {
            child.SetActive(true);
        }
        
        // エネミー出現SEを再生
        AkSoundEngine.PostEvent("SE_Appearance", gameObject);
    }

    /// <summary>
    /// エネミーの子オブジェクトの中で条件に合うものを取得するメソッド
    /// </summary>
    /// <param name="enemy"></param>
    /// <returns></returns>
    private Transform GetOffsetTransform(GameObject enemy)
    {
        foreach (Transform child in enemy.transform)
        {
            SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sharedMaterial != null)
            {
                if (spriteRenderer.sharedMaterial.name == "UnitStanderd")
                {
                    return child; // 条件に一致したオブジェクトを返す
                }
            }
        }
        return null; // 条件に一致するオブジェクトがなければnullを返す
    }

    /// <summary>
    /// 生存キャラクターをGridPanel上に自動配置するメソッド
    /// </summary>
    public void SetupCharacterPlacement()
    {
        // GameManagerからプレイヤーキャラクターのリストを取得　リストから配列に変換
        GameObject[] livingUnits = gameManager.livingUnits.ToArray();

        // 配列内のPrefab数がGridPanelの子オブジェクトの数と一致するか確認
        int gridCells = PlayerGridPoint.childCount;
        if (livingUnits.Length > gridCells)
        {
            Debug.LogError("プレイヤーの数がGridセルの数を超えています。");
            return;
        }

        // GridPanelの子オブジェクト（セル）に対して順番にPlayerPrefabを配置する
        for(int i = 0; i < livingUnits.Length; i++)
        {
            // SetActive(false)で、Unit.ConditionがDeadの場合は処理をスキップ
            if (!livingUnits[i].activeSelf && livingUnits[i].GetComponent<Unit>().condition == 1)
            {
                continue;
            }
            int positionID = livingUnits[i].GetComponent<Unit>().positionID;
            // IDに該当するPlayerGridPointの子オブジェクトを取得
            Transform cellTransform = PlayerGridPoint.GetChild(positionID-1);
            // セルの位置にプレイヤーを配置
            Vector3 playerPosition = cellTransform.position;

            // プレイヤーの位置をセルの位置に設定
            livingUnits[i].transform.position = playerPosition + new Vector3(0, 0, -0.1f); // Z軸を少し下げる事でキャラのコリジョンを優勢にする
            livingUnits[i].SetActive(true);
        }
    }

    private void OnStartBattleButtonClicked()
    {
        // クリックSEを再生
        AkSoundEngine.PostEvent("ST_Click", gameObject);

        // 新しいバトルシーンをロード
        GameManager.Instance.LoadScene("InToBattleScene");

        // 子オブジェクトをすべて取得
        Transform[] children = enemyGroup.GetComponentsInChildren<Transform>();

        // 子オブジェクトの指定のコンポーネントを有効化
        foreach (Transform child in children)
        {
            UnitController unitController = child.GetComponent<UnitController>();
            if (unitController != null)
            {
                unitController.enabled = true;
            }
        }

        // バトル開始時の初期化処理を実行
        BattleManager.Instance.OnBattleStart();

        // バトルシーンに遷移するSEを再生
        AkSoundEngine.PostEvent("ST_BGChange", gameObject);
    }


}
