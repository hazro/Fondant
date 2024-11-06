using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
    }

    /// <summary>
    /// NewGameボタンがクリックされた時の処理
    /// </summary>
    public void OnNewGameButtonClicked()
    {
        if(gameManager != null)
        {
            gameManager.StartNewGame();
        }
    }
    /// <summary>
    /// Continueボタンがクリックされた時の処理
    /// </summary>
    public void OnContiueButtonClicked()
    {
        if (gameManager != null)
        {
            gameManager.LoadGame();
        }
    }
    /// <summary>
    /// Rankingボタンがクリックされた時の処理
    /// </summary>
    public void OnRankingButtonClicked()
    {
        Debug.Log("ランキング未実装");
    }
    /// <summary>
    /// Quitボタンがクリックされた時の処理
    /// </summary>
    public void OnQuitButtonClicked()
    {
        // Quit the game
        Application.Quit();
    }

}
