using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ルームIDの更新とシーン遷移を管理するクラス
/// </summary>
public class NextRoomManager : MonoBehaviour
{
    private GameManager gameManager;

    [SerializeField] private Sprite[] roomImages;
    [SerializeField] private TextMeshProUGUI room1Text;
    [SerializeField] private TextMeshProUGUI room1InfoText;
    [SerializeField] private TextMeshProUGUI room2Text;
    [SerializeField] private TextMeshProUGUI room2InfoText;

    public RoomOptions roomOptions; // 選択されたルームのオプション設定
    public RoomOptions room1Options; // room1のオプション設定（記憶用）
    public RoomOptions room2Options; // room2のオプション設定（記憶用）

    private int room1Id = 1;
    private int room2Id;

    private Dictionary<int, string> roomInfoOptions;

    /// <summary>
    /// 初期化処理
    /// </summary>
    void Start()
    {
        gameManager = GameManager.Instance;

        // room1Optionsとroom2Optionsをインスタンス化して初期化
        room1Options = ScriptableObject.CreateInstance<RoomOptions>();
        room2Options = ScriptableObject.CreateInstance<RoomOptions>();

        // オプションの表示テキスト
        roomInfoOptions = new Dictionary<int, string>()
        {
            { 1, "<color=yellow> Monster Appearance Chance x2 (max. 20) </color>\n<color=green> Item drop rate x1.5 </color>" },
            { 2, "<color=yellow> Monster Appearance Chance x2 (max. 20) </color>\n<color=green> Rune drop rate x1.5 </color>" },
            { 3, "<color=yellow> Monster Level +1 </color>\n<color=green> EXP x2 </color>" },
            { 4, "<color=yellow> Monster HP x1.5 </color>\n<color=green> Gold x2 </color>" },
            { 5, "<color=yellow> Monster Attack Power x1.5 </color>\n<color=green> EXP x2 </color>" },
            { 6, "<color=yellow> Monster Defense x1.5 </color>\n<color=green> Gold x2 </color>" },
            { 7, "<color=red> Monster Appears one after another Within time limit </color>\n<color=green> Gold x2 </color>\n<color=green> EXP x2 </color>" },
            { 8, "<color=red> Monster Appears one after another Within time limit </color>\n<color=green> Item drop rate x1.5 </color>\n<color=green> Rune drop rate x1.5 </color>" }
        };

        // 初期設定として、ルームIDの更新
        UpdateRoomId();
    }

    /// <summary>
    /// ルームIDを更新し、各ルームの情報を設定する
    /// </summary>
    public void UpdateRoomId()
    {
        room1Id = 1;
        List<int> possibleRoom2Ids = new List<int> { 1, 1, 2, 2, 2, 3, 3, 4 };
        room2Id = possibleRoom2Ids[Random.Range(0, possibleRoom2Ids.Count)];

        // room1とroom2のセール情報とルームオプションを保存
        SetSaleInfo(room1Id, room1Options, room1InfoText);
        SetSaleInfo(room2Id, room2Options, room2InfoText);

        LoadRoomOptions(room1Id, room1Options, room1InfoText);
        LoadRoomOptions(room2Id, room2Options, room2InfoText);

        // ルームUIの更新
        UpdateRoomUI(room1Id, room1Text);
        UpdateRoomUI(room2Id, room2Text);
    }

    /// <summary>
    /// セール情報を設定し、記録するメソッド
    /// </summary>
    private void SetSaleInfo(int roomId, RoomOptions options, TextMeshProUGUI infoText)
    {
        if (WorldManager.Instance.GetCurrentRoomEvent() > 1 && Random.value <= 0.3f && roomId != 1)
        {
            options.salePercentage = 25;
            infoText.text += "<color=green> 25% Sale! </color>\n";
        }
        else
        {
            options.salePercentage = 0;
        }
    }

    /// <summary>
    /// ルームオプション情報をランダムに設定し、記録するメソッド
    /// </summary>
    private void LoadRoomOptions(int roomId, RoomOptions options, TextMeshProUGUI infoText)
    {
        if (WorldManager.Instance.GetCurrentRoomEvent() > 1 && roomId == 1)
        {
            if (Random.value <= 0.3f)
            {
                int randomOption = Random.Range(1, roomInfoOptions.Count + 1);
                if (randomOption <= 6 || Random.value <= 0.3f)
                {
                    infoText.text += roomInfoOptions[randomOption];
                    SetRoomFlags(options, randomOption);
                }
            }
        }
    }

    /// <summary>
    /// ルームのオプションフラグを設定するメソッド
    /// </summary>
    private void SetRoomFlags(RoomOptions options, int option)
    {
        options.monsterDoubleCount = (option == 1 || option == 2);
        options.itemDropUp = (option == 1 || option == 8);
        options.runeDropUp = (option == 2 || option == 8);
        options.doubleExp = (option == 3 || option == 5 || option == 7);
        options.monsterLevelUp = (option == 3);
        options.monsterHpUp = (option == 4);
        options.doubleGold = (option == 4 || option == 6 || option == 7);
        options.monsterAtkUp = (option == 5);
        options.monsterDefUp = (option == 6);
        options.monsterAddTime = (option == 7 || option == 8);
    }

    /// <summary>
    /// ルームUIのテキストと画像を更新する
    /// </summary>
    private void UpdateRoomUI(int roomId, TextMeshProUGUI roomText)
    {
        switch (roomId)
        {
            case 1:
                roomText.text = "BATTLE ROOM";
                roomText.transform.parent.GetComponent<Image>().sprite = roomImages[0];
                break;
            case 2:
                roomText.text = "SHOP ROOM";
                roomText.transform.parent.GetComponent<Image>().sprite = roomImages[1];
                break;
            case 3:
                roomText.text = "BLACK SMITH ROOM";
                roomText.transform.parent.GetComponent<Image>().sprite = roomImages[2];
                break;
            case 4:
                roomText.text = "RECOVER ROOM";
                roomText.transform.parent.GetComponent<Image>().sprite = roomImages[3];
                break;
        }
    }

    /// <summary>
    /// roomBtn1がクリックされたときの処理
    /// </summary>
    public void OnRoomBtn1Clicked()
    {
        ApplyRoomOptions(room1Options); // 記録されたroom1のオプション情報を適用
        LoadScene(room1Id);
    }

    /// <summary>
    /// roomBtn2がクリックされたときの処理
    /// </summary>
    public void OnRoomBtn2Clicked()
    {
        ApplyRoomOptions(room2Options); // 記録されたroom2のオプション情報を適用
        LoadScene(room2Id);
    }

    /// <summary>
    /// 記録されたルームオプションを適用するメソッド
    /// </summary>
    private void ApplyRoomOptions(RoomOptions options)
    {
        roomOptions.ResetOptions(); // 適用前にリセット
        roomOptions.salePercentage = options.salePercentage;
        roomOptions.monsterDoubleCount = options.monsterDoubleCount;
        roomOptions.doubleGold = options.doubleGold;
        roomOptions.doubleExp = options.doubleExp;
        roomOptions.monsterLevelUp = options.monsterLevelUp;
        roomOptions.monsterHpUp = options.monsterHpUp;
        roomOptions.monsterAtkUp = options.monsterAtkUp;
        roomOptions.monsterDefUp = options.monsterDefUp;
        roomOptions.monsterAddTime = options.monsterAddTime;
        roomOptions.itemDropUp = options.itemDropUp;
        roomOptions.runeDropUp = options.runeDropUp;
    }

    /// <summary>
    /// クリックされたボタンのルームIDに合わせたシーンに遷移するメソッド
    /// </summary>
    public void LoadScene(int roomID)
    {
        switch (roomID)
        {
            case 1:
                gameManager.LoadScene("InToWorldEntrance");
                break;
            case 2:
                gameManager.LoadScene("ShopScene");
                break;
            case 3:
                gameManager.LoadScene("BlackSmithScene");
                break;
            case 4:
                gameManager.LoadScene("RecoverScene");
                break;
        }
    }
}
