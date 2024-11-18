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
    }

    //経験値追加ボタンがクリックされたときの処理メソッド
    public void OnAddExpBtnClicked(int index)
    {
        AkSoundEngine.PostEvent("ST_Click", gameObject);
        if (statusLog.currentExp == 0) return;

        int playerIndex = Mathf.CeilToInt(index / 3f) - 1;
        int statusIndex = (index - 1) % 3 + 1;
        int addExp = (int)Mathf.Pow(10, statusIndex - 1);

        Unit unit = gameManager.livingUnits[playerIndex].GetComponent<Unit>();

        if (statusLog.currentExp < addExp)
        {
            addExp = statusLog.currentExp;
        }
        statusLog.currentExp -= addExp;

        unit.totalExp += addExp;
        unit.updateStatus();

        UpdateUI();
        ChangeFaceGraphic();
        if(iventryUI != null) iventryUI.UpdateUnitSkillUI(gameManager.livingUnits[playerIndex]);
    }

    //経験値リセットボタンがクリックされたときの処理メソッド
    public void OnResetExpBtnClicked()
    {
        AkSoundEngine.PostEvent("ST_Click", gameObject);
        statusLog.currentExp = initialStockExp;

        for (int i = 0; i < gameManager.livingUnits.Count; i++)
        {
            Unit unit = gameManager.livingUnits[i].GetComponent<Unit>();
            unit.totalExp = initialTotalExp[i];
            unit.updateStatus();
        }
        UpdateUI();
        ChangeFaceGraphic();
        if(iventryUI != null)
        {
            for (int i = 0; i < gameManager.livingUnits.Count; i++)
            {
                iventryUI.UpdateUnitSkillUI(gameManager.livingUnits[i]);
            }
        }
    }

    // UIの表示を更新するメソッド
    public void UpdateUI()
    {
        for (int i = 0; i < gameManager.livingUnits.Count; i++)
        {
            Unit unit = gameManager.livingUnits[i].GetComponent<Unit>();
            expTexts[i].text = "Lv." + unit.currentLevel.ToString("000") + "   NextLvExp: " + unit.remainingExp.ToString("0000");
            string jobStr = unit.job == 0 ? "Knight" : unit.job == 1 ? "Wizard" : unit.job == 2 ? "Assasin" : unit.job == 3 ? "Healer" : "Tank";
            int hp = (int)unit.Hp;
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
            "   WeaponScale: " + unit.attackSize.ToString() + "\n" +
            "   ShildGurdChance: " + (unit.guardChance * 100).ToString() + "%\n" +
            "   ShootingSPD: " + unit.attackSpeed.ToString() + "\n" +
            "   ShootingLifeTime(s): " + unit.attackLifetime.ToString() + "\n" +
            "   ShootingUnitThrough: " + unit.attackUnitThrough.ToString() + "\n" +
            "   ShootingObjectThrough: " + unit.attackObjectThrough.ToString() + "\n" +
            "   ResistCondition: " + unit.resistCondition.ToString() + "\n" +
            "   KnockBackDistance: " + unit.knockBack.ToString() + "\n" +
            "" + "\n" +
            "   CriticalChance: " + (unit.criticalChance * 100).ToString() + "%\n" +
            "   CriticalDamage: " + unit.criticalDamage.ToString() + " x AttackDamage\n" +
            "   comboDamage: " + (unit.comboDamage * 10).ToString() + "\n" +
            "   comboCritical: " + (unit.comboCriticalCount).ToString() + " x times \n\n";
            if(unit.escape >= 1)
            {
                statusTexts[i].text += "   Escape: true" + "\n";
            }
            if(unit.teleportation >= 1)
            {
                statusTexts[i].text += "   Teleportation: true" + "\n";
            }
        }
        gameManager.UpdateGoldAndExpUI();
    }

    // ジョブによって顔グラフィックを変更するメソッド
    public void ChangeFaceGraphic()
    {
        for (int i = 0; i < faceGraphics.Length; i++)
        {
            faceGraphics[i].sprite = faceGraphicsSprites[gameManager.livingUnits[i].GetComponent<Unit>().job];
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

    // RoomSelectSceneに遷移するボタンを押したときのメソッド(gameManagerから呼び出し)
    public void OnRoomSelectSceneBtnClicked()
    {
        AkSoundEngine.PostEvent("ST_Click", gameObject);
        gameManager.LoadScene("RoomSelectScene");
    }
    
}
