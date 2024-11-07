using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace MyGame.Managers
{
public class BlackSmithManager : MonoBehaviour
{
    [SerializeField] private GameObject OriginalRuneImage;
    [SerializeField] private GameObject LvUpRuneImage;

    [Header("Rune Lv Text")]
    [SerializeField] private TextMeshProUGUI OriginalLvText;
    [SerializeField] private TextMeshProUGUI LvUpLvText;

    [Header("Rune Lv Display")]
    [SerializeField] private Image Lv1DisplayWaku;
    [SerializeField] private Image Lv2DisplayWaku;
    [SerializeField] private Image Lv3DisplayWaku;
    [SerializeField] private TextMeshProUGUI Lv1DisplayText;
    [SerializeField] private TextMeshProUGUI Lv2DisplayText;
    [SerializeField] private TextMeshProUGUI Lv3DisplayText;

    [Header("Rune Lv Arrow")]
    [SerializeField] private Image Lv1Arrow;
    [SerializeField] private Image Lv2Arrow;
    
    [Header("Button")]
    [SerializeField] private TextMeshProUGUI PriceText;

    [Header("InfoPanel")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject CancelButton;
    [SerializeField] private TextMeshProUGUI infoText;

    [Header("SetItem")]
    public bool isSetItem; // 強化するアイテムがセットされているかどうかを判定する変数
    public GameObject setItem; // 強化するアイテムを格納する変数
    public int setItemID; // 強化するアイテムのIDを格納する変数
    public int setItemLv; // 強化するアイテムのレベルを格納する変数
    public int price; // 強化するアイテムの価格を格納する変数
    private ItemData.RuneListData runeListData; // 強化するアイテムのデータを格納する変数
    private GameManager gameManager; // GameManagerの参照
    private IventryUI iventryUI; // IventryUIの参照
    private Material orgMaterialInstance; // Materialのインスタンス1
    private Material materialInstance; // Materialのインスタンス2
    private Image lvUpImageComponent; // LvUpRuneImageのImageコンポーネント
    private Image orgImageComponent; // OriginalRuneImageのImageコンポーネント
    public StatusLog statusLog; // ステータスログのインスタンスを取得するためのフィールド

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance; // GameManagerのインスタンスを取得
        if (gameManager != null)
        {
            // IventryUIのインスタンスを取得
            iventryUI = gameManager.GetComponent<IventryUI>();
            // ステータスインフォメーションの更新
            gameManager.UpdateGoldAndExpUI();
        }
        // マテリアルをコピーしてインスタンスを生成
        orgImageComponent = OriginalRuneImage.GetComponent<Image>();
        orgMaterialInstance = new Material(orgImageComponent.material);
        lvUpImageComponent = LvUpRuneImage.GetComponent<Image>();
        materialInstance = new Material(lvUpImageComponent.material);
        OnResetButtonClicked();
    }

    /// <summary>
    /// 強化するアイテムをセットする関数です。
    /// </summary>
    public void SetItem()
    {
        if(!isSetItem) return;
        // setItemIDに該当するPrefabリソースをロードする
        GameObject setItemPrefab = Resources.Load<GameObject>("Prefabs/Runes/" + setItemID);
        // OriginalRuneImageとLvUpRuneImageをアクティブにしてsetItemPrefabにspriteRendererがあったらspriteをセットする
        OriginalRuneImage.SetActive(true);
        LvUpRuneImage.SetActive(true);
        if(setItemPrefab.GetComponent<SpriteRenderer>() != null)
        {
            orgImageComponent.sprite = setItemPrefab.GetComponent<SpriteRenderer>().sprite;
            lvUpImageComponent.sprite = setItemPrefab.GetComponent<SpriteRenderer>().sprite;
        }
        else
        {
            orgImageComponent.sprite = setItemPrefab.GetComponent<Image>().sprite;
            lvUpImageComponent.sprite = setItemPrefab.GetComponent<Image>().sprite;
        }
        // gameManagerのRuneDataからsetItemIDに該当するRuneDataを取得する
        //List<ItemData.RuneListData> runeData = gameManager.itemData.runeList[setItemID];
        runeListData = gameManager.itemData.runeList.Find(x => x.ID == setItemID);

        OriginalLvText.text = "Lv." + setItemLv;
        OriginalRuneImage.GetComponent<ItemDandDHandler>().runeLevel = setItemLv;
        if(runeListData.maxLevel == setItemLv)
        {
            LvUpLvText.text = "Lv." + setItemLv + " (Max)";
            OriginalLvText.text = "Lv." + setItemLv + " (Max)";
            LvUpRuneImage.GetComponent<ItemDandDHandler>().runeLevel = setItemLv;
        }
        else if(runeListData.maxLevel == setItemLv+1)
        {
            LvUpLvText.text = "Lv." + (setItemLv + 1) + " (Max)";
            LvUpRuneImage.GetComponent<ItemDandDHandler>().runeLevel = setItemLv + 1;
        }
        else
        {
            LvUpLvText.text = "Lv." + (setItemLv + 1);
            LvUpRuneImage.GetComponent<ItemDandDHandler>().runeLevel = setItemLv + 1;
        }
        price = runeListData.price * 2 * (setItemLv);
        PriceText.text = "Price: " + price;

        // MaterialインスタンスにitemのruneLevelを設定
        orgMaterialInstance.SetFloat("_Lv", setItemLv);
        orgImageComponent.material = orgMaterialInstance;
        materialInstance.SetFloat("_Lv", LvUpRuneImage.GetComponent<ItemDandDHandler>().runeLevel);
        lvUpImageComponent.material = materialInstance;

        // 手持ちのお金が足りない場合はtextを赤くする
        if(statusLog.currentGold < price)
        {
            PriceText.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }
        else
        {
            PriceText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }

        Lv1DisplayText.text = GetRuneInfo(1);
        // もしmaxLevelが1の場合はLv2DisplayText.textとLv3DisplayText.textには何も表示しない、maxLevelが2の場合はLv3DisplayText.textには何も表示しない
        if(runeListData.maxLevel == 1)
        {
            Lv2DisplayText.text = "";
            Lv3DisplayText.text = "";
        }
        else if(runeListData.maxLevel == 2)
        {
            Lv2DisplayText.text = GetRuneInfo(2);
            Lv3DisplayText.text = "";
        }
        else
        {
            Lv2DisplayText.text = GetRuneInfo(2);
            Lv3DisplayText.text = GetRuneInfo(3);
        }

        // setItemLv==1の場合Lv1DisplayWakuの色を青くする、setItemLv==2の場合Lv2DisplayWakuの色を青くする、setItemLv==3の場合Lv3DisplayWakuの色を青くする、それ以外は灰色にする
        // 色の定義
        Color defaultColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        Color blueColor = new Color(0.25f, 0.25f, 0.8f, 0.5f);
        Color GreenColor = new Color(0.25f, 0.8f, 0.25f, 0.5f);

        if(setItemLv == 1)
        {
            Lv1DisplayWaku.color = blueColor;
            if(runeListData.maxLevel > 1)
            {
                Lv2DisplayWaku.color = GreenColor;
                Lv1Arrow.enabled = true;
                Lv2Arrow.enabled = false;
            }
            else
            {
                Lv2DisplayWaku.color = defaultColor;
            }
            Lv3DisplayWaku.color = defaultColor;
        }
        else if(setItemLv == 2)
        {
            Lv1DisplayWaku.color = defaultColor;
            Lv2DisplayWaku.color = blueColor;
            if(runeListData.maxLevel > 2)
            {
                Lv3DisplayWaku.color = GreenColor;
                Lv1Arrow.enabled = false;
                Lv2Arrow.enabled = true;
            }
            else
            {
                Lv3DisplayWaku.color = defaultColor;
            }
        }
        else if(setItemLv == 3)
        {
            Lv1DisplayWaku.color = defaultColor;
            Lv2DisplayWaku.color = defaultColor;
            Lv3DisplayWaku.color = blueColor;
        }
        else
        {
            Lv1DisplayWaku.color = defaultColor;
            Lv2DisplayWaku.color = defaultColor;
            Lv3DisplayWaku.color = defaultColor;
        }

    }

    /// <summary>
    /// レベルアップボタンが押されたときに呼び出される関数です。
    /// </summary>
    public void OnLvUpButtonClicked()
    {
        infoPanel.SetActive(true);
        if(OriginalLvText.text.Contains("Max"))
        {
            // 黄色で表示
            infoText.text = "<color=yellow>This rune is the maximum level.</color>";
        }
        else if(statusLog.currentGold < price)
        {
            // 赤で表示
            infoText.text = "<color=red>Not enough gold.</color>";
        }
        else
        {
            infoText.text = "Do you want to enhance this rune?\n" + PriceText.text;
            CancelButton.SetActive(true);
        }
    }

    /// <summary>
    /// リセットボタンが押されたときに呼び出される関数です。
    /// </summary>
    public void OnResetButtonClicked()
    {
        infoPanel.SetActive(false);
        CancelButton.SetActive(false);
        OriginalRuneImage.SetActive(false);
        LvUpRuneImage.SetActive(false);
        isSetItem = false; // 強化するアイテムがセットされているかどうかを判定する変数ItemDandDHandlerのアイテム移動にも影響する
        setItem = null;
        setItemID = 0;
        setItemLv = 0;
        OriginalLvText.text = "";
        LvUpLvText.text = "";
        Lv1DisplayWaku.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        Lv2DisplayWaku.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        Lv3DisplayWaku.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        Lv1DisplayText.text = "";
        Lv2DisplayText.text = "";
        Lv3DisplayText.text = "";
        PriceText.text = "Price: 0";
        PriceText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        Lv1Arrow.enabled = false;
        Lv2Arrow.enabled = false;
    }

    /// <summary>
    /// Exitボタンが押されたときに呼び出される関数です。
    /// </summary>
    public void OnExitButtonClicked()
    {
        OnResetButtonClicked();
        // StatusAdjustmentSceneに遷移
        gameManager.LoadScene("StatusAdjustmentScene");
    }

    /// <summary>
    /// OKボタンが押されたときに呼び出される関数です。
    /// </summary>
    public void OnOKButtonClicked()
    {
        // 手持ちのお金が足りていたら強化処理を行う
        if(statusLog.currentGold >= price && !OriginalLvText.text.Contains("Max"))
        {
            Debug.Log("Rune Level Up!");
            // お金を減らす
            Debug.Log("currentGold: " + statusLog.currentGold + " - " + price + " = " + (statusLog.currentGold - price));
            statusLog.currentGold -= price;
            // お金を減らしたことをUIに反映
            gameManager.UpdateGoldAndExpUI();
            // 元のアイテムのruneLevelを上げる
            setItem.GetComponent<ItemDandDHandler>().runeLevel++;
            Material originalMat;
            if(setItem.GetComponent<SpriteRenderer>() != null)
            {
                originalMat = setItem.GetComponent<SpriteRenderer>().material;
                // setItemの親がIventryPanelの何番目の子要素かを取得
                int index = setItem.transform.parent.GetSiblingIndex();
                iventryUI.IventryItem[index] = setItemID * 10 + setItem.GetComponent<ItemDandDHandler>().runeLevel;
            }
            else
            {
                originalMat = setItem.GetComponent<Image>().material;
            }
            // Materialをinstance化してitemのruneLevelを設定
            Material setMaterialInstance = new Material(originalMat);
            originalMat = setMaterialInstance;
            originalMat.SetFloat("_Lv", setItem.GetComponent<ItemDandDHandler>().runeLevel);
            if(iventryUI != null)
            {
                // イベントリの更新
                iventryUI.UpdateIventryPanel();
                // 全キャラのスキルパネルを更新し、ステータスを更新
                if(gameManager.livingUnits != null)
                {
                    foreach(GameObject unit in gameManager.livingUnits)
                    {

                        iventryUI.UpdateUnitSkillUI(unit);
                        unit.GetComponent<Unit>().updateStatus();
                    }
                }
            }
            OnResetButtonClicked();
        }
        infoPanel.SetActive(false);
    }

    /// <summary>
    /// Cancelボタンが押されたときに呼び出される関数です。
    /// </summary>
    public void OnCancelButtonClicked()
    {
        infoPanel.SetActive(false);
    }

    /// <summary>
    /// runeListとruneLevelを渡して、対応するレベルのRuneの情報をstringで返す関数です。
    /// </summary>
    public string GetRuneInfo(int runeLevel)
    {
        if(runeListData == null) return "runeListData is none.";
        string text = "";

        // ルーンの情報を取得
        int lineCount = 0;
        foreach (var field in runeListData.GetType().GetFields())
        {
            // 値が0でない場合のみ表示 (IDは除く)
            if (field.GetValue(runeListData) != null && field.GetValue(runeListData).ToString() != "0" && field.Name != "ID")
            {
                // 2行目はRune Levelを表示
                if (lineCount == 1)
                {
                    text += "runeLevel: " + runeLevel + "\n";
                    lineCount++;
                }
                // 変数名の末尾に"Lv１桁の整数"がつく場合"整数"の値がruneLevelと一致する場合のみ表示
                if (field.Name.Substring(field.Name.Length - 3) == "Lv" + runeLevel)
                {
                    // field.Nameの文字列から"Lv" + runeLevelを削除して表示
                    text += field.Name.Substring(0, field.Name.Length - 3) + ": " + field.GetValue(runeListData) + "\n";
                    lineCount++;
                }
                // field.Name.Length -1~-3がLvでない場合、そのまま表示
                else if (field.Name.Substring(field.Name.Length - 3, 2) != "Lv")
                {
                    text += field.Name + ": " + field.GetValue(runeListData) + "\n";
                    lineCount++;
                }
            }
        }

        // テキストを成形して返す
        return SetActiveAndChangeText(text);
    }

    /// <summary>
    /// ルーンごとの情報textを成形して返す関数です。
    /// </summary>
    public string SetActiveAndChangeText(string text)
    {
        // テキストを行ごとに分割して処理する
        string[] lines = text.Split('\n'); // 改行で各行を分割
        List<string> processedLines = new List<string>();
        string abilityLine = null; // abilityで始まる行を保存するための変数

        for (int i = 0; i < lines.Length; i++)
        {
            // 最初の行がnameで始まる場合、白字＆太字にする
            if (lines[i].StartsWith("name"))
            {
                // 最初の「:」までを削除して内容を取得
                lines[i] = lines[i].Substring(lines[i].IndexOf(":") + 1);
                lines[i] = "<size=+2><color=white><b>" + lines[i] + "</b></color></size>\n";
                processedLines.Add(lines[i]);
            }
            // abilityで始まる行は後で黒字にして最後に移動
            else if (lines[i].StartsWith("ability"))
            {
                abilityLine = "<color=black>" + lines[i] + "</color>\n"; // 黒字にして保存
            }
            //worldで始まる行は飛ばす
            else if (lines[i].StartsWith("world"))
            {
                continue;
            }
            else
            {
                // 他の行は通常の黒字に設定
                processedLines.Add("<color=black>" + lines[i] + "</color>");
            }
        }

        // abilityで始まる行が存在した場合、テキストの最後に移動
        if (abilityLine != null)
        {
            processedLines.Add(abilityLine);
        }

        // 行を結合して最終的なテキストとして返す
        return string.Join("\n", processedLines);
    }
}
}