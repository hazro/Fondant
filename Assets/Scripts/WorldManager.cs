using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 現在のワールドとルームイベントの状態を管理するクラス。
/// </summary>
public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; } // シングルトンのインスタンス

    //public EnemySpawnSettings enemySpawnSettings; // 敵キャラクターの出現設定をアタッチする
    public int currentWorld; // 現在のワールド番号
    public int currentWorldBackground; // 現在のワールドの背景番号
    public int currentRoomEvent; // 現在のルームイベント番号

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
        // 現在のワールドが0の場合、1に設定
        if (currentWorld == 0)
        {
            currentWorld = 1;
        }
        if(currentWorldBackground == 0)
        {
            currentWorldBackground = 1;
        }
    }

    /// <summary>
    /// 現在のワールド番号を取得するメソッド。
    /// </summary>
    /// <returns>現在のワールド番号</returns>
    public int GetCurrentWorld()
    {
        return currentWorld;
    }

    /// <summary>
    /// 現在のワールド番号に1を加算してルームイベントを0にするメソッド。
    /// </summary>
    public void IncrementWorld()
    {
        currentWorld++;
        currentWorldBackground++;
        currentRoomEvent = 0;
    }

    /// <summary>
    /// 現在のルームイベント番号を取得するメソッド。
    /// </summary>
    /// <returns>現在のルームイベント番号</returns>
    public int GetCurrentRoomEvent()
    {
        return currentRoomEvent;
    }

    /// <summary>
    /// 現在のルームイベント番号に1を加算するメソッド。
    /// </summary>
    public void IncrementRoomEvent()
    {
        currentRoomEvent++;
    }
}
