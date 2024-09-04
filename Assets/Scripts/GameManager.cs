using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// ゲーム全体の管理を行うクラス。
/// ゲームの開始、新規ゲームの開始、ゲームのロードなどを担当。
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private IventryUI iventryUI; // IventryUIのインスタンスを取得するためのフィールド

    [SerializeField] private StatusLog statusLog; // ステータスログのインスタンスを取得するためのフィールド

    [SerializeField] private RectTransform statusLogPanel; // ステータスログパネルのRectTransformを取得するためのフィールド
    [SerializeField] private TextMeshProUGUI currentGold; // 現在のゴールドを表示するためのフィールド
    [SerializeField] private TextMeshProUGUI stockExp; // ストック経験値を表示するためのフィールド

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
            statusLogPanel.gameObject.SetActive(true);
            iventryUI.SetButtonEnabled(false);
            SceneManager.LoadScene("TownScene");
        }
        if(sceneName == "InToWorldEntrance")
        {
            statusLogPanel.gameObject.SetActive(false);
            iventryUI.SetButtonEnabled(false);
            SceneManager.LoadScene("BattleSetupScene");
        }
        if(sceneName == "InToBattleScene")
        {
            statusLogPanel.gameObject.SetActive(true);
            SceneManager.LoadScene("BattleScene");
            iventryUI.SetButtonEnabled(true);
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
}
