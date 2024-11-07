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
    private IventryUI iventryUI; // IventryUIの参照
    [SerializeField] private Sprite[] roomImages; // ルームの画像
    private int room1Id = 1; // ルーム1のID
    [SerializeField] private TextMeshProUGUI room1Text; // ルーム1のテキスト
    private int room2Id = 2; // ルーム2のID
    [SerializeField] private TextMeshProUGUI room2Text; // ルーム2のテキスト
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
        if (gameManager != null)
        {
            // IventryUIのインスタンスを取得
            iventryUI = gameManager.GetComponent<IventryUI>();
        }

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

        if(iventryUI != null)
        {
            // gameManager.LivingUnitsの数分オブジェクトを取得
            for (int i = 0; i < gameManager.livingUnits.Count; i++)
            {
                // 全プレイヤーのSkikkUIを更新する
                iventryUI.UpdateUnitSkillUI(gameManager.livingUnits[i]);
            }
        }

        // 確率でルームIdを変更する
        UpdateRoomId();
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
        // iventryUIがnullでない場合、UpdateUnitSkillUIメソッドを呼び出してスキルUIを更新する
        if(iventryUI != null) iventryUI.UpdateUnitSkillUI(gameManager.livingUnits[playerIndex]);
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
        // iventryUIがnullでない場合、UpdateUnitSkillUIメソッドを呼び出してスキルUIを更新する
        if(iventryUI != null)
        {
            // gameManager.LivingUnitsの数分オブジェクトを取得
            for (int i = 0; i < gameManager.livingUnits.Count; i++)
            {
                // 全プレイヤーのSkikkUIを更新する
                iventryUI.UpdateUnitSkillUI(gameManager.livingUnits[i]);
            }
        }
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
            "   MoveSPD: " + unit.moveSpeed.ToString() + "\n" +
            "" + "\n" +
            "   AttackDelay: " + unit.attackDelay.ToString() + "\n" +
            "   AtkRange: " + unit.attackRange.ToString() + "\n" +
            "   ShootingSPD: " + unit.attackSpeed.ToString() + "\n" +
            "   ShootingLifeTime(s): " + unit.attackLifetime.ToString() + "\n" +
            "   ShootingUnitThrough: " + unit.attackUnitThrough.ToString() + "\n" +
            "   ShootingObjectThrough: " + unit.attackObjectThrough.ToString() + "\n" +
            "   ResistCondition: " + unit.resistCondition.ToString() + "\n" +
            "   KnockBackDistance: " + unit.knockBack.ToString() + "\n\n";
            //"   WeaponScale: " + unit.attackSize.ToString()+ "\n\n";

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

    // ルームidを更新するメソッド
    public void UpdateRoomId()
    {
        // 今のところルームの種類が3つしかないので、ランダムな1~3の整数値を生成してroom1Idに代入
        room1Id = Random.Range(1, 6);
        if (room1Id == 4 || room1Id == 5)
        {
            room1Id = 1;
        }
        // ランダムな1~3の整数値を生成してroom1Idとかぶらない値をroom2Idに代入
        while (room1Id == room2Id)
        {
            room2Id = Random.Range(1, 4);
        }
        switch (room1Id)
        {
            case 1:
                room1Text.text = "BATTLE ROOM";
                room1Text.transform.parent.GetComponent<Image>().sprite = roomImages[0];
                break;
            case 2:
                room1Text.text = "SHOP ROOM";
                room1Text.transform.parent.GetComponent<Image>().sprite = roomImages[1];
                break;
            case 3:
                room1Text.text = "BLACK SMITH ROOM";
                room1Text.transform.parent.GetComponent<Image>().sprite = roomImages[2];
                break;
        }
        switch (room2Id)
        {
            case 1:
                room2Text.text = "BATTLE ROOM";
                room2Text.transform.parent.GetComponent<Image>().sprite = roomImages[0];
                break;
            case 2:
                room2Text.text = "SHOP ROOM";
                room2Text.transform.parent.GetComponent<Image>().sprite = roomImages[1];
                break;
            case 3:
                room2Text.text = "BLACK SMITH ROOM";
                room2Text.transform.parent.GetComponent<Image>().sprite = roomImages[2];
                break;
        }
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
        else if (roomID == 3)
        {
            gameManager.LoadScene("BlackSmithScene");
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
