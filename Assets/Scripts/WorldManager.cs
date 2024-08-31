using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 現在のワールドとルームイベントの状態を管理するクラス。
/// </summary>
public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; } // シングルトンのインスタンス

    public EnemySpawnSettings enemySpawnSettings; // 敵キャラクターの出現設定をアタッチする
    private int currentWorld; // 現在のワールド番号
    private int currentRoomEvent; // 現在のルームイベント番号

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
    /// 現在のワールド番号を取得するメソッド。
    /// </summary>
    /// <returns>現在のワールド番号</returns>
    public int GetCurrentWorld()
    {
        return currentWorld;
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
    /// 現在のルームイベント番号に基づいて敵キャラクターの出現設定を取得するメソッド。
    /// </summary>
    /// <returns>敵キャラクターの出現設定の配列</returns>
    public EnemySpawnSettings.RoomEventSettings GetCurrentRoomEventSettings()
    {
        foreach (var eventSetting in enemySpawnSettings.roomEventSettings)
        {
            if (eventSetting.roomEventNumber == currentRoomEvent)
            {
                return eventSetting;
            }
        }

        return default;
    }

    /// <summary>
    /// ワールド1のバトルを開始するメソッド。
    /// </summary>
    public void StartWorld1Battle()
    {
        // バトルシーンへ移動し、キャラクター配置画面を表示
        SceneManager.LoadScene("BattleSetupScene");
    }
}
