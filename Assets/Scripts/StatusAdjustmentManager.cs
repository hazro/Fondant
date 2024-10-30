using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ステータス調整画面のUIを管理するクラス
/// </summary>
public class StatusAdjustmentManager : MonoBehaviour
{
    private GameManager gameManager; // GameManagerの参照
    private int room1Id = 1; // ルーム1のID
    private int room2Id = 2; // ルーム2のID
    [SerializeField] private StatusLog statusLog;
    [SerializeField] private Image[] faceGraphics; // ジョブによって変更する顔グラフィック
    [SerializeField] private Sprite[] faceGraphicsSprites; // ジョブによって変更する顔グラフィックのスプライト
    [SerializeField] private TextMeshProUGUI[] expTexts; // 経験値表示用のテキスト
    [SerializeField] private TextMeshProUGUI[] statusTexts; // ステータス表示用のテキスト

    // ステータス初期値格納用の変数
    [SerializeField] private int initialStockExp; // 分配前のストック経験値
    [SerializeField] private int[] initialTotalExp; // プレイヤー毎の分配前経験値

    // Start is called before the first frame update
    void Start()
    {
        // GameManagerのインスタンスを取得
        gameManager = GameManager.Instance;

        // ストック経験値表示用のテキストを更新する
        gameManager.UpdateGoldAndExpUI();

        // ステータス初期値を取得
        initialStockExp = statusLog.currentExp; // 分配前のストック経験値を取得
        initialTotalExp = new int[gameManager.livingUnits.Count];
        for (int i = 0; i < gameManager.livingUnits.Count; i++)
        {
            Unit unit = gameManager.livingUnits[i].GetComponent<Unit>();
            initialTotalExp[i] = unit.totalExp; // プレイヤー毎の分配前経験値を取得
            expTexts[i].text = "Lv." + unit.currentLevel.ToString("00") + "   NextLvExp: " + unit.remainingExp.ToString("0000");
        }

        // UIの表示を更新する
        UpdateUI();

        // ジョブによって顔グラフィックを変更するメソッド
        ChangeFaceGraphic();

        // 確率でルームIdを変更する
        // 未実装・・・
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //経験値追加ボタンがクリックされたときの処理メソッド
    public void OnAddExpBtnClicked(int index)
    {
        //　ストック経験値が0の場合は処理を終了
        if (statusLog.currentExp == 0) return;

        // どのプレイヤーか　(index / 3 小数点以下繰り上げ)
        int playerIndex = Mathf.CeilToInt(index / 3f) - 1;
        // 1,10,100単位で経験値を加算する　(index % 3 で割った余り)
        int statusIndex = (index - 1) % 3 + 1;
        int addExp = (int)Mathf.Pow(10, statusIndex - 1);

        Unit unit = gameManager.livingUnits[playerIndex].GetComponent<Unit>();

        // addExpの値分ストック経験値を減算する。　ストック経験値が足りない場合はその分addExpを減らして減算する
        if (statusLog.currentExp < addExp)
        {
            addExp = statusLog.currentExp;
        }
        statusLog.currentExp -= addExp;

        // playerIndexのプレイヤーの経験値をaddExp加算する
        unit.totalExp += addExp;
        // UpdateStatusメソッドを呼び出してステータスを更新する
        unit.updateStatus();

        // UIの表示を更新する
        UpdateUI();
        ChangeFaceGraphic();
    }

    //経験値リセットボタンがクリックされたときの処理メソッド
    public void OnResetExpBtnClicked()
    {
        // ストック経験値を初期値に戻す
        statusLog.currentExp = initialStockExp;

        // プレイヤー毎の経験値を初期値に戻す
        for (int i = 0; i < gameManager.livingUnits.Count; i++)
        {
            Unit unit = gameManager.livingUnits[i].GetComponent<Unit>();
            unit.totalExp = initialTotalExp[i];
            unit.updateStatus();
        }
        // UIの表示を更新する
        UpdateUI();
        ChangeFaceGraphic();
    }

    // UIの表示を更新するメソッド
    public void UpdateUI()
    {
        // player毎のステータス表示用のテキストを更新する
        for (int i = 0; i < gameManager.livingUnits.Count; i++)
        {
            Unit unit = gameManager.livingUnits[i].GetComponent<Unit>();
            // expTextsの表示を更新する
            expTexts[i].text = "Lv." + unit.currentLevel.ToString("000") + "   NextLvExp: " + unit.remainingExp.ToString("0000");
            // string jobStr が　unit.job=0の場合は"Knight"、unit.job=1の場合は"Wizard"、unit.job=2の場合は"Assasin"、unit.job=3の場合は"Healer"、unit.job=4の場合は"Tank"を代入
            string jobStr = unit.job == 0 ? "Knight" : unit.job == 1 ? "Wizard" : unit.job == 2 ? "Assasin" : unit.job == 3 ? "Healer" : "Tank";
            int hp = (int)unit.Hp;
            // statusTextsの表示を更新する
            statusTexts[i].text = 
            "   Job: " + jobStr + "\n" +
            "   HP: " + hp + "\n\n" +
            "   ATK: " + unit.physicalAttackPower.ToString() + "\n" +
            "   DEF: " + unit.physicalDefensePower.ToString() + "\n" +
            "   MATK: " + unit.magicalAttackPower.ToString() + "\n" +
            "   MDEF: " + unit.magicalDefensePower.ToString() + "\n" +
            "   SPD: " + unit.Speed.ToString() + "\n" +
            "   Delay: " + unit.attackDelay.ToString() + "\n" +
            "   ResistCondition: " + unit.resistCondition.ToString() + "%\n" +
            "   KnockBack: " + unit.knockBack.ToString() + "\n" +
            "   AtkRange: " + unit.attackRange.ToString() + "\n" +
            "   WeaponScale: " + unit.attackSize.ToString()+ "\n" +
            "   AtkUnitThrough: " + unit.attackUnitThrough.ToString() + "\n" +
            "   AtkObjectThrough: " + unit.attackObjectThrough.ToString() + "\n\n";
            // unit.escapeが1以上の場合は文字列"true"を追加
            if(unit.escape >= 1)
            {
                statusTexts[i].text += "   Escape: true" + "\n";
            }
            // unit.teleportationが1以上の場合は文字列"true"を追加
            if(unit.teleportation >= 1)
            {
                statusTexts[i].text += "   Teleportation: true" + "\n";
            }
        }
        // ストック経験値表示用のテキストを更新する
        gameManager.UpdateGoldAndExpUI();
    }

    // roomBtn1がクリックされたときの処理
    public void OnRoomBtn1Clicked()
    {
        // room1Idが1の場合、バトルルームに遷移
        LoadScene(room1Id);
    }

    // roomBtn2がクリックされたときの処理
    public void OnRoomBtn2Clicked()
    {
        // room2Idが2の場合、ショップに遷移
        LoadScene(room2Id);
    }

    // クリックされたボタンのルームIDに合わせたシーンに遷移するメソッド
    public void LoadScene(int roomID)
    {
        if (roomID == 1)
        {
            gameManager.LoadScene("InToWorldEntrance");
        }
        else if (roomID == 2)
        {
            gameManager.LoadScene("ShopScene");
        }
    }

    // ジョブによって顔グラフィックを変更するメソッド
    public void ChangeFaceGraphic()
    {
        for (int i = 0; i < faceGraphics.Length; i++)
        {
            // ジョブによって顔グラフィックを変更
            faceGraphics[i].sprite = faceGraphicsSprites[gameManager.livingUnits[i].GetComponent<Unit>().job];
            // livingUnits[i].SetActive(false)の場合死亡しているので、顔グラフィックを黒くする
            if (!gameManager.livingUnits[i].activeSelf)
            {
                faceGraphics[i].color = new Color(0.3f, 0.15f, 0.15f, 1);
            }
            else
            {
                faceGraphics[i].color = new Color(1, 1, 1, 1);
            }
        }
    }
}
