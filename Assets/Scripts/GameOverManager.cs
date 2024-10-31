using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        // Get the GameManager instance.
        gameManager = GameManager.Instance;
    }

    /// <summary>
    /// 街に戻るボタンが押されたときに呼び出される関数です。
    /// </summary>
    public void OnBackToTownButtonClicked()
    {
        Debug.Log("街に戻るボタンが押されました。");
        // 街シーンへ移動
        gameManager.LoadScene("InToTownScene");
    }
}
