using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// ゲーム全体の管理を行うクラス。
/// ゲームの開始、新規ゲームの開始、ゲームのロードなどを担当。
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public List<GameObject> playerUnits = new List<GameObject>(); // プレイヤーユニットのリスト
    public List<GameObject> livingUnits = new List<GameObject>(); // 生存しているユニットのリスト
    public List<GameObject> deadPanelList = new List<GameObject>(); // 死亡したユニットのスキルパネルを塞ぐオブジェクトのリスト
    public int enemyCount = 0; // 敵の数
    private Transform playerGroup; // プレイヤーグループのTransform
    [SerializeField] private float zoomSpeed = 15.0f; // ズームスピード
    [SerializeField] private float slowMotionDuration = 2.0f; // スローモーションの時間
    [SerializeField] private IventryUI iventryUI; // IventryUIのインスタンスを取得するためのフィールド

    [SerializeField] private StatusLog statusLog; // ステータスログのインスタンスを取得するためのフィールド

    [SerializeField] private RectTransform statusLogPanel; // ステータスログパネルのRectTransformを取得するためのフィールド
    [SerializeField] private RectTransform[] UnitSkillPanels; // ユニットスキルパネルのRectTransformを取得するためのフィールド
    [SerializeField] private TextMeshProUGUI currentGold; // 現在のゴールドを表示するためのフィールド
    [SerializeField] private TextMeshProUGUI stockExp; // ストック経験値を表示するためのフィールド
    [SerializeField] private Canvas uiCanvas; // キャンバスを取得するためのフィールド

    void Awake()
    {
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
        // プレイヤーユニットがいない場合、キャラクターを生成
        if (livingUnits.Count == 0)
        {
            createCharacter();
        }
        // 他のスクリプトのStartメソッドが完了するまで待機
        StartCoroutine(WaitForStartMethods());
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
        if(sceneName == "InToTownScene")
        {
            statusLog.currentGold /= 3; // 街には1/3のゴールドしか持ち帰れない
            statusLogPanel.gameObject.SetActive(true);
            iventryUI.SetButtonEnabled(true);
            // UnitSkillPanelsの要素をすべてアクティブにする
            foreach (RectTransform panel in UnitSkillPanels)
            {
                panel.gameObject.SetActive(true);
            }
            SceneManager.LoadScene("TownScene");
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

            SceneManager.LoadScene("BattleSetupScene");

            // 子オブジェクトの指定のコンポーネントを有効化
            foreach (GameObject unit in livingUnits)
            {
                unit.GetComponent<PlayerDraggable>().enabled = true;
            }
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
            }
        }
        if(sceneName == "VictoryScene")
        {
            statusLogPanel.gameObject.SetActive(false);
            SceneManager.LoadScene("VictoryScene");
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
        stockExp.text = statusLog.currentExp.ToString("D6");
    }

    /// <summary>
    /// ゴールドを追加する。
    /// </summary>
    /// <param name="gold"></param>
    public void AddGold(int gold)
    {
        statusLog.currentGold += gold;
        currentGold.text = statusLog.currentGold.ToString("D6");
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
        // 勝利演出
        yield return StartCoroutine(GetComponent<CameraController>().StartZoomAndSlowMotion(zoomSpeed, slowMotionDuration, lastEnemy));
        
        // ↑の処理が終わったら、次のシーンに遷移
        LoadScene("VictoryScene");
    }
    
}
