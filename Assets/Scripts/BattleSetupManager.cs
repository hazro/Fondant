using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

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
        // WorldManagerのインスタンスを動的に取得
        worldManager = WorldManager.Instance;
        // GameManagerのインスタンスを取得
        gameManager = GameManager.Instance;
        iventryUI = FindObjectOfType<IventryUI>(); // IventryUIのインスタンスを取得

        if (startBattleButton == null || worldManager == null || enemyGridPoint == null || PlayerGridPoint == null)
        {
            Debug.LogError("必要なフィールドが設定されていません。");
            return;
        }
        SetupCharacterPlacement(); // プレイヤーキャラクターの配置
        SetupEnemies(); // 敵キャラクターの配置

        // 戦闘開始ボタンがクリックされたときの処理を設定
        startBattleButton.onClick.AddListener(OnStartBattleButtonClicked);
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

            // 次のシーンに持っていくために、DontDestroyOnLoadを設定
            DontDestroyOnLoad(enemyGroupObj);
        }

        var roomEventSettings = worldManager.GetCurrentRoomEventSettings();

        if (roomEventSettings.enemyTypeSpawnChances.Length == 0)
        {
            Debug.LogWarning("現在のルームイベントに対する敵キャラクター設定がありません。");
            return;
        }

        // グリッドセルのリストを作成
        List<Transform> gridPositions = new List<Transform>();
        for (int i = 0; i < enemyGridPoint.childCount; i++)
        {
            gridPositions.Add(enemyGridPoint.GetChild(i));
        }

        // 敵キャラクターのスポーン設定を集める
        Dictionary<EnemySpawnSettings.EnemyTypeSpawnChance, int> enemyCounts = new Dictionary<EnemySpawnSettings.EnemyTypeSpawnChance, int>();
        foreach (var spawnSetting in roomEventSettings.enemyTypeSpawnChances)
        {
            int enemyCount = Random.Range(spawnSetting.minCount, spawnSetting.maxCount + 1);
            enemyCounts[spawnSetting] = enemyCount;
        }

        int totalEnemies = enemyCounts.Sum(x => x.Value);

        // 20体を超える場合に削減する処理
        while (totalEnemies > 20)
        {
            var maxEnemy = enemyCounts.OrderByDescending(x => x.Value).FirstOrDefault();
            enemyCounts[maxEnemy.Key]--;
            totalEnemies--;
            if (enemyCounts[maxEnemy.Key] <= 0)
            {
                enemyCounts.Remove(maxEnemy.Key);
            }
        }

        // 敵の配置
        List<Transform> availablePositions = gridPositions.ToList();

        // 敵キャラクターを優先順位でソート
        var sortedSpawnSettings = roomEventSettings.enemyTypeSpawnChances
            .OrderByDescending(s => s.priority)
            .ToList();

        foreach (var spawnSetting in sortedSpawnSettings)
        {
            int numEnemies = enemyCounts.ContainsKey(spawnSetting) ? enemyCounts[spawnSetting] : 0;
            for (int i = 0; i < numEnemies; i++)
            {
                bool placed = false;

                // PreferredColumnに基づいた空き位置の検索
                int startIndex = spawnSetting.preferredColumn * 5;
                int endIndex = startIndex + 5;

                // 中間の番号を優先するように位置を選択
                List<int> preferredOrder = new List<int> { 2, 1, 3, 0, 4 };

                foreach (int offset in preferredOrder)
                {
                    int index = startIndex + offset;

                    // 配置可能な位置を確認
                    if (index < availablePositions.Count && availablePositions[index] != null)
                    {
                        Transform availablePosition = availablePositions[index];
                        GameObject enemy = Instantiate(spawnSetting.enemyPrefab, availablePosition.position, Quaternion.identity, enemyGroup);
                        enemy.transform.SetParent(enemyGroup);
                        enemy.transform.position = availablePosition.position + new Vector3(0, 0, -0.1f);
                        availablePositions[index] = null; // この位置を利用済みとしてマーク
                        placed = true;
                        break;
                    }
                }

                // PreferredColumnが埋まっている場合、近いColumnに配置
                if (!placed)
                {
                    List<int> adjacentColumns = new List<int>
                    {
                        (spawnSetting.preferredColumn - 1 + 4) % 4, // 左隣のColumn
                        (spawnSetting.preferredColumn + 1) % 4  // 右隣のColumn
                    };

                    foreach (int column in adjacentColumns)
                    {
                        startIndex = column * 5;
                        endIndex = startIndex + 5;

                        foreach (int offset in preferredOrder)
                        {
                            int index = startIndex + offset;

                            if (index < availablePositions.Count && availablePositions[index] != null)
                            {
                                Transform availablePosition = availablePositions[index];
                                GameObject enemy = Instantiate(spawnSetting.enemyPrefab, availablePosition.position, Quaternion.identity, enemyGroup);
                                enemy.transform.SetParent(enemyGroup);
                                enemy.transform.position = availablePosition.position + new Vector3(0, 0, -0.1f);
                                availablePositions[index] = null; // この位置を利用済みとしてマーク
                                placed = true;
                                break;
                            }
                        }

                        if (placed)
                        {
                            break;
                        }
                    }
                }

                // 近いColumnでも配置できない場合、空いている位置に配置
                if (!placed && availablePositions.Count > 0)
                {
                    Transform fallbackPosition = availablePositions[0];
                    GameObject enemy = Instantiate(spawnSetting.enemyPrefab, fallbackPosition.position, Quaternion.identity, enemyGroup);
                    enemy.transform.SetParent(enemyGroup);
                    enemy.transform.position = fallbackPosition.position + new Vector3(0, 0, -0.1f);
                    availablePositions.RemoveAt(0);
                }
            }
        }
        // 配置した敵の数を取得
        gameManager.enemyCount = enemyGroup.childCount;
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
        for (int i = 0; i < livingUnits.Length; i++)
        {
            // 各セルのTransformを取得
            Transform cellTransform = PlayerGridPoint.GetChild(i);

            if (cellTransform == null)
            {
                Debug.LogError("GridPanelの子オブジェクトがTransformではありません。");
                continue;
            }

            Vector3 playerPosition = cellTransform.position;

            // プレイヤーの位置をセルの位置に設定
            livingUnits[i].transform.position = playerPosition + new Vector3(0, 0, -0.1f); // Z軸を少し下げる事でキャラのコリジョンを優勢にする
            livingUnits[i].SetActive(true);
        }
    }

    private void OnStartBattleButtonClicked()
    {
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


        // 自動戦闘を開始する
        BattleManager.Instance.StartAutoBattle();
    }
}
