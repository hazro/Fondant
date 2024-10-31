using UnityEngine;

/// <summary>
/// タウンシーンを管理し、プレイヤーの生成やタウン関連のイベントを処理します。
/// </summary>
public class TownManager : MonoBehaviour
{
    GameManager gameManager;

    private void Start()
    {
        // Get the GameManager instance.
        gameManager = GameManager.Instance;
    }

    /// <summary>
    /// outボタン(外出)が押されたときに呼び出される関数です。
    /// </summary>
    public void OnOutButtonClicked()
    {
        Debug.Log("outボタンが押されました。");
        // バトルシーンへ移動し、キャラクター配置画面を表示
        gameManager.LoadScene("InToWorldEntrance");
    }

    /// <summary>
    /// Shopボタンが押されたときに呼び出される関数です。
    /// </summary>
    public void OnShopButtonClicked()
    {
        Debug.Log("Shopボタンが押されました。");
    }

    /// <summary>
    /// organizationボタン(メンバー編成)が押されたときに呼び出される関数です。
    /// </summary>
    public void OnOrganizationButtonClicked()
    {
        Debug.Log("organizationボタンが押されました。");
    }

    /// <summary>
    /// limitationボタン(制限)が押されたときに呼び出される関数です。
    /// </summary>
    public void OnLimitationButtonClicked()
    {
        Debug.Log("limitationボタンが押されました。");
    }

    /// <summary>
    /// libraryボタン(図書館)が押されたときに呼び出される関数です。
    /// </summary>
    public void OnLibraryButtonClicked()
    {
        Debug.Log("libraryボタンが押されました。");
    }
}
