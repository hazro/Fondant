using UnityEngine;

/// <summary>
/// タウンシーンを管理し、プレイヤーの生成やタウン関連のイベントを処理します。
/// </summary>
public class TownManager : MonoBehaviour
{
    public GameObject playerPrefab; // プレイヤーのプレハブへの参照
    private GameObject playerInstance; // 生成されたプレイヤーへの参照

    private void Start()
    {
        // タウンシーンが開始されたときにプレイヤーの生成を行います
        SpawnPlayer();
    }

    /// <summary>
    /// プレイヤーを生成するメソッドです。
    /// </summary>
    public void SpawnPlayer()
    {
        if (playerPrefab != null && playerInstance == null)
        {
            playerInstance = Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        }
        else if (playerPrefab == null)
        {
            Debug.LogError("TownManagerにプレイヤーのプレハブが割り当てられていません。");
        }
    }
}
