using UnityEngine;

/// <summary>
/// ルームイベントが終わり次のルームを選択するクラス
/// </summary>
public class RoomSelectionManager : MonoBehaviour
{
    public static RoomSelectionManager Instance { get; private set; }

    public GameObject[] roomOptions;

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
    /// 次に進むルームを選択するUIを表示
    /// </summary>
    public void DisplayRoomOptions()
    {
        // ルーム選択UIの表示
        // ランダムに2つのルームを表示する
        int option1 = Random.Range(0, roomOptions.Length);
        int option2 = Random.Range(0, roomOptions.Length);
        
        while (option2 == option1)
        {
            option2 = Random.Range(0, roomOptions.Length);
        }

        roomOptions[option1].SetActive(true);
        roomOptions[option2].SetActive(true);
    }
}
