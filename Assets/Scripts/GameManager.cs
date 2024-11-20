using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// ゲーム全体の管理を行うクラス。
/// ゲームの開始、新規ゲームの開始、ゲームのロードなどを担当。
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private Button settingOpenButton; // 設定画面を開くボタン
    public SettingManager settingManager; // SettingManagerのインスタンスを取得するためのフィールド

    [Header("------------------------ ]")]
    private JsonDecryptor jsonDecryptor; // JsonDecryptorクラスのインスタンスを取得するためのフィールド
    public ItemData itemData; // デシリアライズしたデータを格納するクラス
    public List<GameObject> playerUnits = new List<GameObject>(); // プレイヤーユニットのリスト
    public List<GameObject> livingUnits = new List<GameObject>(); // 生存しているユニットのリスト
    public List<GameObject> deadPanelList = new List<GameObject>(); // 死亡したユニットのスキルパネルを塞ぐオブジェクトのリスト
    public GameObject enemyGroup; // 敵グループのGameObject
    public int enemyCount = 0; // 敵の数
    private Transform playerGroup; // プレイヤーグループのTransform
    private WorldManager worldManager; // WorldManagerの参照
    [SerializeField] private float zoomSpeed = 15.0f; // ズームスピード
    [SerializeField] private float slowMotionDuration = 2.0f; // スローモーションの時間
    [SerializeField] private IventryUI iventryUI; // IventryUIのインスタンスを取得するためのフィールド
    [SerializeField] private BattleManager battleManager; // BattleManagerのインスタンスを取得するためのフィールド

    [Header("[Setting Log ------------------------ ]")]
    public StatusLog statusLog; // ステータスログのインスタンスを取得するためのフィールド
    public RoomOptions roomOptions; // Room用のオプション設定

    [SerializeField] private RectTransform statusLogPanel; // ステータスログパネルのRectTransformを取得するためのフィールド
    [SerializeField] private RectTransform[] UnitSkillPanels; // ユニットスキルパネルのRectTransformを取得するためのフィールド
    [SerializeField] private TextMeshProUGUI worldUItxt; // ワールド番号UIのテキストを取得するためのフィールド
    [SerializeField] private TextMeshProUGUI stageUItxt; // ステージ番号UIのテキストを取得するためのフィールド
    [SerializeField] private TextMeshProUGUI currentGold; // 現在のゴールドを表示するためのフィールド
    [SerializeField] private TextMeshProUGUI stockExp; // ストック経験値を表示するためのフィールド
    [SerializeField] private Canvas uiCanvas; // キャンバスを取得するためのフィールド

    [Header("[victoryUI ------------------------ ]")]
    [SerializeField] private Sprite[] jobFace; // ジョブの顔画像を表示するImage
    [SerializeField] private Image[] unitFaceImage; // ユニットの顔画像を表示するImage
    [SerializeField] private GameObject victoryUI; // 勝利UI
    [SerializeField] private TextMeshProUGUI[] victoryTexts; // 勝利テキスト
    [SerializeField] private Button nextButton; // 次へボタン

    void Awake()
    {
        //world番号とstage番号の初期値を設定
        worldUItxt.text = "01";
        stageUItxt.text = "00/12";

        // シングルトンパターンの設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        // カメラにPhysics2DRaycasterがアタッチされていない場合、アタッチする
        Camera camera = Camera.main;
        if(camera != null){
            if(camera.GetComponent<Physics2DRaycaster>() == null){
                camera.gameObject.AddComponent<Physics2DRaycaster>();
            }
        }
        /*
        // キャンバスのカメラを設定
        if(uiCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            uiCanvas.worldCamera = camera;
        }
        else
        {
            uiCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            uiCanvas.worldCamera = camera;
        }
        */
    }

    private void Start()
    {
        // settingManagerのインスタンスを取得
        settingManager = SettingManager.Instance;
        // settingOpenButtonのOnClickイベントにOpenSettingメソッドを追加
        settingOpenButton.onClick.AddListener(OpenSetting);

        // JsonDecryptorクラスを使用する
        jsonDecryptor = new JsonDecryptor();
        string filename = "items"; // ファイル名
        // itemDataにMasterデータを取得
        itemData = GetItemData(filename);

        //worldManagerのインスタンスを取得
        worldManager = WorldManager.Instance;
        //battleManagerのインスタンスを取得
        battleManager = BattleManager.Instance;

        // プレイヤーユニットがいない場合、キャラクターを生成
        if (livingUnits.Count == 0)
        {
            createCharacter();
        }
        // 他のスクリプトのStartメソッドが完了するまで待機
        StartCoroutine(WaitForStartMethods());

        // RoomOptionsのオプションをリセット
        roomOptions.ResetOptions();
    }

    /// <summary>
    /// 他のスクリプトのStartメソッドが完了するまで待機するコルーチン。
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForStartMethods()
    {
        // フレームの終わりまで待機
        yield return new WaitForEndOfFrame();

        // すべてのプレイヤーユニットのスキルUIを更新
        foreach (GameObject livingUnits in livingUnits)
        {
            // updateStatusを実行
            iventryUI.UpdateUnitSkillUI(livingUnits);
        }
        // iventryPanelの更新
        iventryUI.UpdateIventryPanel();

        // もしGameStartSceneだったら
        if(SceneManager.GetActiveScene().name == "GameStartScene")
        {
            // ゲームスタートシーンの処理を行う
            LoadScene("GameStartScene");
        }
    }

    // itemDataを取得するメソッド
    public ItemData GetItemData(string filename)
    {
        // 暗号化されたJSONファイルを復号化
        string decryptedJson = jsonDecryptor.ReadAndDecryptJson(filename);

        if (!string.IsNullOrEmpty(decryptedJson))
        {
            // 復号化されたJSONをクラスにデシリアライズ
            ItemData data = JsonUtility.FromJson<ItemData>(decryptedJson);
            return data;
        }
        else
        {
            Debug.LogError("復号化に失敗しました");
        }
        return null;
    }

    /// <summary>
    /// 新規ゲームを開始するメソッド。
    /// ゲームの初期設定やデータのリセットを行う。
    /// </summary>
    public void StartNewGame()
    {
        Debug.Log("Starting New Game...");

        // ゲームの初期化処理
        InitializeGame();

        // タウンシーンをロード
        LoadScene("InToTownScene");
    }

    /// <summary>
    /// セーブデータをロードしてゲームを再開するメソッド。
    /// 以前のゲーム状態を復元する。
    /// </summary>
    public void LoadGame()
    {
        Debug.Log("Loading Game...");

        // セーブデータをロード
        LoadGameData();

        // ゲームを復元し、前回の状態に戻す
        ResumeGame();
    }

    /// <summary>
    /// ゲームの初期化処理を行う（新規ゲーム開始時）。
    /// </summary>
    private void InitializeGame()
    {
        //StatusLogの初期化
        statusLog.currentGold = 0;
        statusLog.currentExp = 0;
        
        // ゲーム開始時に必要な初期化処理をここに記述
        // 例: プレイヤーデータのリセット、初期アイテムの設定など
    }

    /// <summary>
    /// セーブデータをロードする。
    /// </summary>
    private void LoadGameData()
    {
        // セーブデータの読み込み処理をここに記述
        // 例: JSONファイルからプレイヤーデータや進行状況を読み込む
    }

    /// <summary>
    /// ゲームを再開し、前回の状態に復元する。
    /// </summary>
    private void ResumeGame()
    {
        // ゲームの状態を復元する処理をここに記述
        // 例: プレイヤーの位置、進行状況、所持アイテムの復元など
    }

    /// <summary>
    /// 指定したシーンをロードする。
    /// </summary>
    /// <param name="sceneName"></param>
    public void LoadScene(string sceneName)
    {
        if(sceneName == "GameStartScene")
        {
            SceneManager.LoadScene("Title");
        }

        if(sceneName == "InToTownScene")
        {
            // ワールド番号とステージ番号を初期化する
            worldManager.currentWorld = 1;
            worldManager.currentRoomEvent = 0;
            // UIのワールド番号とステージ番号を更新
            UpdateWorldStageUI();

            // RoomOptionsのオプションをリセット
            roomOptions.ResetOptions();

            //// チームステータス関連の初期化
            statusLog.currentGold /= 3; // 街には1/3のゴールドしか持ち帰れない
            statusLog.currentExp = 0; // ストック経験値をリセット
            statusLog.totalDamage = 0; // 累計ダメージをリセット
            statusLog.totalKill = 0; // 累計キル数をリセット
            for (int i = 0; i < statusLog.UnitTotalDamage.Length; i++)
            {
                statusLog.UnitTotalDamage[i] = 0; // ユニットごとの累計ダメージをリセット
                statusLog.UnitTotalKill[i] = 0; // ユニットごとの累計キル数をリセット
            }
            statusLog.expGained = 0; // 獲得経験値をリセット
            //statusLog.goldGained = 0; // 獲得ゴールドをリセット
            // ステータスをUIに反映
            UpdateGoldAndExpUI();

            statusLogPanel.gameObject.SetActive(true);
            iventryUI.SetButtonEnabled(true);
            // UnitSkillPanelsの要素をすべてアクティブにする
            foreach (RectTransform panel in UnitSkillPanels)
            {
                panel.gameObject.SetActive(true);
            }
            SceneManager.LoadScene("TownScene");
            // 全てのユニットをアクティブにし、HPを回復する
            foreach (GameObject unit in livingUnits)
            {
                unit.SetActive(true);
                unit.GetComponent<UnitController>().enabled = false;
                unit.GetComponent<PlayerDraggable>().enabled = true;
                unit.GetComponent<AttackController>().enabled = false;
                unit.GetComponent<Unit>().ChangeEqpByJob(); // 初期装備に変更
                unit.GetComponent<Unit>().condition = new bool[7]; // 状態異常を解除
                unit.GetComponent<Unit>().live = true; // 生存状態に変更
                // subSocketをすべて0にする
                for (int i = 0; i < unit.GetComponent<Unit>().subSocket.Length; i++)
                {
                    unit.GetComponent<Unit>().subSocket[i] = 0;
                }
                IventryUI.UpdateUnitSkillUI(unit); // ユニットのスキルUIを更新
                unit.GetComponent<Unit>().updateStatus(); // ユニットのステータスを更新
                unit.GetComponent<Unit>().InitHp(); // ユニットのHPを初期化
                // ユニットを画面外に移動
                unit.transform.position = new Vector3(100.0f,100.0f,0.0f);
            }
            // deadPanelをすべて非アクティブにする
            foreach (GameObject panel in deadPanelList)
            {
                panel.SetActive(false);
            }
            // iventryのアイテムを空にする
            iventryUI.ClearItem();
        }
        if(sceneName == "InToWorldEntrance")
        {
            statusLogPanel.gameObject.SetActive(true);
            iventryUI.SetButtonEnabled(true);
            // UnitSkillPanelsの要素をすべてアクティブにする
            foreach (RectTransform panel in UnitSkillPanels)
            {
                panel.gameObject.SetActive(true);
            }
            // 全てのアクティブユニットの設定をする
            foreach (GameObject unit in livingUnits)
            {
                // アクティブであったらHPを回復
                if(unit.activeSelf){
                    unit.GetComponent<Unit>().InitHp();
                }
                
                unit.GetComponent<PlayerDraggable>().enabled = true;
                unit.GetComponent<UnitController>().enabled = false;
                unit.GetComponent<AttackController>().enabled = false;
            }
            // 33%の確率でBattleSetupBSceneに遷移、それ以外はBattleSetupASceneに遷移
            int random = Random.Range(0, 100);
            if(random < 33){
                SceneManager.LoadScene("BattleSetupBScene");
            }
            else{
                SceneManager.LoadScene("BattleSetupAScene");
            }

            // ステージ番号を加算する
            if(worldManager != null){
                worldManager.IncrementRoomEvent();
            }
            // ワールド番号とステージ番号を更新
            UpdateWorldStageUI();
            // ステータスをUIに反映
            UpdateGoldAndExpUI();
        }
        if(sceneName == "InToBattleScene")
        {
            statusLogPanel.gameObject.SetActive(true);
            SceneManager.LoadScene("BattleScene");
            iventryUI.SetButtonEnabled(true);
            // UnitSkillPanelsの要素をすべてアクティブにする
            foreach (RectTransform panel in UnitSkillPanels)
            {
                panel.gameObject.SetActive(true);
            }

            // 子オブジェクトの指定のコンポーネントを有効化
            foreach (GameObject unit in livingUnits)
            {
                unit.GetComponent<UnitController>().enabled = true;
                unit.GetComponent<PlayerDraggable>().enabled = false;
                unit.GetComponent<AttackController>().enabled = true;
                unit.GetComponent<AttackController>().wasShootingEnabled = false; //前回の攻撃状態をリセットすることで再び攻撃ができるようにする
            }
        }
        if(sceneName == "VictoryScene")
        {
            statusLogPanel.gameObject.SetActive(false);
            SceneManager.LoadScene("VictoryScene");

            // 勝利テキストを表示
            victoryUI.SetActive(true);
            nextButton.gameObject.SetActive(true);

            iventryUI.SetButtonEnabled(false);
            // UnitSkillPanelsの要素をすべて非アクティブにする
            foreach (RectTransform panel in UnitSkillPanels)
            {
                panel.gameObject.SetActive(false);
            }

            // infomationPanelを非アクティブにする
            GetComponent<InfomationPanelDisplay>().infomationPanel.SetActive(false);

            // 子オブジェクトの指定のコンポーネントを有効化
            foreach (GameObject unit in livingUnits)
            {
                unit.GetComponent<UnitController>().enabled = false;
                unit.GetComponent<PlayerDraggable>().enabled = false;
                unit.GetComponent<AttackController>().enabled = false;
                // ユニットを画面外に移動
                unit.transform.position = new Vector3(100.0f,100.0f,0.0f);
            }

            // 一通り処理終わってからCameraControllerのwhiteoutMaskを非アクティブにする
            GetComponent<CameraController>().whiteoutMask.SetActive(false);
        }
        if(sceneName == "StatusAdjustmentScene")
        {
            // 勝利テキストを非表示
            victoryUI.SetActive(false);
            nextButton.gameObject.SetActive(false);

            statusLogPanel.gameObject.SetActive(true);
            SceneManager.LoadScene("StatusAdjustmentScene");

            // 一旦roomEvent12まで行ったらworldを進めてroomEventを0にする
            if(worldManager.GetCurrentRoomEvent() == 12){
                worldManager.IncrementWorld();
                // まだボスとワールド2以降が実装されていないのでワールド番号を1にする
                //worldManager.currentWorld = 1;
            }
            // ワールド番号とステージ番号を更新
            UpdateWorldStageUI();

            iventryUI.SetButtonEnabled(true);
            // UnitSkillPanelsの要素をすべて非アクティブにする
            foreach (RectTransform panel in UnitSkillPanels)
            {
                panel.gameObject.SetActive(true);
            }

            // infomationPanelを非アクティブにする
            //GetComponent<InfomationPanelDisplay>().infomationPanel.SetActive(false);

            // 子オブジェクトの指定のコンポーネントを有効化
            foreach (GameObject unit in livingUnits)
            {
                unit.GetComponent<UnitController>().enabled = false;
                unit.GetComponent<PlayerDraggable>().enabled = false;
                unit.GetComponent<AttackController>().enabled = false;
                // ユニットを画面外に移動
                unit.transform.position = new Vector3(100.0f,100.0f,0.0f);
            }

            // 一通り処理終わってからCameraControllerのwhiteoutMaskを非アクティブにする
            //GetComponent<CameraController>().whiteoutMask.SetActive(false);
        }
        if(sceneName == "RoomSelectScene")
        {

            statusLogPanel.gameObject.SetActive(true);
            SceneManager.LoadScene("RoomSelectScene");
            // ワールド番号とステージ番号を更新
            UpdateWorldStageUI();
            iventryUI.SetButtonEnabled(true);
            // UnitSkillPanelsの要素をすべて非アクティブにする
            foreach (RectTransform panel in UnitSkillPanels)
            {
                panel.gameObject.SetActive(true);
            }

            // 子オブジェクトの指定のコンポーネントを無効化
            foreach (GameObject unit in livingUnits)
            {
                unit.GetComponent<UnitController>().enabled = false;
                unit.GetComponent<PlayerDraggable>().enabled = false;
                unit.GetComponent<AttackController>().enabled = false;
                // ユニットを画面外に移動
                unit.transform.position = new Vector3(100.0f,100.0f,0.0f);
            }

        }
        if(sceneName == "ShopScene")
        {
            // 勝利テキストを非表示
            victoryUI.SetActive(false);
            nextButton.gameObject.SetActive(false);

            statusLogPanel.gameObject.SetActive(true);
            SceneManager.LoadScene("ShopScene");
            // ステージ番号を加算する
            if(worldManager != null){
                worldManager.IncrementRoomEvent();
            }
            // ワールド番号とステージ番号を更新
            UpdateWorldStageUI();
            iventryUI.SetButtonEnabled(true);
            // UnitSkillPanelsの要素をすべて非アクティブにする
            foreach (RectTransform panel in UnitSkillPanels)
            {
                panel.gameObject.SetActive(true);
            }

            // infomationPanelを非アクティブにする
            //GetComponent<InfomationPanelDisplay>().infomationPanel.SetActive(false);

            // 子オブジェクトの指定のコンポーネントを有効化
            foreach (GameObject unit in livingUnits)
            {
                unit.GetComponent<UnitController>().enabled = false;
                unit.GetComponent<PlayerDraggable>().enabled = false;
                unit.GetComponent<AttackController>().enabled = false;
                // ユニットを画面外に移動
                unit.transform.position = new Vector3(100.0f,100.0f,0.0f);
            }

            // 一通り処理終わってからCameraControllerのwhiteoutMaskを非アクティブにする
            //GetComponent<CameraController>().whiteoutMask.SetActive(false);
        }
        if(sceneName == "BlackSmithScene")
        {
            // 勝利テキストを非表示
            victoryUI.SetActive(false);
            nextButton.gameObject.SetActive(false);

            statusLogPanel.gameObject.SetActive(true);
            SceneManager.LoadScene("BlacksmithScene");
            // ステージ番号を加算する
            if(worldManager != null){
                worldManager.IncrementRoomEvent();
            }
            // ワールド番号とステージ番号を更新
            UpdateWorldStageUI();
            iventryUI.SetButtonEnabled(true);
            // UnitSkillPanelsの要素をすべて非アクティブにする
            foreach (RectTransform panel in UnitSkillPanels)
            {
                panel.gameObject.SetActive(true);
            }

            // infomationPanelを非アクティブにする
            //GetComponent<InfomationPanelDisplay>().infomationPanel.SetActive(false);

            // 子オブジェクトの指定のコンポーネントを無効化
            foreach (GameObject unit in livingUnits)
            {
                unit.GetComponent<UnitController>().enabled = false;
                unit.GetComponent<PlayerDraggable>().enabled = false;
                unit.GetComponent<AttackController>().enabled = false;
                // ユニットを画面外に移動
                unit.transform.position = new Vector3(100.0f,100.0f,0.0f);
            }

            // 一通り処理終わってからCameraControllerのwhiteoutMaskを非アクティブにする
            //GetComponent<CameraController>().whiteoutMask.SetActive(false);
        }
        if(sceneName == "RecoverScene")
        {
            // 勝利テキストを非表示
            victoryUI.SetActive(false);
            nextButton.gameObject.SetActive(false);

            statusLogPanel.gameObject.SetActive(true);
            SceneManager.LoadScene("RecoverScene");
            // ステージ番号を加算する
            if(worldManager != null){
                worldManager.IncrementRoomEvent();
            }
            // ワールド番号とステージ番号を更新
            UpdateWorldStageUI();
            iventryUI.SetButtonEnabled(true);
            // UnitSkillPanelsの要素をすべて非アクティブにする
            foreach (RectTransform panel in UnitSkillPanels)
            {
                panel.gameObject.SetActive(true);
            }

            // infomationPanelを非アクティブにする
            //GetComponent<InfomationPanelDisplay>().infomationPanel.SetActive(false);

            // 子オブジェクトの指定のコンポーネントを無効化
            foreach (GameObject unit in livingUnits)
            {
                unit.GetComponent<UnitController>().enabled = false;
                unit.GetComponent<PlayerDraggable>().enabled = false;
                unit.GetComponent<AttackController>().enabled = false;
                // ユニットを画面外に移動
                unit.transform.position = new Vector3(100.0f,100.0f,0.0f);
            }

            // 一通り処理終わってからCameraControllerのwhiteoutMaskを非アクティブにする
            //GetComponent<CameraController>().whiteoutMask.SetActive(false);
        }
        if(sceneName == "GameOverScene")
        {
            statusLogPanel.gameObject.SetActive(false);
            SceneManager.LoadScene("GameOverScene");
            iventryUI.SetButtonEnabled(false);
            // UnitSkillPanelsの要素をすべて非アクティブにする
            foreach (RectTransform panel in UnitSkillPanels)
            {
                panel.gameObject.SetActive(false);
            }

            // infomationPanelを非アクティブにする
            GetComponent<InfomationPanelDisplay>().infomationPanel.SetActive(false);

            // 子オブジェクトの指定のコンポーネントを有効化
            foreach (GameObject unit in livingUnits)
            {
                unit.GetComponent<UnitController>().enabled = false;
                unit.GetComponent<PlayerDraggable>().enabled = false;
                unit.GetComponent<AttackController>().enabled = false;
                // ユニットを画面外に移動
                unit.transform.position = new Vector3(100.0f,100.0f,0.0f);
            }

            // 一通り処理終わってからCameraControllerのwhiteoutMaskを非アクティブにする
            GetComponent<CameraController>().whiteoutMask.SetActive(false);
        }
    }

    /// <summary>
    /// ストック経験値を追加する。
    /// </summary>
    /// <param name="exp"></param>
    public void AddExperience(int exp)
    {
        statusLog.currentExp += exp;
        UpdateGoldAndExpUI();
    }

    /// <summary>
    /// ゴールドを追加する。
    /// </summary>
    /// <param name="gold"></param>
    public void AddGold(int gold)
    {
        statusLog.currentGold += gold;
        UpdateGoldAndExpUI();
    }

    /// <summary>
    /// ゴールドとストック経験値のUIを更新する。
    /// </summary>
    public void UpdateGoldAndExpUI()
    {
        currentGold.text = statusLog.currentGold.ToString("D6");
        stockExp.text = statusLog.currentExp.ToString("D6");
    }

    /// <summary>
    /// イベントリーUIを渡すプロパティ。
    /// </summary>
    public IventryUI IventryUI
    {
        get { return iventryUI; }
    }

    /// <summary>
    /// キャラクターを生成するメソッド。
    /// </summary>
    public void createCharacter()
    {
        // 生存ユニットをクリア
        livingUnits.Clear();

        // PlayerGroupオブジェクトのチェックと作成
        if (playerGroup == null)
        {
            GameObject playerGroupObj = new GameObject("PlayerGroup");
            playerGroup = playerGroupObj.transform;

            // 次のシーンに持っていくために、DontDestroyOnLoadを設定
            DontDestroyOnLoad(playerGroupObj);
        }

        // プレイヤーの数が1体以上、5体以下であることを確認
        if (playerUnits.Count < 1 || playerUnits.Count > 5)
        {
            Debug.LogError("プレイヤーの数は1体以上、5体以下でなければなりません。");
            return;
        }

        // プレイヤーユニットの数だけ繰り返す
        for (int i = 0; i < playerUnits.Count; i++)
        {
            // プレイヤーのPrefabを画面外の位置にインスタンス化
            GameObject player = Instantiate(playerUnits[i], new Vector3(100.0f,100.0f,0.0f), Quaternion.identity);

            // プレイヤーをPlayerGroupの子として設定
            player.transform.SetParent(playerGroup);

            // プレイヤーをlivingUnitsリストに追加
            livingUnits.Add(player);

            // プレイヤーを非アクティブに設定
            //player.SetActive(false);
        }
    }

    /// <summary>
    /// 勝利時の処理を行うメソッド。
    /// </summary>
    public void victory(GameObject lastEnemy)
    {
        // 勝利時の処理を行うコルーチンを開始
        StartCoroutine(VictoryCoroutine(lastEnemy));
    }

    /// <summary>
    /// 勝利時の処理を行うコルーチン。
    /// </summary>
    /// <param name="lastEnemy">最後の敵オブジェクト</param>
    /// <returns>コルーチン</returns>
    private IEnumerator VictoryCoroutine(GameObject lastEnemy)
    {
        if (lastEnemy != null)
        {
            // 勝利カメラ演出
            yield return StartCoroutine(GetComponent<CameraController>().StartZoomAndSlowMotion(zoomSpeed, slowMotionDuration, lastEnemy));
        }

        // ↑の処理が終わったら、次のシーンに遷移
        // enemyGroupを削除
        Destroy(enemyGroup);
        enemyCount = 0; // 敵の数をリセット

        // 勝利テキストを変更
        ChangeVictoryText();
        // 勝利画面の顔画像を変更
        for (int i = 0; i < unitFaceImage.Length; i++)
        {
            int jobID = itemData.jobList.Find(job => job.ID == livingUnits[i].GetComponent<Unit>().job).ID;
            unitFaceImage[i].sprite = jobFace[jobID];
        }
        LoadScene("VictoryScene");
    }

    //現在のワールド番号とステージ番号を取得してUIを更新するメソッド
    public void UpdateWorldStageUI()
    {
        if(worldManager == null){
            return;
        }
        worldUItxt.text = worldManager.GetCurrentWorld().ToString("D2");
        stageUItxt.text = worldManager.GetCurrentRoomEvent().ToString("D2") + "/12";
    }

    /// <summary>
    /// 勝利テキストを変更するメソッド。
    /// </summary>
    public void ChangeVictoryText()
    {
        for (int i = 0; i < victoryTexts.Length; i++)
        {
            if(i == 0 )
            {
                // battleEXPを入力
                victoryTexts[i].text = statusLog.expGained.ToString();
            }
            else if(i == 1)
            {
                // BattleGoldを入力
                victoryTexts[i].text = statusLog.goldGained.ToString();
            }
            else if(i == 2)
            {
                // P1 の　unitDamageを入力
                victoryTexts[i].text = statusLog.unitDamage[0].ToString();
            }
            else if(i == 3)
            {
                // P1 の unitDPSを入力
                victoryTexts[i].text = statusLog.unitDPS[0].ToString();
            }
            else if(i == 4)
            {
                // P2 の　unitDamageを入力
                victoryTexts[i].text = statusLog.unitDamage[1].ToString();
            }
            else if(i == 5)
            {
                // P2 の unitDPSを入力
                victoryTexts[i].text = statusLog.unitDPS[1].ToString();
            }
            else if(i == 6)
            {
                // P3 の　unitDamageを入力
                victoryTexts[i].text = statusLog.unitDamage[2].ToString();
            }
            else if(i == 7)
            {
                // P3 の unitDPSを入力
                victoryTexts[i].text = statusLog.unitDPS[2].ToString();
            }
            else if(i == 8)
            {
                // P4 の　unitDamageを入力
                victoryTexts[i].text = statusLog.unitDamage[3].ToString();
            }
            else if(i == 9)
            {
                // P4 の unitDPSを入力
                victoryTexts[i].text = statusLog.unitDPS[3].ToString();
            }
            else if(i == 10)
            {
                // P5 の　unitDamageを入力
                victoryTexts[i].text = statusLog.unitDamage[4].ToString();
            }
            else if(i == 11)
            {
                // P5 の unitDPSを入力
                victoryTexts[i].text = statusLog.unitDPS[4].ToString();
            }
        }
    }

    /// <summary>
    /// 指定のユニットを治療するメソッド。
    /// </summary>
    public void RecoverUnit(GameObject unit)
    {
        // ユニットのIDを取得
        int unitIndex = unit.GetComponent<Unit>().ID - 1;
        // 死亡パネルを非アクティブにする
        deadPanelList[unitIndex].SetActive(false);
        // ユニットのGameObjectをアクティブにする
        unit.SetActive(true);
        // ユニットのHPを回復
        unit.GetComponent<Unit>().InitHp();
        // ユニットの状態異常を解除
        unit.GetComponent<Unit>().condition = new bool[7];
        unit.GetComponent<Unit>().live = true;
        // ユニットのスキルUIを更新
        iventryUI.UpdateUnitSkillUI(unit);
        // ユニットのステータスを更新
        unit.GetComponent<Unit>().updateStatus();
    }

    /// <summary>
    /// 設定画面を開くボタンを押した時の処理
    /// </summary>
    public void OpenSetting()
    {
        if(settingManager == null){
            settingManager = SettingManager.Instance;
        }
        if(settingManager != null)
        {
            settingManager.OpenSettingPanel();
        }
    }
    
}
