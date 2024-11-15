using UnityEngine;

/// <summary>
/// タウンシーンを管理し、プレイヤーの生成やタウン関連のイベントを処理します。
/// </summary>
public class TownManager : MonoBehaviour
{
    GameManager gameManager;
    [SerializeField] private Canvas canvas;

    private void Start()
    {
        // Get the GameManager instance.
        gameManager = GameManager.Instance;
        canvas.worldCamera = Camera.main;
    }

    /// <summary>
    /// outボタン(外出)が押されたときに呼び出される関数です。
    /// </summary>
    public void OnOutButtonClicked()
    {
        AkSoundEngine.PostEvent("ST_Click", gameObject);
        Debug.Log("outボタンが押されました。");
        // バトルシーンへ移動し、キャラクター配置画面を表示
        gameManager.LoadScene("InToWorldEntrance");
    }

    /// <summary>
    /// Shopボタンが押されたときに呼び出される関数です。
    /// </summary>
    public void OnShopButtonClicked()
    {
        AkSoundEngine.PostEvent("ST_Click", gameObject);
        Debug.Log("Shopボタンが押されました。");
    }

    /// <summary>
    /// organizationボタン(メンバー編成)が押されたときに呼び出される関数です。
    /// </summary>
    public void OnOrganizationButtonClicked()
    {
        AkSoundEngine.PostEvent("ST_Click", gameObject);
        Debug.Log("organizationボタンが押されました。");
    }

    /// <summary>
    /// limitationボタン(制限)が押されたときに呼び出される関数です。
    /// </summary>
    public void OnLimitationButtonClicked()
    {
        AkSoundEngine.PostEvent("ST_Click", gameObject);
        Debug.Log("limitationボタンが押されました。");
    }

    /// <summary>
    /// libraryボタン(図書館)が押されたときに呼び出される関数です。
    /// </summary>
    public void OnLibraryButtonClicked()
    {
        AkSoundEngine.PostEvent("ST_Click", gameObject);
        Debug.Log("libraryボタンが押されました。");
    }
}
