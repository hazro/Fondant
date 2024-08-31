using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体の管理を行うクラス。
/// ゲームの開始、新規ゲームの開始、ゲームのロードなどを担当。
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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
        SceneManager.LoadScene("TownScene");
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
    /// シーンがロードされたときに呼び出されるコールバックメソッドです。
    /// TownSceneがロードされた場合、プレイヤーを生成します。
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "TownScene")
        {
            // TownSceneがロードされたら、TownManagerでプレイヤーを生成
            TownManager townManager = FindObjectOfType<TownManager>();
            if (townManager != null)
            {
                townManager.SpawnPlayer();
            }
        }
    }
}
